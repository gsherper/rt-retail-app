import { createSlice } from '@reduxjs/toolkit'
import type { PayloadAction } from '@reduxjs/toolkit'
import type { ConnectionStatus } from '../lib/hub/HubClient'
import type { RootState } from './index'

/** Status of the tab's one hub connection (store/hubConnection.ts), shared by every topic */
interface HubStatusState {
  status: ConnectionStatus
}

const initialState: HubStatusState = {
  status: 'disconnected',
}

const hubStatusSlice = createSlice({
  name: 'hubStatus',
  initialState,
  reducers: {
    hubStatusChanged(state, action: PayloadAction<ConnectionStatus>) {
      state.status = action.payload
    },
  },
})

export const { hubStatusChanged } = hubStatusSlice.actions

export const selectHubStatus = (state: RootState) => state.hubStatus.status

export const hubStatusReducer = hubStatusSlice.reducer
