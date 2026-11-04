import type { Customer } from '../models/Customer'
import { api } from './api'
import { createLiveList } from './liveList'
import { dashboardHub } from './hubConnection'

// Topic `customers` on the server's DashboardHub
const customersLive = createLiveList<Customer>({
  hub: dashboardHub,
  name: 'customers',
  subscribeMethod: 'SubscribeCustomers',
  unsubscribeMethod: 'UnsubscribeCustomers',
  changedEvent: 'CustomerChanged',
  getId: (customer) => customer.customerID,
})

export type CustomerInput = Omit<Customer, 'customerID' | 'version'>

export const customersApi = api.injectEndpoints({
  endpoints: (build) => ({
    getCustomers: build.query<Customer[], void>({
      queryFn: customersLive.queryFn,
      // Remove the cache entry +unsubscribe as soon as user navigates away
      keepUnusedDataFor: 0,
      onCacheEntryAdded: (
        arg,
        { updateCachedData, cacheDataLoaded, cacheEntryRemoved },
      ) => {
        console.log('call sync')
        return customersLive.sync(arg, {
          update: (recipe) => updateCachedData((draft) => recipe(draft)),
          cacheDataLoaded,
          cacheEntryRemoved,
        })
      },
    }),

    addCustomer: build.mutation<Customer, CustomerInput>({
      query: (request) => ({ method: 'CreateCustomer', args: [request] }),
    }),

    updateCustomer: build.mutation<Customer, Customer>({
      query: ({ customerID, ...request }) => ({
        method: 'UpdateCustomer',
        args: [customerID, request],
      }),
    }),

    deleteCustomer: build.mutation<
      void,
      Pick<Customer, 'customerID' | 'version'>
    >({
      query: ({ customerID, version }) => ({
        method: 'DeleteCustomer',
        args: [customerID, version],
      }),
    }),
  }),
})

export const {
  useGetCustomersQuery,
  useAddCustomerMutation,
  useUpdateCustomerMutation,
  useDeleteCustomerMutation,
} = customersApi
