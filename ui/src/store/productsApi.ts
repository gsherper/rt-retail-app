import type { Product } from '../models/Product'
import { api } from './api'
import { createLiveList } from './liveList'
import { dashboardHub } from './hubConnection'

// Topic `products` on the server's DashboardHub
const productsLive = createLiveList<Product>({
  hub: dashboardHub,
  name: 'products',
  subscribeMethod: 'SubscribeProducts',
  unsubscribeMethod: 'UnsubscribeProducts',
  changedEvent: 'ProductChanged',
  getId: (product) => product.productID,
})

// Read-only: products feed the order form's product picker and total
export const productsApi = api.injectEndpoints({
  endpoints: (build) => ({
    getProducts: build.query<Product[], void>({
      queryFn: productsLive.queryFn,
      // Remove the cache entry +unsubscribe as soon as user navigates away
      keepUnusedDataFor: 0,
      onCacheEntryAdded: (
        arg,
        { updateCachedData, cacheDataLoaded, cacheEntryRemoved },
      ) =>
        productsLive.sync(arg, {
          update: (recipe) => updateCachedData((draft) => recipe(draft)),
          cacheDataLoaded,
          cacheEntryRemoved,
        }),
    }),
  }),
})

export const { useGetProductsQuery } = productsApi
