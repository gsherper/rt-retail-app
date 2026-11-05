import { useCallback, useMemo } from 'react'
import { useSearchParams } from 'react-router'
import { RtGrid } from '../components/common/Grid/RtGrid'
import { DEFAULT_SALES_PERSON_ID } from '../config/appConfig'
import {
  describeOrder,
  getOrderRowId,
  orderColumnDefs,
  orderFormFields,
  orderTotalField,
  toOrderFormValues,
} from '../config/orderListDefs'
import { fullName } from '../lib/utilities'
import type { Order, OrderFormValues } from '../models/Order'
import { useAppSelector } from '../store/hooks'
import {
  useAddOrderMutation,
  useDeleteOrderMutation,
  useGetCustomerOrdersQuery,
  useGetOrdersQuery,
  useUpdateOrderMutation,
} from '../store/ordersApi'
import { useGetProductsQuery } from '../store/productsApi'
import { selectHubStatus } from '../store/hubStatusSlice'

export function Orders() {
  const [searchParams] = useSearchParams()
  const customerIdParam = searchParams.get('customerId')
  const customerId = customerIdParam === null ? null : Number(customerIdParam)

  // If customerId exists, this will effectively be skipped.
  // No subscription happens
  const allOrders = useGetOrdersQuery(undefined, {
    skip: customerId !== null,
  })

  // If customerId exists, this will effectively be skipped.
  // No subscription happens
  const customerOrders = useGetCustomerOrdersQuery(customerId ?? 0, {
    skip: customerId === null,
  })

  const { data, isLoading } = customerId === null ? allOrders : customerOrders
  // Prices for the form's total
  const { data: products } = useGetProductsQuery()
  const connectionStatus = useAppSelector(selectHubStatus)

  const [addOrder] = useAddOrderMutation()
  const [updateOrder] = useUpdateOrderMutation()
  const [deleteOrder] = useDeleteOrderMutation()

  // Stable handler: this page re-renders on every connection status change,
  // and a new function would make the grid redraw its action buttons each time
  const onDelete = useCallback(
    (order: Order) => deleteOrder(order).unwrap(),
    [deleteOrder],
  )

  // On one customer's orders, a new order is for that customer: start the form
  // with them selected and don't let it change
  const formFields = useMemo(
    () => [
      ...(customerId === null
        ? orderFormFields
        : orderFormFields.map((field) =>
            'name' in field && field.name === 'customerID'
              ? { ...field, disabled: true }
              : field,
          )),
      orderTotalField(products),
    ],
    [customerId, products],
  )
  const newOrderValues = useMemo<Partial<OrderFormValues>>(
    () => ({
      salesPersonID: DEFAULT_SALES_PERSON_ID,
      ...(customerId === null ? {} : { customerID: customerId }),
    }),
    [customerId],
  )

  // Every order embeds its customer
  const customer = data?.[0]?.customer
  const title =
    customerId === null
      ? 'Orders'
      : `Orders — ${customer ? fullName(customer) : `Customer ${customerId}`}`

  return (
    <div className="page">
      <RtGrid<Order, OrderFormValues>
        title={title}
        entityName="Order"
        rows={data ?? []}
        loading={isLoading}
        columnDefs={orderColumnDefs}
        getRowId={getOrderRowId}
        formFields={formFields}
        toFormValues={toOrderFormValues}
        newRowValues={newOrderValues}
        describe={describeOrder}
        searchPlaceholder="Search orders…"
        connectionStatus={connectionStatus}
        // The grid updates when the server pushes the change, not from the responses
        onAdd={(values) => addOrder(values as OrderFormValues).unwrap()}
        onUpdate={(order, values) => {
          const { salesPersonID, productID, quantity } = {
            ...toOrderFormValues(order),
            ...values,
          }
          return updateOrder({
            orderID: order.orderID,
            version: order.version,
            salesPersonID,
            productID,
            quantity,
          }).unwrap()
        }}
        onDelete={onDelete}
      />
    </div>
  )
}
