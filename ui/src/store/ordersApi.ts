import type { Order, OrderFormValues } from '../models/Order'
import { api } from './api'
import { createLiveList } from './liveList'
import { dashboardHub } from './hubConnection'

// Topic `orders` on the server's DashboardHub: every customer's orders
const ordersLive = createLiveList<Order>({
  hub: dashboardHub,
  name: 'orders',
  subscribeMethod: 'SubscribeOrders',
  unsubscribeMethod: 'UnsubscribeOrders',
  changedEvent: 'OrderChanged',
  getId: (order) => order.orderID,
})

// Topic `customer:{id}:orders` for customer specific view
const customerOrdersLive = createLiveList<Order, number>({
  hub: dashboardHub,
  name: 'customer orders',
  subscribeMethod: 'SubscribeCustomerOrders',
  unsubscribeMethod: 'UnsubscribeCustomerOrders',
  methodArgs: (customerID) => [customerID],
  changedEvent: 'OrderChanged',
  getId: (order) => order.orderID,
  // OrderChanged also arrives from the `orders` topic if that's subscribed too
  belongs: (order, customerID) => order.customer.customerID === customerID,
})

export type OrderUpdate = Omit<OrderFormValues, 'customerID'> & {
  orderID: number
  version: number
}

export const ordersApi = api.injectEndpoints({
  endpoints: (build) => ({
    getOrders: build.query<Order[], void>({
      queryFn: ordersLive.queryFn,
      // Remove the cache entry (and so unsubscribe) as soon as no page uses it,
      // instead of RTK Query's default 60 s; coming back subscribes again
      keepUnusedDataFor: 0,
      onCacheEntryAdded: (
        arg,
        { updateCachedData, cacheDataLoaded, cacheEntryRemoved },
      ) =>
        ordersLive.sync(arg, {
          update: (recipe) => updateCachedData((draft) => recipe(draft)),
          cacheDataLoaded,
          cacheEntryRemoved,
        }),
    }),

    getCustomerOrders: build.query<Order[], number>({
      queryFn: customerOrdersLive.queryFn,
      keepUnusedDataFor: 0,
      onCacheEntryAdded: (
        arg,
        { updateCachedData, cacheDataLoaded, cacheEntryRemoved },
      ) =>
        customerOrdersLive.sync(arg, {
          update: (recipe) => updateCachedData((draft) => recipe(draft)),
          cacheDataLoaded,
          cacheEntryRemoved,
        }),
    }),

    addOrder: build.mutation<Order, OrderFormValues>({
      query: ({ customerID, ...request }) => ({
        method: 'CreateOrder',
        args: [customerID, request],
      }),
    }),

    updateOrder: build.mutation<Order, OrderUpdate>({
      query: ({ orderID, ...request }) => ({
        method: 'UpdateOrder',
        args: [orderID, request],
      }),
    }),

    deleteOrder: build.mutation<void, Pick<Order, 'orderID' | 'version'>>({
      query: ({ orderID, version }) => ({
        method: 'DeleteOrder',
        args: [orderID, version],
      }),
    }),
  }),
})

export const {
  useGetOrdersQuery,
  useGetCustomerOrdersQuery,
  useAddOrderMutation,
  useUpdateOrderMutation,
  useDeleteOrderMutation,
} = ordersApi
