// App-wide settings

// Used as the default sales person a new order starts with (employee 127);
// the user can change to another sales person in the order form
export const DEFAULT_SALES_PERSON_ID = 127

// This retry delay is in place to show the disconnected status (red) in the
// footer for this demo, otherwise it will always be in connecting status (orange)
// due to continuous reconnect attempts
export const HUB_RETRY_DELAY_MS = 2_000
