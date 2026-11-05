import { Badge, Space, Tooltip, Typography } from 'antd'
import type { BadgeProps } from 'antd'
import type { ConnectionStatus } from '../../../lib/hub/HubClient'
import styles from './RtGridStatusBar.module.css'

const { Text } = Typography

const connectionBadge: Record<
  ConnectionStatus,
  { status: BadgeProps['status']; label: string }
> = {
  connected: { status: 'success', label: 'Websocket Connected' },
  connecting: { status: 'warning', label: 'Websocket Connecting…' },
  disconnected: { status: 'error', label: 'Websocket Disconnected' },
}

interface GridStatusBarProps {
  totalRows: number
  filteredRows?: number
  connectionStatus?: ConnectionStatus
}

export function RtGridStatusBar({
  totalRows,
  filteredRows,
  connectionStatus,
}: GridStatusBarProps) {
  const showingFiltered =
    filteredRows !== undefined && filteredRows !== totalRows

  return (
    <div className={styles.statusBar}>
      <Space size="large">
        <Text type="secondary">
          {showingFiltered
            ? `count: ${filteredRows.toLocaleString()} of ${totalRows.toLocaleString()}`
            : `count: ${totalRows.toLocaleString()}`}
        </Text>
      </Space>

      <Space size="large">
        {connectionStatus && (
          <Tooltip
            title={connectionBadge[connectionStatus].label}
            placement="left"
          >
            <Badge status={connectionBadge[connectionStatus].status} />
          </Tooltip>
        )}
      </Space>
    </div>
  )
}
