import { useMemo } from 'react'
import { Select } from 'antd'

export interface RTSelectorProps {
  id?: string
  value?: number | null
  onChange?: (value: number) => void
  // readonly
  disabled?: boolean
}

interface RTSelectProps<T> extends RTSelectorProps {
  rows: T[] | undefined
  loading: boolean
  getId: (row: T) => number
  getLabel: (row: T) => string
  placeholder: string
}

// Typeahead select over a list of rows
// type part of the label or ID to filter
export function RTSelect<T>({
  rows,
  loading,
  getId,
  getLabel,
  ...props
}: RTSelectProps<T>) {
  const options = useMemo(
    () =>
      (rows ?? []).map((row) => ({
        value: getId(row),
        label: `(${getId(row)}) ${getLabel(row)}`,
      })),
    [rows, getId, getLabel],
  )

  return (
    <Select
      {...props}
      options={options}
      loading={loading}
      showSearch={{ optionFilterProp: 'label' }}
    />
  )
}
