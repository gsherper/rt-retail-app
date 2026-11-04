import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The .NET server (server/Properties/launchSettings.json)
const apiTarget = 'http://localhost:5000'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Same-origin requests in dev, so the app can use relative URLs
    proxy: {
      '/api': apiTarget,
      '/ws': { target: apiTarget, ws: true },
    },
  },
})
