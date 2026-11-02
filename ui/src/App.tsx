import { useState } from 'react'
import { Button, Card, Space, Typography } from 'antd'
import { AgGridReact } from 'ag-grid-react'
import type { ColDef } from 'ag-grid-community'

interface Product {
  sku: string
  name: string
  price: number
  stock: number
}

const columnDefs: ColDef<Product>[] = [
  { field: 'sku' },
  { field: 'name', flex: 1 },
  { field: 'price', valueFormatter: (p) => `$${p.value?.toFixed(2)}` },
  { field: 'stock' },
]

const initialRows: Product[] = [
  { sku: 'A-100', name: 'Coffee Mug', price: 12.5, stock: 40 },
  { sku: 'B-200', name: 'Tote Bag', price: 18, stock: 25 },
  { sku: 'C-300', name: 'Notebook', price: 6.75, stock: 120 },
]

function App() {
  const [rows, setRows] = useState(initialRows)

  const addRow = () =>
    setRows((r) => [
      ...r,
      { sku: `N-${r.length + 1}`, name: 'New Product', price: 0, stock: 0 },
    ])

  return (
    <Card style={{ margin: 24 }}>
      <Space direction="vertical" style={{ width: '100%' }}>
        <Typography.Title level={3}>Products</Typography.Title>
        <Button type="primary" onClick={addRow}>
          Add product
        </Button>
        <div style={{ height: 400 }}>
          <AgGridReact<Product> rowData={rows} columnDefs={columnDefs} />
        </div>
      </Space>
    </Card>
  )
}

export default App
