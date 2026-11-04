import { configureStore } from '@reduxjs/toolkit'
import { api } from './api'
import { dashboardHub } from './hubConnection'
import { hubStatusChanged, hubStatusReducer } from './hubStatusSlice'

export const store = configureStore({
  reducer: {
    [api.reducerPath]: api.reducer,
    hubStatus: hubStatusReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(api.middleware),
})

// Mirror the WebSocket connection status into the store for the status bar
dashboardHub.onStatusChange((status) =>
  store.dispatch(hubStatusChanged(status)),
)

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
