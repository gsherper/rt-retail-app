import { App, Button, Tooltip } from 'antd'
import { DeleteOutlined, EditOutlined } from '@ant-design/icons'
import type { CustomCellRendererProps } from 'ag-grid-react'
import { errorMessage } from '../../../lib/errors'
import styles from './RtGridRowActions.module.css'

export interface RowActionsContext<T> {
  entityName: string
  getRowId: (row: T) => string
  // describe is used to get row info to be shown in the delete confirm
  describe?: (row: T) => string
  onEdit: (row: T) => void
  onDelete: (row: T) => unknown
}

export function RtGridRowActions<T>({
  data,
  context,
}: CustomCellRendererProps<T, unknown, RowActionsContext<T>>) {
  // AG Grid renders cells inside the app, so antd's App context is available
  const { modal, message } = App.useApp()
  if (!data) return null

  const { entityName, getRowId, describe, onEdit, onDelete } = context
  const entityLower = entityName.toLowerCase()

  const confirmDelete = () =>
    modal.confirm({
      title: `Delete ${entityLower} ${getRowId(data)}?`,
      content: describe ? `${describe(data)} will be removed.` : undefined,
      okText: 'Delete',
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await onDelete(data)
        } catch (error) {
          message.error(errorMessage(error, `Could not delete ${entityLower}`))
        }
      },
    })

  return (
    <span className={styles.actions}>
      <Tooltip title={`Edit ${entityName}`}>
        <Button
          type="text"
          size="small"
          icon={<EditOutlined />}
          aria-label={`Edit ${entityName}`}
          onClick={() => onEdit(data)}
        />
      </Tooltip>
      <Tooltip title={`Delete ${entityName}`}>
        <Button
          type="text"
          size="small"
          danger
          icon={<DeleteOutlined />}
          aria-label={`Delete ${entityName}`}
          onClick={confirmDelete}
        />
      </Tooltip>
    </span>
  )
}
