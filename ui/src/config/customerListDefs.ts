import type { ColDef } from 'ag-grid-community'
import type { RTFormField } from '../components/common/RtFormModal/RtFormModal'
import { fullName } from '../lib/utilities'
import type { Customer } from '../models/Customer'

// Grid and form definitions for the customer list page

export const customerColumnDefs: ColDef<Customer>[] = [
  { field: 'customerID', type: 'rightAligned', width: 150, sort: 'desc' },
  { field: 'firstName', flex: 1 },
  { field: 'middleInitial', flex: 1 },
  { field: 'lastName', flex: 1 },
]

export const customerFormFields: RTFormField<Customer>[] = [
  { name: 'firstName', label: 'First Name', required: true },
  { name: 'middleInitial', label: 'Middle Initial', maxLength: 1 },
  { name: 'lastName', label: 'Last Name', required: true },
]

export const getCustomerRowId = (customer: Customer) =>
  String(customer.customerID)

// customer full name label delete confirmation
export const describeCustomer = (customer: Customer) => fullName(customer)
