export interface Customer {
  customerID: number
  firstName: string
  middleInitial?: string | null
  lastName: string
  //used to detect concurrent edits
  version: number
}
