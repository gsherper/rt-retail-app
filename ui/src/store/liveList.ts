import type { HubClient } from '../lib/hub/HubClient'
import { invokeHub } from './hub'
import type { ChangeEvent } from './hub'

interface LiveListOptions<T, A> {
  hub: HubClient
  name: string
  subscribeMethod: string
  unsubscribeMethod: string

  methodArgs?: (arg: A) => unknown[]
  changedEvent: string
  getId: (row: T) => number
  belongs?: (row: T, arg: A) => boolean
}

interface SyncLifecycle<T> {
  //Run a recipe against the cached list
  update: (recipe: (rows: T[]) => T[] | void) => void
  cacheDataLoaded: Promise<unknown>
  cacheEntryRemoved: Promise<unknown>
}

export function createLiveList<T extends { version: number }, A = void>({
  hub,
  name,
  subscribeMethod,
  unsubscribeMethod,
  methodArgs,
  changedEvent,
  getId,
  belongs,
}: LiveListOptions<T, A>) {
  const argsFor = (arg: A) => methodArgs?.(arg) ?? []

  const subscribe = (arg: A) =>
    invokeHub<T[]>(
      hub,
      { method: subscribeMethod, args: argsFor(arg) },
      `Could not load ${name}`,
    )

  // Update only if version number isn't older.
  // deleted: deleted row ID → the version it was deleted at.
  // Events can arrive out of order, example: an update published just before a
  // delete but delivered after it...that update would add the
  // deleted row back
  function applyChange(
    rows: T[],
    change: ChangeEvent<T>,
    arg: A,
    deleted: Map<number, number>,
  ) {
    const index = rows.findIndex((row) => getId(row) === change.id)
    switch (change.type) {
      case 'created':
      case 'updated': {
        if (belongs && !belongs(change.data, arg)) break
        // Older than (or the same as) the row's delete: it's stale, drop it
        const deletedAt = deleted.get(change.id)
        if (deletedAt !== undefined && change.version <= deletedAt) break
        if (index === -1) rows.push(change.data)
        else if (change.version > rows[index].version) rows[index] = change.data
        break
      }
      case 'deleted':
        deleted.set(
          change.id,
          Math.max(change.version, deleted.get(change.id) ?? change.version),
        )
        if (index !== -1) rows.splice(index, 1)
        break
      default:
        // Unknown change type from server: ignore it
        break
    }
  }

  return {
    queryFn: subscribe,

    async sync(
      arg: A,
      { update, cacheDataLoaded, cacheEntryRemoved }: SyncLifecycle<T>,
    ) {
      // After a reconnect the server has forgotten the subscription:
      // subscribe again and replace the list with the one returned
      const resync = async () => {
        const result = await subscribe(arg)
        if ('data' in result) update(() => result.data)
        // The next reconnect triggers another resync, so just log
        else console.warn(`${name} resync failed`, result.error.data)
      }

      // Listen before the first load finishes so nothing is missed, but hold
      // changes until there's data to apply them to
      const pending: ChangeEvent<T>[] = []
      let loaded = false
      // Rows deleted while this list is in use; see applyChange
      const deleted = new Map<number, number>()

      const apply = (change: ChangeEvent<T>) =>
        update((rows) => applyChange(rows, change, arg, deleted))

      // clean up listeners list
      const removeListeners = [
        hub.on(changedEvent, (change: ChangeEvent<T>) =>
          loaded ? apply(change) : pending.push(change),
        ),
        hub.onReconnected(() => {
          if (loaded) void resync()
        }),
      ]

      try {
        // Pause until cacheDataLoaded resolved by RTK Query after the data has loaded
        await cacheDataLoaded
        loaded = true
        pending.forEach(apply)
      } catch {}

      // Pause until cacheEntryRemoved resolved by RTK Query
      await cacheEntryRemoved
      // Stop handling the topic's events here, then stop the server sending them
      removeListeners.forEach((remove) => remove())
      hub.invoke(unsubscribeMethod, ...argsFor(arg)).catch(() => {})
    },
  }
}
