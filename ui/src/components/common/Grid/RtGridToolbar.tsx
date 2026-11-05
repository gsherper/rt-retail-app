import { Button, Input, Typography } from 'antd'
import { PlusOutlined, SearchOutlined } from '@ant-design/icons'
import styles from './RtGridToolbar.module.css'

const { Text } = Typography

interface RtGridToolbarProps {
  title?: string
  onSearch?: (text: string) => void
  searchPlaceholder?: string
  onAdd: () => void
  addLabel?: string
}

export function RtGridToolbar({
  title,
  onSearch,
  searchPlaceholder = 'Search…',
  onAdd,
  addLabel = 'Add',
}: RtGridToolbarProps) {
  return (
    <div className={styles.toolbar}>
      <Text className={styles.title}>{title}</Text>
      <div className={styles.controls}>
        {onSearch && (
          <Input
            className={styles.search}
            prefix={<SearchOutlined />}
            placeholder={searchPlaceholder}
            allowClear
            onChange={(e) => onSearch(e.target.value)}
          />
        )}
        <div className={styles.actions}>
          <Button type="link" icon={<PlusOutlined />} onClick={onAdd}>
            {addLabel}
          </Button>
        </div>
      </div>
    </div>
  )
}
