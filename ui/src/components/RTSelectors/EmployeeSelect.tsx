import { RTSelect } from '../common/RTSelect/RTSelect'
import type { RTSelectorProps } from '../common/RTSelect/RTSelect'
import { lastFirstName } from '../../lib/utilities'
import type { Employee } from '../../models/Employee'
import { useGetEmployeesQuery } from '../../store/employeesApi'

const getId = (employee: Employee) => employee.employeeID

export function EmployeeSelect(props: RTSelectorProps) {
  const { data, isLoading } = useGetEmployeesQuery()
  return (
    <RTSelect
      {...props}
      rows={data}
      loading={isLoading}
      getId={getId}
      getLabel={lastFirstName}
      placeholder="Select a sales person"
    />
  )
}
