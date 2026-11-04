import type { BaseQueryFn } from '@reduxjs/toolkit/query/react'
import type { HubClient } from '../lib/hub/HubClient'
import { dashboardHub } from './hubConnection'

// the server HubError
export interface HubError {
  status: number
  title: string
  detail?: string | null
  errors?: Record<string, string[]> | null
}

// the server HubResult
export interface HubResult<T> {
  ok: boolean
  data?: T | null
  error: HubError | null
}

export type ChangeEvent<T> =
  | { type: 'created' | 'updated'; id: number; version: number; data: T }
  | { type: 'deleted'; id: number; version: number; data: null }

// A hub method call and args
export interface HubCall {
  method: string
  args?: unknown[]
}

// RTK Query Error type
export interface HubQueryError {
  status: number
  data: HubError
}

// Client side only errors
const clientError = (title: string): HubQueryError => ({
  status: 0,
  data: { status: 0, title, detail: title },
})

// Wrapper to call Hub method and handle response
export async function invokeHub<T>(
  hub: HubClient,
  { method, args = [] }: HubCall,
  failure: string,
): Promise<{ data: T } | { error: HubQueryError }> {
  try {
    const result = await hub.invoke<HubResult<T>>(method, ...args)
    if (result.ok) return { data: (result.data ?? null) as T }
    return result.error
      ? { error: { status: result.error.status, data: result.error } }
      : { error: clientError(failure) }
  } catch (error) {
    console.warn(`${method} failed`, error)
    return { error: clientError(failure) }
  }
}

export const hubBaseQuery: BaseQueryFn<HubCall, unknown, HubQueryError> = (
  hubMethodAndParams,
) => {
  //Handle no connection, nothing will be sent to server if there is no connection
  if (dashboardHub.getStatus() !== 'connected') {
    return { error: clientError('Not connected to the server; try again') }
  }
  return invokeHub(
    dashboardHub,
    hubMethodAndParams,
    `${hubMethodAndParams.method} failed`,
  )
}
