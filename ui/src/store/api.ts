import { createApi } from '@reduxjs/toolkit/query/react'
import { hubBaseQuery } from './hub'

// application RTK Query API, queries and mutations are injected
// later by store domain apis
export const api = createApi({
  reducerPath: 'api',
  baseQuery: hubBaseQuery,
  endpoints: () => ({}),
})
