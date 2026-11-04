import { Layout } from 'antd'
import { GithubOutlined } from '@ant-design/icons'
import { NavLink } from 'react-router'
import styles from './AppHeader.module.css'

const { Header } = Layout

const REPO_URL = 'https://github.com/gsherper/rt-retail-app'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  isActive ? `${styles.navLink} ${styles.active}` : styles.navLink

export function AppHeader() {
  return (
    <Header className={styles.header}>
      <span className={styles.title}>Realtime Customer Management Demo</span>
      <nav className={styles.nav}>
        <NavLink to="/" end className={navLinkClass}>
          Customers
        </NavLink>
        <NavLink to="/orders" className={navLinkClass}>
          Orders
        </NavLink>
      </nav>
      <a
        className={styles.repoLink}
        href={REPO_URL}
        target="_blank"
        rel="noopener noreferrer"
      >
        <GithubOutlined /> GitHub
      </a>
    </Header>
  )
}
