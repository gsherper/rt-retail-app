import { AgGridReact } from 'ag-grid-react'
import type { ColDef } from 'ag-grid-community'
import { App } from 'antd'
import { useState, useCallback, useMemo } from 'react'

import type {
  CellDoubleClickedEvent,
  ColumnState,
  GetRowIdParams,
  GridReadyEvent,
  ModelUpdatedEvent,
  RowSelectionOptions,
  SortChangedEvent,
} from 'ag-grid-community'
import styles from './RtGrid.module.css'
import { RtGridStatusBar } from './RtGridStatusBar'
import { RtGridToolbar } from './RtGridToolbar'
import { RtGridRowActions } from './RtGridRowActions'
import type { RowActionsContext } from './RtGridRowActions'
import { RtFormModal } from '../RtFormModal/RtFormModal'
import type { RTFormField } from '../RtFormModal/RtFormModal'
import type { ConnectionStatus } from '../../../lib/hub/HubClient'
import { errorMessage } from '../../../lib/errors'

// Flash cells whose value changes when a row is updated (needs getRowId)
const defaultColDef: ColDef = { enableCellChangeFlash: true }

const ACTIONS_COL_ID = 'actions'

// Pinned-right column with each row's Edit and Delete buttons
const actionsColumn: ColDef = {
  colId: ACTIONS_COL_ID,
  headerName: '',
  width: 52,
  cellClass: styles.actionsCell,
  pinned: 'right',
  lockPinned: true,
  cellRenderer: RtGridRowActions,
  enableCellChangeFlash: false,
  sortable: false,
  resizable: false,
  suppressMovable: true,
}

const rowSelection: RowSelectionOptions = {
  mode: 'singleRow',
  checkboxes: false,
  enableClickSelection: true,
}

type EditState<T> = { mode: 'add' } | { mode: 'edit'; row: T } | null

// Saved sort: which columns are sorted, which way, and in what order.
type SavedSort = Pick<ColumnState, 'colId' | 'sort' | 'sortIndex'>[]

const sortStorageKey = (key: string) => `rtGrid.sort.${key}`

function loadSort(key: string): SavedSort | null {
  try {
    const saved = localStorage.getItem(sortStorageKey(key))
    return saved === null ? null : (JSON.parse(saved) as SavedSort)
  } catch {
    return null
  }
}

function saveSort(key: string, sort: SavedSort) {
  try {
    localStorage.setItem(sortStorageKey(key), JSON.stringify(sort))
  } catch {
    // Not saved; the next visit uses the default sort
  }
}

export interface RtGridProps<T extends object, F extends object = T> {
  title: string
  entityName: string
  rows: T[]
  loading?: boolean
  columnDefs: ColDef<T>[]
  getRowId: (row: T) => string
  formFields: RTFormField<F>[]
  toFormValues?: (row: T) => Partial<F>
  // The add form's starting values if form is opened from customer's orders list page
  newRowValues?: Partial<F>
  describe?: (row: T) => string
  searchPlaceholder?: string
  //User's sort in localStorage under sortStorageKey
  sortStorageKey?: string
  connectionStatus?: ConnectionStatus
  onRowDoubleClick?: (row: T) => void
  onAdd: (values: Partial<F>) => unknown
  onUpdate: (row: T, values: Partial<F>) => unknown
  onDelete: (row: T) => unknown
}

export function RtGrid<T extends object, F extends object = T>({
  title,
  entityName,
  rows,
  loading = false,
  columnDefs,
  getRowId,
  formFields,
  toFormValues,
  newRowValues,
  describe,
  searchPlaceholder,
  sortStorageKey,
  connectionStatus,
  onRowDoubleClick,
  onAdd,
  onUpdate,
  onDelete,
}: RtGridProps<T, F>) {
  const [filteredCount, setFilteredCount] = useState<number | undefined>()
  const [editState, setEditState] = useState<EditState<T>>(null)
  const [searchText, setSearchText] = useState('')
  const { message } = App.useApp()

  // Fields marked lockOnEdit are disabled when editing an existing row
  const isEditing = editState?.mode === 'edit'
  const modalFields = useMemo(
    () =>
      isEditing
        ? formFields.map((field) =>
            'lockOnEdit' in field && field.lockOnEdit
              ? { ...field, disabled: true }
              : field,
          )
        : formFields,
    [formFields, isEditing],
  )

  const editRow = useCallback(
    (row: T) => setEditState({ mode: 'edit', row }),
    [],
  )

  // Passed to values through the grid's context for RtGridRowActions buttons
  const rowActionsContext = useMemo<RowActionsContext<T>>(
    () => ({ entityName, getRowId, describe, onEdit: editRow, onDelete }),
    [entityName, getRowId, describe, editRow, onDelete],
  )

  const allColumnDefs = useMemo<ColDef<T>[]>(
    () => [...columnDefs, actionsColumn],
    [columnDefs],
  )

  const gridGetRowId = useCallback(
    (params: GetRowIdParams<T>) => getRowId(params.data),
    [getRowId],
  )

  // Restore the sort saved on a previous visit; it replaces the default sort
  const onGridReady = useCallback(
    ({ api }: GridReadyEvent<T>) => {
      if (!sortStorageKey) return
      const saved = loadSort(sortStorageKey)
      if (saved) {
        api.applyColumnState({ state: saved, defaultState: { sort: null } })
      }
    },
    [sortStorageKey],
  )

  // Save the sort whenever it changes (clicking a header)
  const onSortChanged = useCallback(
    ({ api }: SortChangedEvent<T>) => {
      if (!sortStorageKey) return
      const sorted = api
        .getColumnState()
        .filter((column) => column.sort)
        .map(({ colId, sort, sortIndex }) => ({ colId, sort, sortIndex }))
      saveSort(sortStorageKey, sorted)
    },
    [sortStorageKey],
  )

  const onModelUpdated = useCallback((params: ModelUpdatedEvent) => {
    setFilteredCount(params.api.getDisplayedRowCount())
  }, [])

  const onCellDoubleClicked = useCallback(
    (params: CellDoubleClickedEvent<T>) => {
      if (!params.data || params.column.getColId() === ACTIONS_COL_ID) return
      if (onRowDoubleClick) onRowDoubleClick(params.data)
      else editRow(params.data)
    },
    [onRowDoubleClick, editRow],
  )

  // Populate form values for edit mode
  const getFormStartValues = (): Partial<F> | undefined => {
    if (editState?.mode !== 'edit') return newRowValues

    if (toFormValues) return toFormValues(editState.row)

    return editState.row as unknown as Partial<F>
  }

  const save = async (values: Partial<F>) => {
    try {
      if (editState?.mode === 'edit') {
        await onUpdate(editState.row, values)
      } else {
        await onAdd(values)
      }
      setEditState(null)
    } catch (error) {
      message.error(
        errorMessage(error, `Could not save ${entityName.toLowerCase()}`),
      )
      throw error // keeps the modal open
    }
  }

  return (
    <div className={styles.root}>
      <RtGridToolbar
        title={title}
        onSearch={setSearchText}
        searchPlaceholder={searchPlaceholder}
        onAdd={() => setEditState({ mode: 'add' })}
        addLabel={`Add ${entityName}`}
      />
      <div className={styles.gridArea}>
        <AgGridReact<T>
          rowData={rows}
          loading={loading}
          animateRows={false}
          columnDefs={allColumnDefs}
          defaultColDef={defaultColDef}
          quickFilterText={searchText}
          getRowId={gridGetRowId}
          context={rowActionsContext}
          onCellDoubleClicked={onCellDoubleClicked}
          rowSelection={rowSelection}
          onModelUpdated={onModelUpdated}
          onGridReady={onGridReady}
          onSortChanged={onSortChanged}
        />
      </div>
      <RtGridStatusBar
        totalRows={rows.length}
        filteredRows={filteredCount}
        connectionStatus={connectionStatus}
      />
      <RtFormModal<F>
        open={editState !== null}
        title={
          editState?.mode === 'edit'
            ? `Edit ${entityName} ${getRowId(editState.row)}`
            : `Add ${entityName}`
        }
        fields={modalFields}
        initialValues={getFormStartValues()}
        okText={editState?.mode === 'edit' ? 'Save' : 'Add'}
        onSave={save}
        onCancel={() => setEditState(null)}
      />
    </div>
  )
}
