import { HUB_RETRY_DELAY_MS } from '../config/appConfig'
import { HubClient } from '../lib/hub/HubClient'

// one SignalR connection for all topics
export const dashboardHub = new HubClient('/hub', HUB_RETRY_DELAY_MS)
