interface OrderPerson {
  firstName: string
  middleInitial?: string | null
  lastName: string
}

export interface OrderCustomer extends OrderPerson {
  customerID: number
}

// The employee who made the sale; employeeID is the order's sales person ID
export interface OrderSalesPerson extends OrderPerson {
  employeeID: number
}

export interface OrderProduct {
  productID: number
  name: string
}

export interface Order {
  orderID: number
  customer: OrderCustomer
  salesPerson: OrderSalesPerson
  product: OrderProduct
  quantity: number
  totalUsd: number
  version: number
}

export interface OrderFormValues {
  customerID: number
  salesPersonID: number
  productID: number
  quantity: number
}
