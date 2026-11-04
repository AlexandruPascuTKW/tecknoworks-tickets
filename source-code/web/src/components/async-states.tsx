import { AlertCircle, type LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Empty, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Skeleton } from '@/components/ui/skeleton'
import type { AsyncState } from '@/lib/use-async'

/** Names the cause and offers the remedy, as every data-driven view must (states-and-feedback). */
export function LoadError({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <Alert variant="destructive">
      <AlertCircle />
      <AlertTitle>That didn't load</AlertTitle>
      <AlertDescription>
        <p>{message}</p>
        {onRetry && (
          <Button variant="outline" size="sm" className="mt-2" onClick={onRetry}>
            Try again
          </Button>
        )}
      </AlertDescription>
    </Alert>
  )
}

export function EmptyState({
  icon: Icon,
  title,
  children,
  action,
}: {
  icon: LucideIcon
  title: string
  children?: ReactNode
  action?: ReactNode
}) {
  return (
    <Empty className="rounded-xl border border-dashed py-12">
      <EmptyHeader>
        <EmptyMedia variant="icon">
          <Icon />
        </EmptyMedia>
        <EmptyTitle>{title}</EmptyTitle>
        {children && <EmptyDescription>{children}</EmptyDescription>}
      </EmptyHeader>
      {action}
    </Empty>
  )
}

export function RowsSkeleton({ rows = 3, height = 'h-14' }: { rows?: number; height?: string }) {
  return (
    <div className="grid gap-2" aria-busy="true" aria-label="Loading">
      {Array.from({ length: rows }, (_, i) => (
        <Skeleton key={i} className={`${height} w-full`} />
      ))}
    </div>
  )
}

/** Loading, error, then the content — `children` only runs once there is data. */
export function Loadable<T>({
  state,
  skeleton = <RowsSkeleton />,
  children,
}: {
  state: AsyncState<T>
  skeleton?: ReactNode
  children: (data: T) => ReactNode
}) {
  if (state.error && state.data === undefined) return <LoadError message={state.error.message} onRetry={state.reload} />
  if (state.data === undefined) return <>{skeleton}</>

  return <>{children(state.data)}</>
}
