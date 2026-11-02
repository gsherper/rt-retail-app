import { Layout } from 'antd'
import AppHeader from './components/AppHeader'
import AppFooter from './components/AppFooter'
import CustomerList from './pages/CustomerList'

function App() {
  return (
    <Layout className="app-layout">
      <AppHeader />
      <Layout.Content className="app-content">
        <CustomerList />
      </Layout.Content>
      <AppFooter />
    </Layout>
  )
}

export default App
