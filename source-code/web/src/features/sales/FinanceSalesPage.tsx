import { Receipt } from 'lucide-react'
import { useNavigate, useParams } from 'react-router'
import { EmptyState, Loadable, RowsSkeleton } from '@/components/async-states'
import { FormField } from '@/components/form-field'
import { PageHeader } from '@/components/page-header'
import { StatusBadge } from '@/components/status-badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { api } from '@/lib/api'
import { formatLocalDateTime, formatMoment, formatMoney } from '@/lib/format'
import { useAsync } from '@/lib/use-async'

type PerformanceItem = { performanceId: string; eventName: string; startsAt: string }

type SoldOrder = {
  orderId: string
  attendeeName: string
  attendeeEmail: string
  placedAt: string
  orderStatus: string
  lines: { ticketTypeName: string; numberOfTickets: number; ticketPrice: number }[]
  faceValue: number
  bookingFee: number
  orderTotal: number
  paymentResult: string | null
}

/** SAL-V3 */
type PerformanceSales = {
  performanceId: string
  eventName: string
  performanceStartsAt: string
  orders: SoldOrder[]
  ticketsSold: { ticketTypeName: string; numberOfTickets: number }[]
  totalFaceValue: number
  totalBookingFee: number
}

/** SAL-V3 — See a Performance's sales, to reconcile it. Finance manager only; every Organizer's. */
export function FinanceSalesPage() {
  const { performanceId } = useParams()
  const navigate = useNavigate()
  const performances = useAsync(() => api.get<PerformanceItem[]>('/Performances/All'), [])

  return (
    <div className="grid gap-6">
      <PageHeader
        title="Sales"
        description="Everything sold for one performance, to reconcile it. Pick a performance to see its orders and totals."
      />

      <Loadable state={performances} skeleton={<RowsSkeleton rows={1} height="h-16" />}>
        {(list) =>
          list.length === 0 ? (
            <EmptyState icon={Receipt} title="Nothing to reconcile yet">
              Performances appear here once an organizer publishes an event.
            </EmptyState>
          ) : (
            <>
              <div className="max-w-md">
                <FormField label="Performance">
                  {(control) => (
                    <Select value={performanceId ?? ''} onValueChange={(id) => navigate(`/finance/sales/${id}`)}>
                      <SelectTrigger {...control} className="w-full">
                        <SelectValue placeholder="Choose a performance" />
                      </SelectTrigger>
                      <SelectContent>
                        {list.map((p) => (
                          <SelectItem key={p.performanceId} value={p.performanceId}>
                            {p.eventName} · {formatLocalDateTime(p.startsAt)}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                </FormField>
              </div>

              {performanceId && <SalesView performanceId={performanceId} />}
            </>
          )
        }
      </Loadable>
    </div>
  )
}

function SalesView({ performanceId }: { performanceId: string }) {
  const sales = useAsync(() => api.get<PerformanceSales>(`/Performances/Sales/${performanceId}`), [performanceId])

  return (
    <Loadable state={sales} skeleton={<RowsSkeleton rows={2} height="h-32" />}>
      {(data) => (
        <div className="grid gap-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">
                {data.eventName} · {formatLocalDateTime(data.performanceStartsAt)}
              </CardTitle>
            </CardHeader>
            <CardContent className="grid gap-4 sm:grid-cols-3">
              <div>
                <p className="text-sm text-muted-foreground">Tickets sold</p>
                {data.ticketsSold.length === 0 ? (
                  <p className="text-lg font-semibold">—</p>
                ) : (
                  <ul className="text-sm">
                    {data.ticketsSold.map((t) => (
                      <li key={t.ticketTypeName} className="tabular-nums">
                        <span className="text-lg font-semibold">{t.numberOfTickets}</span> {t.ticketTypeName}
                      </li>
                    ))}
                  </ul>
                )}
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total face value</p>
                <p className="text-lg font-semibold tabular-nums">{formatMoney(data.totalFaceValue)}</p>
                <p className="text-xs text-muted-foreground">Goes to the organizer</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total booking fee</p>
                <p className="text-lg font-semibold tabular-nums">{formatMoney(data.totalBookingFee)}</p>
                <p className="text-xs text-muted-foreground">Kept by TecknoworksTickets</p>
              </div>
            </CardContent>
          </Card>

          {data.orders.length === 0 ? (
            <EmptyState icon={Receipt} title="No sales yet">
              Confirmed orders for this performance — and any whose late payment was refunded — are listed here.
            </EmptyState>
          ) : (
            <Card className="py-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Attendee</TableHead>
                    <TableHead>Placed at</TableHead>
                    <TableHead>Order status</TableHead>
                    <TableHead>Tickets</TableHead>
                    <TableHead className="text-right">Face value</TableHead>
                    <TableHead className="text-right">Booking fee</TableHead>
                    <TableHead className="text-right">Order total</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.orders.map((order) => (
                    <TableRow key={order.orderId}>
                      <TableCell>
                        <p className="font-medium">{order.attendeeName}</p>
                        <p className="text-xs text-muted-foreground">{order.attendeeEmail}</p>
                      </TableCell>
                      <TableCell className="whitespace-nowrap text-muted-foreground">{formatMoment(order.placedAt)}</TableCell>
                      <TableCell>
                        <div className="flex flex-wrap items-center gap-1.5">
                          <StatusBadge status={order.orderStatus} />
                          {order.paymentResult === 'Refunded' && <StatusBadge status="Refunded" />}
                        </div>
                      </TableCell>
                      <TableCell className="text-sm">
                        {order.lines.map((l) => (
                          <p key={l.ticketTypeName} className="tabular-nums">
                            {l.numberOfTickets} × {l.ticketTypeName} at {formatMoney(l.ticketPrice)}
                          </p>
                        ))}
                      </TableCell>
                      <TableCell className="text-right tabular-nums">{formatMoney(order.faceValue)}</TableCell>
                      <TableCell className="text-right tabular-nums">{formatMoney(order.bookingFee)}</TableCell>
                      <TableCell className="text-right tabular-nums">{formatMoney(order.orderTotal)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Card>
          )}
        </div>
      )}
    </Loadable>
  )
}
