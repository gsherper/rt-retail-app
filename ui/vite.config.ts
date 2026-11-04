import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The .NET server (server/Properties/launchSettings.json)
const apiTarget = 'http://localhost:5000'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Same-origin in dev, so the app can use a relative URL. Only the hub:
    // the UI never calls the server's REST API (/api is for debugging)
    proxy: {
      // ws: true also proxies the WebSocket upgrade, not just /hub/negotiate
      '/hub': { target: apiTarget, ws: true },
    },
  },
})
