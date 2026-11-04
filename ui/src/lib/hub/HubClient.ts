import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import type { HubConnection } from '@microsoft/signalr'

export type ConnectionStatus = 'connected' | 'connecting' | 'disconnected'

type Listener<T> = (value: T) => void

const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms))

// SingalR connection wrapper with
// custom retry logic - forever retry and wait to retry
export class HubClient {
  private readonly connection: HubConnection
  // retryDelayMs defines how long wait after a failed connection attempt before trying again
  private readonly retryDelayMs: number
  private startPromise: Promise<void> | null = null

  private stopped = false
  private status: ConnectionStatus = 'disconnected'
  private readonly statusListeners = new Set<Listener<ConnectionStatus>>()
  private readonly reconnectedListeners = new Set<() => void>()

  constructor(url: string, retryDelayMs: number) {
    this.retryDelayMs = retryDelayMs
    this.connection = new HubConnectionBuilder()
      .withUrl(url)
      .configureLogging(LogLevel.Warning)
      .build()

    this.connection.onclose(() => {
      this.setStatus('disconnected')
      this.startPromise = this.stopped
        ? null
        : this.connectWithRetry({ isReconnect: true })
    })
  }

  getStatus(): ConnectionStatus {
    return this.status
  }

  // Connects if not already connected or connecting. This resolves when connected
  start(): Promise<void> {
    this.stopped = false
    this.startPromise ??= this.connectWithRetry({ isReconnect: false })
    return this.startPromise
  }

  // Closes the connection and stops reconnecting
  async stop(): Promise<void> {
    this.stopped = true
    this.startPromise = null
    await this.connection.stop()
  }

  // Calls a hub method
  async invoke<T>(method: string, ...args: unknown[]): Promise<T> {
    await this.start()
    return this.connection.invoke<T>(method, ...args)
  }

  // Listens for a server-to-client method. Returns an unsubscribe function
  on<Args extends unknown[]>(
    method: string,
    handler: (...args: Args) => void,
  ): () => void {
    const callback = handler as (...args: unknown[]) => void
    this.connection.on(method, callback)
    return () => this.connection.off(method, callback)
  }

  // Add connection status listener and provide return function to remove listener
  onStatusChange(listener: Listener<ConnectionStatus>): () => void {
    this.statusListeners.add(listener)
    listener(this.status)
    return () => this.statusListeners.delete(listener)
  }

  // Add reconnected listener when connection dropped on reconnected.
  // This is used to renew any existing subscriptions
  onReconnected(listener: () => void): () => void {
    this.reconnectedListeners.add(listener)
    return () => this.reconnectedListeners.delete(listener)
  }

  private async connectWithRetry({
    isReconnect,
  }: {
    isReconnect: boolean
  }): Promise<void> {
    for (let attempt = 1; ; attempt++) {
      if (this.stopped) return

      this.setStatus('connecting')
      try {
        await this.connection.start()
        this.setStatus('connected')
        if (isReconnect) {
          this.reconnectedListeners.forEach((listener) => listener())
        }
        return
      } catch (error) {
        console.warn(`WebSocket connection failed (attempt ${attempt})`, error)
        this.setStatus('disconnected')
        await wait(this.retryDelayMs)
      }
    }
  }

  private setStatus(status: ConnectionStatus) {
    if (status === this.status) return
    this.status = status
    this.statusListeners.forEach((listener) => listener(status))
  }
}
