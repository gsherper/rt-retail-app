import { Space, Typography } from 'antd'
import styles from './RtGridStatusBar.module.css'

const { Text } = Typography

interface GridStatusBarProps {
  totalRows: number
  filteredRows?: number
  selectedRows?: number
}

export default function RtGridStatusBar({
  totalRows,
  filteredRows,
  selectedRows = 0,
}: GridStatusBarProps) {
  const showingFiltered =
    filteredRows !== undefined && filteredRows !== totalRows

  return (
    <div className={styles.statusBar}>
      <Space size="large">
        <Text type="secondary">
          {showingFiltered
            ? `${filteredRows.toLocaleString()} of ${totalRows.toLocaleString()} rows`
            : `${totalRows.toLocaleString()} rows`}
        </Text>
      </Space>

      <Space size="large">
        {selectedRows > 0 && (
          <Text type="secondary">{selectedRows.toLocaleString()} selected</Text>
        )}
      </Space>
    </div>
  )
}
