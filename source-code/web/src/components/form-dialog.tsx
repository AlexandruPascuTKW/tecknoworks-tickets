import { AlertCircle, Loader2 } from 'lucide-react'
import type { FormEvent, ReactNode } from 'react'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import type { ApiError } from '@/lib/api'

/** A refusal or failure, said where the person is looking and announced to assistive technology. */
export function FormError({ error }: { error?: ApiError }) {
  if (!error) return null

  return (
    <Alert variant="destructive">
      <AlertCircle />
      <AlertDescription>{error.message}</AlertDescription>
    </Alert>
  )
}

/** A dialog that holds one small form: a title, the fields, a Cancel and the one action. */
export function FormDialog({
  open,
  onOpenChange,
  title,
  description,
  submitLabel,
  pending,
  error,
  onSubmit,
  children,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  description?: string
  submitLabel: string
  pending: boolean
  error?: ApiError
  onSubmit: () => void
  children: ReactNode
}) {
  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    onSubmit()
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <form onSubmit={handleSubmit} className="grid gap-4" noValidate>
          <DialogHeader>
            <DialogTitle>{title}</DialogTitle>
            <DialogDescription className={description ? undefined : 'sr-only'}>
              {description ?? title}
            </DialogDescription>
          </DialogHeader>

          <FormError error={error} />

          <div className="grid gap-4">{children}</div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={pending}>
              {pending && <Loader2 className="animate-spin" />}
              {submitLabel}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
