import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App as AntdApp, ConfigProvider } from 'antd'
import {
  AllCommunityModule,
  ModuleRegistry,
  provideGlobalGridOptions,
  themeQuartz,
} from 'ag-grid-community'
import '@fontsource-variable/inter'
import './index.css'
import App from './App.tsx'

const fontFamily = "'Inter Variable', system-ui, 'Segoe UI', Roboto, sans-serif"
const fontSize = 12

ModuleRegistry.registerModules([AllCommunityModule])
provideGlobalGridOptions({
  theme: themeQuartz.withParams({
    fontFamily,
    fontSize,
    oddRowBackgroundColor: '#f5f7fa',
    rowHeight: 28,
    headerHeight: 32,
  }),
})

const theme = {
  token: { fontFamily, fontSize, colorPrimary: '#000E24' },
  components: {
    Layout: {
      headerBg: '#000E24',
      headerPadding: '0 24px',
      headerColor: '#fff',
      headerHeight: 36,
      footerPadding: '8px 24px',
    },
  },
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ConfigProvider theme={theme}>
      <AntdApp>
        <App />
      </AntdApp>
    </ConfigProvider>
  </StrictMode>,
)
