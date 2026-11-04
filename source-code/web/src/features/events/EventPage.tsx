import { ArrowLeft, CalendarDays, MapPin } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router'
import { Loadable, RowsSkeleton } from '@/components/async-states'
import { EventImage } from '@/components/event-image'
import { FormDialog } from '@/components/form-dialog'
import { FormField } from '@/components/form-field'
import { PageHeader } from '@/components/page-header'
import { StatusBadge } from '@/components/status-badge'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { api } from '@/lib/api'
import { useAuth } from '@/lib/auth'
import { formatDate, formatMoney, formatTime } from '@/lib/format'
import { useAsync } from '@/lib/use-async'
import { useSubmit } from '@/lib/use-submit'
import type { PerformanceDto, PublicEvent } from './types'

/** SAL-V2 — whether each Performance and each Ticket type is Sold out. Never how many are left (SAL-R14). */
type Availability = {
  performanceId: string
  soldOut: boolean
  ticketTypes: { ticketTypeId: string; ticketTypeName: string; soldOut: boolean }[]
}

const MAX_TICKETS = 8 // SAL-R4

/** EVT-V5 — See an Event on sale, with what's left (SAL-V2) and the way into checkout (SAL-A1). */
export function EventPage() {
  const { eventId } = useParams()
  const event = useAsync(() => api.get<PublicEvent>(`/WhatsOn/Details/${eventId}`), [eventId])
  const availability = useAsync(() => api.get<Availability[]>(`/Performances/Availability?eventId=${eventId}`), [eventId])
  const [buying, setBuying] = useState<PerformanceDto>()

  // Holds run out and free tickets again, so keep what's left fresh while the page is open.
  useEffect(() => {
    const timer = setInterval(availability.reload, 10_000)
    return () => clearInterval(timer)
  }, [availability.reload])

  return (
    <div className="grid gap-6">
      <Link to="/whats-on" className="inline-flex w-fit items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="size-4" />
        What's on
      </Link>

      <Loadable state={event} skeleton={<RowsSkeleton rows={3} height="h-32" />}>
        {(data) => {
          const soldOut = (performanceId: string) => availability.data?.find((a) => a.performanceId === performanceId)

          return (
            <>
              <div className="grid gap-6 lg:grid-cols-[minmax(0,26rem)_1fr]">
                <EventImage imageId={data.imageId} alt={`Picture of ${data.eventName}`} className="aspect-video w-full rounded-xl" />
                <div className="grid content-start gap-3">
                  <PageHeader title={data.eventName} />
                  <p className="flex items-center gap-1.5 text-sm text-muted-foreground">
                    <MapPin className="size-4 shrink-0" />
                    {data.venueName} · {data.address}
                  </p>
                  {data.description && <p className="max-w-prose whitespace-pre-line">{data.description}</p>}
                  <ul className="flex flex-wrap gap-2">
                    {data.ticketTypes.map((ticketType) => (
                      <li key={ticketType.id}>
                        <Badge variant="outline" className="h-6 gap-1.5 px-2.5 text-sm">
                          {ticketType.ticketTypeName}
                          <span className="font-semibold tabular-nums">{formatMoney(ticketType.price)}</span>
                        </Badge>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>

              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2 text-lg">
                    <CalendarDays className="size-5 text-primary" />
                    Performances
                  </CardTitle>
                </CardHeader>
                <CardContent className="grid gap-2">
                  {data.performances.length === 0 && <p className="text-sm text-muted-foreground">Dates to be announced.</p>}
                  {data.performances.map((performance) => {
                    const state = soldOut(performance.id)

                    return (
                      <div key={performance.id} className="flex flex-wrap items-center justify-between gap-3 rounded-lg border p-3">
                        <div className="grid gap-0.5">
                          <p className="font-medium">{formatDate(performance.date)}</p>
                          <p className="text-sm text-muted-foreground tabular-nums">
                            Doors {formatTime(performance.doorsTime)} · Starts {formatTime(performance.startTime)}
                          </p>
                        </div>
                        {state?.soldOut ? <StatusBadge status="Sold out" /> : <Button onClick={() => setBuying(performance)}>Buy tickets</Button>}
                      </div>
                    )
                  })}
                </CardContent>
              </Card>

              {buying && (
                <BuyDialog
                  event={data}
                  performance={buying}
                  availability={soldOut(buying.id)}
                  onClose={() => setBuying(undefined)}
                  onRefused={availability.reload}
                />
              )}
            </>
          )
        }}
      </Loadable>
    </div>
  )
}

/** SAL-A1 — choose how many of each Ticket type; up to eight in all (SAL-R4). */
function BuyDialog({
  event,
  performance,
  availability,
  onClose,
  onRefused,
}: {
  event: PublicEvent
  performance: PerformanceDto
  availability: Availability | undefined
  onClose: () => void
  onRefused: () => void
}) {
  const { account } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [quantities, setQuantities] = useState<Record<string, string>>({})
  const submit = useSubmit()

  const total = Object.values(quantities).reduce((sum, q) => sum + (Number(q) || 0), 0)

  async function onSubmit() {
    // Buying needs an account: that is where a person's tickets live.
    if (!account) {
      navigate('/sign-in', { state: { from: location.pathname } })
      return
    }

    const lines = Object.entries(quantities)
      .filter(([, q]) => Number(q) > 0)
      .map(([ticketTypeId, q]) => ({ ticketTypeId, numberOfTickets: Number(q) }))

    let orderId = ''
    const started = await submit.run(async () => {
      orderId = (await api.post<{ orderId: string }>('/Orders/StartCheckout', { performanceId: performance.id, lines })).orderId
    })

    if (started) {
      navigate(`/orders/${orderId}`)
    } else {
      onRefused()
    }
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title="Buy tickets"
      description={`${event.eventName} · ${formatDate(performance.date)}, ${formatTime(performance.startTime)}. Your tickets are held for ten minutes while you pay.`}
      submitLabel="Check out"
      pending={submit.pending}
      error={submit.error}
      onSubmit={onSubmit}
    >
      {event.ticketTypes.map((ticketType) => {
        const ticketSoldOut = availability?.ticketTypes.find((t) => t.ticketTypeId === ticketType.id)?.soldOut ?? false

        return (
          <FormField
            key={ticketType.id}
            label={ticketType.ticketTypeName}
            hint={ticketSoldOut ? 'Sold out' : `${formatMoney(ticketType.price)} each`}
          >
            {(control) => (
              <Input
                {...control}
                type="number"
                inputMode="numeric"
                min={0}
                max={MAX_TICKETS}
                disabled={ticketSoldOut}
                value={quantities[ticketType.id] ?? ''}
                placeholder="0"
                onChange={(e) => setQuantities((q) => ({ ...q, [ticketType.id]: e.target.value }))}
              />
            )}
          </FormField>
        )
      })}

      <p className={total > MAX_TICKETS ? 'text-sm text-destructive' : 'text-sm text-muted-foreground'} aria-live="polite">
        {total} of {MAX_TICKETS} tickets
        {account ? '' : ' · you will be asked to sign in first'}
      </p>
    </FormDialog>
  )
}
