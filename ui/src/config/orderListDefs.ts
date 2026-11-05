import { createElement } from 'react'
import type { ColDef } from 'ag-grid-community'
import type { RTFormField } from '../components/common/RtFormModal/RtFormModal'
import { CustomerSelect } from '../components/RTSelectors/CustomerSelect'
import { EmployeeSelect } from '../components/RTSelectors/EmployeeSelect'
import { ProductSelect } from '../components/RTSelectors/ProductSelect'
import { formatUsd, fullName } from '../lib/utilities'
import type { Order, OrderFormValues } from '../models/Order'
import type { Product } from '../models/Product'

// Grid and form definitions for the orders list page

export const orderColumnDefs: ColDef<Order>[] = [
  {
    field: 'orderID',
    headerName: 'OrderID',
    type: 'rightAligned',
    width: 140,
    sort: 'desc',
  },
  {
    colId: 'customer',
    headerName: 'Customer',
    valueGetter: ({ data }) => (data ? fullName(data.customer) : ''),
    flex: 1,
  },
  {
    colId: 'salesPerson',
    headerName: 'Sales Person',
    valueGetter: ({ data }) => (data ? fullName(data.salesPerson) : ''),
    flex: 1,
  },
  {
    colId: 'product',
    headerName: 'Product',
    valueGetter: ({ data }) => data?.product.name ?? '',
    flex: 1,
  },
  {
    field: 'quantity',
    type: 'rightAligned',
    width: 110,
  },
  {
    field: 'totalUsd',
    headerName: 'Total',
    type: 'rightAligned',
    width: 130,
    //this value will be calculated by server on reads
    valueFormatter: ({ value }) =>
      typeof value === 'number' ? formatUsd(value) : '',
  },
]

export const orderFormFields: RTFormField<OrderFormValues>[] = [
  {
    name: 'customerID',
    label: 'Customer',
    required: true,
    // Lock the customer ID when user opens edit form. User shouldn't be able to move
    // an existing order to another customer
    lockOnEdit: true,
    input: () => createElement(CustomerSelect),
  },
  {
    name: 'salesPersonID',
    label: 'Sales Person',
    required: true,
    input: () => createElement(EmployeeSelect),
  },
  {
    name: 'productID',
    label: 'Product',
    required: true,
    input: () => createElement(ProductSelect),
  },
  {
    name: 'quantity',
    label: 'Quantity',
    type: 'number',
    required: true,
    min: 1,
    precision: 0,
  },
]

//Read-only total calculated on the UI during new order creation
export const orderTotalField = (
  products: Product[] | undefined,
): RTFormField<OrderFormValues> => ({
  key: 'total',
  label: 'Total',
  display: ({ productID, quantity }) => {
    const price = products?.find((p) => p.productID === productID)?.price
    return formatUsd(price !== undefined && quantity ? price * quantity : 0)
  },
})

// load current order values to form
export const toOrderFormValues = (order: Order): OrderFormValues => ({
  customerID: order.customer.customerID,
  salesPersonID: order.salesPerson.employeeID,
  productID: order.product.productID,
  quantity: order.quantity,
})

export const getOrderRowId = (order: Order) => String(order.orderID)

// used in the delete confirmation
export const describeOrder = (order: Order) =>
  `Order ${order.orderID} (${order.quantity} × ${order.product.name})`
