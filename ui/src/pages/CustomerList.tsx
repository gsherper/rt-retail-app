import { useCallback } from 'react'
import { useNavigate } from 'react-router'
import type { Customer } from '../models/Customer'

import { RtGrid } from '../components/common/Grid/RtGrid'
import {
  customerColumnDefs,
  customerFormFields,
  describeCustomer,
  getCustomerRowId,
} from '../config/customerListDefs'

import type { CustomerInput } from '../store/customersApi'

import {
  useAddCustomerMutation,
  useDeleteCustomerMutation,
  useGetCustomersQuery,
  useUpdateCustomerMutation,
} from '../store/customersApi'

import { useAppSelector } from '../store/hooks'

import { selectHubStatus } from '../store/hubStatusSlice'

export function CustomerList() {
  const navigate = useNavigate()

  const { data, isLoading } = useGetCustomersQuery()
  const connectionStatus = useAppSelector(selectHubStatus)
  const [addCustomer] = useAddCustomerMutation()
  const [updateCustomer] = useUpdateCustomerMutation()
  const [deleteCustomer] = useDeleteCustomerMutation()

  const openOrders = useCallback(
    (customer: Customer) =>
      navigate(`/orders?customerId=${customer.customerID}`),
    [navigate],
  )
  const onDelete = useCallback(
    (customer: Customer) => deleteCustomer(customer).unwrap(),
    [deleteCustomer],
  )

  return (
    <div className="page">
      <RtGrid<Customer>
        title="Customers"
        entityName="Customer"
        rows={data ?? []}
        loading={isLoading}
        columnDefs={customerColumnDefs}
        getRowId={getCustomerRowId}
        formFields={customerFormFields}
        describe={describeCustomer}
        searchPlaceholder="Search name…"
        sortStorageKey="customers"
        connectionStatus={connectionStatus}
        // Double-click opens the customer orders list
        onRowDoubleClick={openOrders}
        onAdd={(values) => addCustomer(values as CustomerInput).unwrap()}
        onUpdate={(customer, values) =>
          updateCustomer({ ...customer, ...values }).unwrap()
        }
        onDelete={onDelete}
      />
    </div>
  )
}
