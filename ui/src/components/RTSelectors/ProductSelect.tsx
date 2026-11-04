import { RTSelect } from '../common/RTSelect/RTSelect'
import type { RTSelectorProps } from '../common/RTSelect/RTSelect'
import type { Product } from '../../models/Product'
import { useGetProductsQuery } from '../../store/productsApi'

const getId = (product: Product) => product.productID
const getLabel = (product: Product) => product.name

export function ProductSelect(props: RTSelectorProps) {
  const { data, isLoading } = useGetProductsQuery()
  return (
    <RTSelect
      {...props}
      rows={data}
      loading={isLoading}
      getId={getId}
      getLabel={getLabel}
      placeholder="Type a name or ID"
    />
  )
}
