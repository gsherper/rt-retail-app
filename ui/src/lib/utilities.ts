interface PersonName {
  firstName: string
  middleInitial?: string | null
  lastName: string
}

export const fullName = ({ firstName, middleInitial, lastName }: PersonName) =>
  [firstName, middleInitial, lastName].filter(Boolean).join(' ')

export const lastFirstName = ({
  firstName,
  middleInitial,
  lastName,
}: PersonName) =>
  `${lastName}, ${[firstName, middleInitial].filter(Boolean).join(' ')}`

const usd = new Intl.NumberFormat(undefined, {
  style: 'currency',
  currency: 'USD',
})

export const formatUsd = (amount: number) => usd.format(amount)
