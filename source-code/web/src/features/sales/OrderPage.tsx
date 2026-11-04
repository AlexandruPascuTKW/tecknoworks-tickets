import { AlertCircle, CheckCircle2, Clock, Loader2, Undo2 } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { Loadable, RowsSkeleton } from '@/components/async-states'
import { PageHeader } from '@/components/page-header'
import { StatusBadge } from '@/components/status-badge'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { api } from '@/lib/api'
import { formatClock, formatLocalDateTime, formatMoney } from '@/lib/format'
import { useAsync } from '@/lib/use-async'
import { useSubmit } from '@/lib/use-submit'

/** SAL-V1 */
export type Order = {
  id: string
  performanceId: string
  eventName: string
  performanceStartsAt: string
  orderStatus: 'Pending payment' | 'Confirmed' | 'Expired'
  placedAt: string
  holdEndsAt: string
  lines: { ticketTypeName: string; numberOfTickets: number; ticketPrice: number }[]
  faceValue: number
  bookingFee: number
  orderTotal: number
  paymentResult: 'Paid' | 'Declined' | 'Refunded' | null
}

function useNow(intervalMs: number) {
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    const timer = setInterval(() => setNow(Date.now()), intervalMs)
    return () => clearInterval(timer)
  }, [intervalMs])

  return now
}

function countdown(ms: number): string {
  const seconds = Math.max(0, Math.ceil(ms / 1000))
  return `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}`
}

/**
 * SAL-V1 — See an Order in checkout: what is being bought, the Face value, Booking fee and Order total each
 * on their own (SAL-R10), and how long the tickets stay held. SAL-A2 — Pay for it.
 */
export function OrderPage() {
  const { orderId } = useParams()
  const order = useAsync(() => api.get<Order>(`/Orders/Checkout/${orderId}`), [orderId])

  return (
    <div className="mx-auto grid w-full max-w-3xl gap-6">
      <Loadable state={order} skeleton={<RowsSkeleton rows={3} height="h-28" />}>
        {(data) => <OrderView order={data} reload={order.reload} />}
      </Loadable>
    </div>
  )
}

function OrderView({ order, reload }: { order: Order; reload: () => void }) {
  const navigate = useNavigate()
  const now = useNow(1000)
  const submit = useSubmit()
  const remaining = new Date(order.holdEndsAt).getTime() - now
  const pending = order.orderStatus === 'Pending payment'
  const lapsed = pending && remaining <= 0

  // When the Hold ends, ask again: the order is Expired from that moment (SAL-R5).
  useEffect(() => {
    if (lapsed) reload()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lapsed])

  async function pay() {
    let redirectUrl = ''

    const started = await submit.run(async () => {
      redirectUrl = (await api.post<{ redirectUrl: string }>(`/Orders/Pay/${order.id}`)).redirectUrl
    })

    if (!started) return

    // Card details are entered only on the Payment provider's own page (SAL-R11).
    if (redirectUrl.startsWith('/')) navigate(redirectUrl)
    else window.location.assign(redirectUrl)
  }

  return (
    <>
      <PageHeader
        title="Your order"
        description={`${order.eventName} · ${formatLocalDateTime(order.performanceStartsAt)}`}
        actions={<StatusBadge status={lapsed ? 'Expired' : order.orderStatus} />}
      />

      {order.orderStatus === 'Confirmed' && (
        <Alert className="border-success/40">
          <CheckCircle2 className="text-success" />
          <AlertTitle>Order confirmed</AlertTitle>
          <AlertDescription>
            Your payment went through. Your tickets are on their way to{' '}
            <Link to="/my-tickets" className="font-medium text-primary underline-offset-4 hover:underline">
              My tickets
            </Link>
            ; give it a moment if they are not there yet.
          </AlertDescription>
        </Alert>
      )}

      {order.orderStatus === 'Expired' && order.paymentResult === 'Refunded' && (
        <Alert variant="destructive">
          <Undo2 />
          <AlertTitle>Payment refunded: these tickets were no longer available</AlertTitle>
          <AlertDescription>
            Your hold ran out before your card was approved and someone else bought the tickets. The whole payment has been
            refunded to your card.
          </AlertDescription>
        </Alert>
      )}

      {(order.orderStatus === 'Expired' || lapsed) && order.paymentResult !== 'Refunded' && (
        <Alert>
          <Clock />
          <AlertTitle>This order has expired</AlertTitle>
          <AlertDescription>
            The hold ran out and the tickets went back on sale.{' '}
            <Link to="/whats-on" className="font-medium text-primary underline-offset-4 hover:underline">
              Choose tickets again
            </Link>
            .
          </AlertDescription>
        </Alert>
      )}

      {pending && !lapsed && order.paymentResult === 'Declined' && (
        <Alert variant="destructive">
          <AlertCircle />
          <AlertTitle>Payment declined</AlertTitle>
          <AlertDescription>Your card was refused. Your tickets are still held, so you can try again.</AlertDescription>
        </Alert>
      )}

      {pending && !lapsed && (
        <Card className="border-warning/60 bg-warning/10">
          <CardContent className="flex flex-wrap items-center justify-between gap-3">
            <div className="grid gap-0.5">
              <p className="flex items-center gap-1.5 text-sm font-medium">
                <Clock className="size-4" />
                Tickets held until {formatClock(order.holdEndsAt)}
              </p>
              <p className="text-sm text-muted-foreground" aria-live="off">
                <span className="font-semibold text-foreground tabular-nums">{countdown(remaining)}</span> left to pay
              </p>
            </div>
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Tickets</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Ticket type</TableHead>
                <TableHead className="text-right">Number of tickets</TableHead>
                <TableHead className="text-right">Ticket price</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {order.lines.map((line) => (
                <TableRow key={line.ticketTypeName}>
                  <TableCell className="font-medium">{line.ticketTypeName}</TableCell>
                  <TableCell className="text-right tabular-nums">{line.numberOfTickets}</TableCell>
                  <TableCell className="text-right tabular-nums">{formatMoney(line.ticketPrice)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>

          {/* SAL-R10: each amount stated on its own, before paying. */}
          <dl className="mt-4 grid gap-2 border-t pt-4 text-sm">
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Face value</dt>
              <dd className="tabular-nums">{formatMoney(order.faceValue)}</dd>
            </div>
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Booking fee</dt>
              <dd className="tabular-nums">{formatMoney(order.bookingFee)}</dd>
            </div>
            <div className="flex justify-between text-base font-semibold">
              <dt>Order total</dt>
              <dd className="tabular-nums">{formatMoney(order.orderTotal)}</dd>
            </div>
          </dl>
        </CardContent>

        {pending && !lapsed && (
          <CardFooter className="flex-col items-stretch gap-3">
            {submit.error && (
              <Alert variant="destructive">
                <AlertCircle />
                <AlertDescription>{submit.error.message}</AlertDescription>
              </Alert>
            )}
            <Button size="lg" onClick={pay} disabled={submit.pending}>
              {submit.pending && <Loader2 className="animate-spin" />}
              Pay
            </Button>
            <p className="text-center text-xs text-muted-foreground">
              You enter your card on our payment provider's own page. We never see your card number.
            </p>
          </CardFooter>
        )}
      </Card>

      {order.paymentResult && (
        <p className="flex items-center gap-2 text-sm text-muted-foreground">
          Latest payment <StatusBadge status={order.paymentResult} />
        </p>
      )}
    </>
  )
}
