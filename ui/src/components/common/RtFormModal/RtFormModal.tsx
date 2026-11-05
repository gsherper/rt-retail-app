import { Form, Input, InputNumber, Modal, Select } from 'antd'
import type { FormInstance } from 'antd'
import { cloneElement, useState } from 'react'
import type { ReactElement, ReactNode } from 'react'

export interface RTInputField<T> {
  name: keyof T & string
  label: string
  required?: boolean
  // In RT form disabled prop is efffectively readonly on the field
  disabled?: boolean
  lockOnEdit?: boolean

  input?: () => ReactElement<{ disabled?: boolean }>
  type?: 'text' | 'number' | 'select'
  // Used for middle initials
  maxLength?: number
  // number fields only: whole numbers only (quantity), we need min and precision
  min?: number
  precision?: number
  // dropdown options
  options?: { value: string; label: string }[]
}

// This is used for read only field
export interface RTDisplayField<T> {
  key: string
  label: string
  display: (values: Partial<T>) => ReactNode
}

export type RTFormField<T> = RTInputField<T> | RTDisplayField<T>

function renderInput<T>(field: RTInputField<T>) {
  const { disabled } = field
  if (field.input) return cloneElement(field.input(), { disabled })
  switch (field.type) {
    case 'number':
      return (
        <InputNumber
          min={field.min}
          precision={field.precision}
          disabled={disabled}
          style={{ width: '100%' }}
        />
      )
    case 'select':
      return <Select options={field.options} disabled={disabled} />
    default:
      return <Input maxLength={field.maxLength} disabled={disabled} />
  }
}

function renderField<T>(field: RTFormField<T>, form: FormInstance<Partial<T>>) {
  if ('display' in field) {
    return (
      <Form.Item key={field.key} label={field.label}>
        {/* shouldUpdate re-renders this on every change to the form's values */}
        <Form.Item noStyle shouldUpdate>
          {() => field.display(form.getFieldsValue())}
        </Form.Item>
      </Form.Item>
    )
  }

  return (
    <Form.Item
      key={field.name}
      name={field.name as string}
      label={field.label}
      rules={
        field.required
          ? [{ required: true, message: `${field.label} is required` }]
          : undefined
      }
    >
      {renderInput(field)}
    </Form.Item>
  )
}

interface RtFormModalProps<T> {
  open: boolean
  title: string
  fields: RTFormField<T>[]
  initialValues?: Partial<T>
  okText?: string
  //onSave prop should use unwrap on the promise
  onSave: (values: Partial<T>) => void | Promise<void>
  onCancel: () => void
}

export function RtFormModal<T>({
  open,
  title,
  fields,
  initialValues,
  okText = 'Save',
  onSave,
  onCancel,
}: RtFormModalProps<T>) {
  const [form] = Form.useForm<Partial<T>>()
  const [saving, setSaving] = useState(false)

  const handleOk = async () => {
    let values: Partial<T>
    try {
      values = await form.validateFields()
    } catch {
      return // antd already shows the field errors
    }

    setSaving(true)
    try {
      await onSave(values)
    } catch {
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      open={open}
      title={title}
      okText={okText}
      onOk={handleOk}
      confirmLoading={saving}
      onCancel={onCancel}
      destroyOnHidden
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={initialValues}
        preserve={false}
      >
        {fields.map((field) => renderField(field, form))}
      </Form>
    </Modal>
  )
}
