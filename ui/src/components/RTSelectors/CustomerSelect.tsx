import { RTSelect } from '../common/RTSelect/RTSelect'
import type { RTSelectorProps } from '../common/RTSelect/RTSelect'
import { lastFirstName } from '../../lib/utilities'
import type { Customer } from '../../models/Customer'
import { useGetCustomersQuery } from '../../store/customersApi'

const getId = (customer: Customer) => customer.customerID

export function CustomerSelect(props: RTSelectorProps) {
  const { data, isLoading } = useGetCustomersQuery()
  return (
    <RTSelect
      {...props}
      rows={data}
      loading={isLoading}
      getId={getId}
      getLabel={lastFirstName}
      placeholder="Type a name or ID"
    />
  )
}
