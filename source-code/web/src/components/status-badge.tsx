import { Badge } from '@/components/ui/badge'

// Status words as the specs use them: Event status (EVT-F8), Order status (SAL-F4), Payment result (SAL-F12).
const tone: Record<string, string> = {
  Draft: 'bg-secondary text-secondary-foreground',
  Published: 'bg-success text-success-foreground',
  'On sale': 'bg-success text-success-foreground',
  'Off sale': 'bg-muted text-muted-foreground',
  'Pending payment': 'bg-warning text-warning-foreground',
  Confirmed: 'bg-success text-success-foreground',
  Expired: 'bg-muted text-muted-foreground',
  Paid: 'bg-success text-success-foreground',
  Declined: 'bg-destructive/10 text-destructive',
  Refunded: 'bg-info text-info-foreground',
  'Sold out': 'bg-muted text-muted-foreground',
}

export function StatusBadge({ status }: { status: string }) {
  return <Badge className={tone[status] ?? 'bg-secondary text-secondary-foreground'}>{status}</Badge>
}
