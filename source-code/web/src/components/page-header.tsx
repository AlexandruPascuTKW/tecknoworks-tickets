import type { ReactNode } from 'react'

/** The one pattern every page opens with: a title, a line of context, and the primary action on the right. */
export function PageHeader({
  title,
  description,
  actions,
}: {
  title: string
  description?: ReactNode
  actions?: ReactNode
}) {
  return (
    <div className="flex flex-wrap items-start justify-between gap-3 print:mb-2">
      <div className="grid gap-1">
        <h2 className="text-2xl font-semibold tracking-tight text-heading">{title}</h2>
        {description && <p className="max-w-prose text-sm text-muted-foreground">{description}</p>}
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2 print:hidden">{actions}</div>}
    </div>
  )
}
