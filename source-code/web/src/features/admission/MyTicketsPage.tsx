import { MapPin, Ticket } from 'lucide-react'
import { Link } from 'react-router'
import { EmptyState, Loadable } from '@/components/async-states'
import { PageHeader } from '@/components/page-header'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { api } from '@/lib/api'
import { formatDate, formatTime } from '@/lib/format'
import { useAsync } from '@/lib/use-async'

/** ADM-V1 */
type MyTicket = {
  id: string
  ticketCode: string
  ticketTypeName: string
  performanceId: string
  eventName: string
  venueName: string
  date: string
  doorsTime: string
  startTime: string
}

/** ADM-V1 — See my Tickets: every ticket bought, with its code and where and when it gets the person in. */
export function MyTicketsPage() {
  const tickets = useAsync(() => api.get<MyTicket[]>('/Tickets/Mine'), [])

  return (
    <div className="grid gap-6">
      <PageHeader title="My tickets" description="Every ticket you have bought. Show the code at the door." />

      <Loadable state={tickets}>
        {(list) => {
          if (list.length === 0) {
            return (
              <EmptyState
                icon={Ticket}
                title="No tickets yet"
                action={
                  <Button asChild>
                    <Link to="/whats-on">See what's on</Link>
                  </Button>
                }
              >
                Tickets you buy appear here, one for every person getting in. If you have just paid, give it a moment.
              </EmptyState>
            )
          }

          // One card per performance, in the order the host sends them (soonest first).
          const groups = list.reduce<MyTicket[][]>((all, ticket) => {
            const group = all.find((g) => g[0].performanceId === ticket.performanceId)
            if (group) group.push(ticket)
            else all.push([ticket])
            return all
          }, [])

          return (
            <div className="grid gap-4 lg:grid-cols-2">
              {groups.map((group) => {
                const first = group[0]

                return (
                  <Card key={first.performanceId}>
                    <CardHeader>
                      <CardTitle className="text-lg">{first.eventName}</CardTitle>
                      <p className="flex items-center gap-1.5 text-sm text-muted-foreground">
                        <MapPin className="size-4" />
                        {first.venueName}
                      </p>
                      <p className="text-sm text-muted-foreground tabular-nums">
                        {formatDate(first.date)} · Doors {formatTime(first.doorsTime)} · Starts {formatTime(first.startTime)}
                      </p>
                    </CardHeader>
                    <CardContent>
                      <ul className="grid gap-2">
                        {group.map((ticket) => (
                          <li key={ticket.id} className="flex items-center justify-between gap-3 rounded-lg border bg-muted/40 px-3 py-2">
                            <span className="font-mono text-lg font-semibold tracking-widest">{ticket.ticketCode}</span>
                            <Badge variant="secondary">{ticket.ticketTypeName}</Badge>
                          </li>
                        ))}
                      </ul>
                    </CardContent>
                  </Card>
                )
              })}
            </div>
          )
        }}
      </Loadable>
    </div>
  )
}
