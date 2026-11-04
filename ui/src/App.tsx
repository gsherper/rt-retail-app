import { Layout } from 'antd'
import { Navigate, Route, Routes } from 'react-router'
import { AppHeader } from './components/AppHeader'
import { AppFooter } from './components/AppFooter'
import { CustomerList } from './pages/CustomerList'
import { Orders } from './pages/Orders'
import styles from './App.module.css'

export function App() {
  return (
    <Layout className={styles.layout}>
      <AppHeader />
      <Layout.Content className={styles.content}>
        <Routes>
          <Route path="/" element={<CustomerList />} />
          <Route path="/orders" element={<Orders />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Layout.Content>
      <AppFooter />
    </Layout>
  )
}
