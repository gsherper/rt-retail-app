import { Layout } from 'antd';

const { Header } = Layout;

export default function AppHeader() {
    return (        
      <Header
        style={{
          display: 'flex',
          alignItems: 'center',
        }}
      >
        <span className="app-title">Customer Ordering App</span>
      </Header>
    )
}