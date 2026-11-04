import type { Employee } from '../models/Employee'
import { api } from './api'
import { createLiveList } from './liveList'
import { dashboardHub } from './hubConnection'

// Topic `employees` on the server's DashboardHub
const employeesLive = createLiveList<Employee>({
  hub: dashboardHub,
  name: 'employees',
  subscribeMethod: 'SubscribeEmployees',
  unsubscribeMethod: 'UnsubscribeEmployees',
  changedEvent: 'EmployeeChanged',
  getId: (employee) => employee.employeeID,
})

// Read-only: employees feed the order form's sales person picker
export const employeesApi = api.injectEndpoints({
  endpoints: (build) => ({
    getEmployees: build.query<Employee[], void>({
      queryFn: employeesLive.queryFn,
      // Remove the cache entry (and so unsubscribe) as soon as no page uses it,
      keepUnusedDataFor: 0,
      onCacheEntryAdded: (
        arg,
        { updateCachedData, cacheDataLoaded, cacheEntryRemoved },
      ) =>
        employeesLive.sync(arg, {
          update: (recipe) => updateCachedData((draft) => recipe(draft)),
          cacheDataLoaded,
          cacheEntryRemoved,
        }),
    }),
  }),
})

export const { useGetEmployeesQuery } = employeesApi
