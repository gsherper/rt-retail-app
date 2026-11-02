import { AgGridReact } from 'ag-grid-react'
import type { ColDef } from 'ag-grid-community'
import type {
  GridReadyEvent,
  FilterChangedEvent,
  SelectionChangedEvent,
} from 'ag-grid-community'
import GridStatusBar from './RtGridStatusBar'
import { useState, useCallback } from 'react'
import type { Customer } from '../../models/Customer'

const columnDefs: ColDef<Customer>[] = [
  { field: 'customerId' },
  { field: 'firstName' },
  { field: 'middleInitial' },
  { field: 'lastName' },
]

const initialRows: Customer[] = [
  {
    customerId: 1001,
    firstName: 'James',
    middleInitial: 'A',
    lastName: 'Anderson',
  },
  {
    customerId: 1002,
    firstName: 'Mary',
    middleInitial: 'B',
    lastName: 'Thomas',
  },
  {
    customerId: 1003,
    firstName: 'John',
    middleInitial: null,
    lastName: 'Jackson',
  },
  {
    customerId: 1004,
    firstName: 'Patricia',
    middleInitial: 'C',
    lastName: 'White',
  },
  {
    customerId: 1005,
    firstName: 'Robert',
    middleInitial: 'D',
    lastName: 'Harris',
  },
  {
    customerId: 1006,
    firstName: 'Jennifer',
    middleInitial: 'E',
    lastName: 'Martin',
  },
  {
    customerId: 1007,
    firstName: 'Michael',
    middleInitial: null,
    lastName: 'Thompson',
  },
  {
    customerId: 1008,
    firstName: 'Linda',
    middleInitial: 'F',
    lastName: 'Garcia',
  },
  {
    customerId: 1009,
    firstName: 'William',
    middleInitial: 'G',
    lastName: 'Martinez',
  },
  {
    customerId: 1010,
    firstName: 'Elizabeth',
    middleInitial: null,
    lastName: 'Robinson',
  },
  {
    customerId: 1011,
    firstName: 'David',
    middleInitial: 'H',
    lastName: 'Clark',
  },
  {
    customerId: 1012,
    firstName: 'Barbara',
    middleInitial: 'I',
    lastName: 'Rodriguez',
  },
  {
    customerId: 1013,
    firstName: 'Richard',
    middleInitial: null,
    lastName: 'Lewis',
  },
  { customerId: 1014, firstName: 'Susan', middleInitial: 'J', lastName: 'Lee' },
  {
    customerId: 1015,
    firstName: 'Joseph',
    middleInitial: 'K',
    lastName: 'Walker',
  },
  {
    customerId: 1016,
    firstName: 'Jessica',
    middleInitial: null,
    lastName: 'Hall',
  },
]

export default function RtGrid() {
  const [rows, setRows] = useState(initialRows)
  const [filteredCount, setFilteredCount] = useState<number | undefined>()
  const [selectedCount, setSelectedCount] = useState(0)

  const onGridReady = useCallback((params: GridReadyEvent) => {
    setFilteredCount(params.api.getDisplayedRowCount())
  }, [])

  const onFilterChanged = useCallback((params: FilterChangedEvent) => {
    setFilteredCount(params.api.getDisplayedRowCount())
  }, [])

  const onSelectionChanged = useCallback((params: SelectionChangedEvent) => {
    setSelectedCount(params.api.getSelectedRows().length)
  }, [])

  return (
    <div className="page-grid">
      <AgGridReact<Customer>
        rowData={rows}
        columnDefs={columnDefs}
        containerStyle={{ flex: 1, minHeight: 0 }}
        onGridReady={onGridReady}
        onFilterChanged={onFilterChanged}
        onSelectionChanged={onSelectionChanged}
      />
      <GridStatusBar
        totalRows={rows.length}
        filteredRows={filteredCount}
        selectedRows={selectedCount}
      />
    </div>
  )
}
