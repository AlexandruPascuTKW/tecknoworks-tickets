import { useId, type ComponentProps, type ReactNode } from 'react'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'

type ControlProps = { id: string; 'aria-invalid': boolean | undefined; 'aria-describedby': string | undefined }

/** A label, a control and its inline error, wired together for assistive technology. */
export function FormField({
  label,
  error,
  hint,
  children,
}: {
  label: string
  error?: string
  hint?: string
  children: (control: ControlProps) => ReactNode
}) {
  const id = useId()
  const messageId = `${id}-message`
  const hasMessage = Boolean(error || hint)

  return (
    <div className="grid gap-1.5" data-invalid={error ? true : undefined}>
      <Label htmlFor={id}>{label}</Label>
      {children({
        id,
        'aria-invalid': error ? true : undefined,
        'aria-describedby': hasMessage ? messageId : undefined,
      })}
      {hasMessage && (
        <p id={messageId} className={error ? 'text-sm text-destructive' : 'text-sm text-muted-foreground'}>
          {error ?? hint}
        </p>
      )}
    </div>
  )
}

export function TextField({
  label,
  error,
  hint,
  ...props
}: { label: string; error?: string; hint?: string } & Omit<ComponentProps<typeof Input>, 'id'>) {
  return (
    <FormField label={label} error={error} hint={hint}>
      {(control) => <Input {...control} {...props} />}
    </FormField>
  )
}

export function TextareaField({
  label,
  error,
  hint,
  ...props
}: { label: string; error?: string; hint?: string } & Omit<ComponentProps<typeof Textarea>, 'id'>) {
  return (
    <FormField label={label} error={error} hint={hint}>
      {(control) => <Textarea {...control} {...props} />}
    </FormField>
  )
}
