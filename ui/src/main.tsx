import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App as AntdApp, ConfigProvider, theme as antdTheme } from 'antd'
import type { ThemeConfig } from 'antd'
import {
  AllCommunityModule,
  ModuleRegistry,
  provideGlobalGridOptions,
  themeQuartz,
} from 'ag-grid-community'
import '@fontsource-variable/inter'
import './index.css'
import { Provider } from 'react-redux'
import { BrowserRouter } from 'react-router'
import { App } from './App.tsx'
import { store } from './store'

const fontFamily = "'Inter Variable', system-ui, 'Segoe UI', Roboto, sans-serif"
const fontSize = 12

ModuleRegistry.registerModules([AllCommunityModule])
provideGlobalGridOptions({
  theme: themeQuartz.withParams({
    fontFamily,
    fontSize,
    oddRowBackgroundColor: '#f5f7fa',
    rowHeight: 28,
    valueChangeValueHighlightBackgroundColor: 'rgba(19, 194, 194, 0.35)',
    headerColumnResizeHandleHeight: '75%',
  }),
})

const theme: ThemeConfig = {
  algorithm: [antdTheme.defaultAlgorithm, antdTheme.compactAlgorithm],
  token: { fontFamily, fontSize: fontSize + 2, colorPrimary: '#0E4491' },
  components: {
    Form: { itemMarginBottom: 10, verticalLabelPadding: '0 0 2px' },
    Layout: {
      headerBg: '#0E4491',
      headerPadding: '0 24px',
      headerColor: '#fff',
      headerHeight: 36,
      footerPadding: '8px 24px',
    },
    Badge: { statusSize: 12 },
  },
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider store={store}>
      <BrowserRouter>
        <ConfigProvider theme={theme}>
          <AntdApp>
            <App />
          </AntdApp>
        </ConfigProvider>
      </BrowserRouter>
    </Provider>
  </StrictMode>,
)
