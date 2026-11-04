import { ArrowLeft, Printer, Users } from 'lucide-react'
import { Link, useParams } from 'react-router'
import { EmptyState, Loadable } from '@/components/async-states'
import { PageHeader } from '@/components/page-header'
import { Button } from '@/components/ui/button'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { api } from '@/lib/api'
import { formatDate, formatTime } from '@/lib/format'
import { useAsync } from '@/lib/use-async'

/** ADM-V2 */
type DoorList = {
  performanceId: string
  eventName: string
  date: string
  startTime: string
  tickets: { ticketCode: string; ticketTypeName: string; buyersName: string }[]
}

/**
 * ADM-V2 — See a Performance's Door list, ready to print for the Front of house team to tick off at the door.
 * The sidebar and header hide themselves when printing (see App.tsx), leaving just the list.
 */
export function DoorListPage() {
  const { organizerId, performanceId } = useParams()
  const doorList = useAsync(() => api.get<DoorList>(`/Tickets/DoorList/${performanceId}`), [performanceId])

  return (
    <div className="grid gap-6">
      <Link
        to={`/team/${organizerId}/events`}
        className="inline-flex w-fit items-center gap-1 text-sm text-muted-foreground hover:text-foreground print:hidden"
      >
        <ArrowLeft className="size-4" />
        Events
      </Link>

      <Loadable state={doorList}>
        {(data) => (
          <>
            <PageHeader
              title="Door list"
              description={`${data.eventName} · ${formatDate(data.date)}, starts ${formatTime(data.startTime)} · ${data.tickets.length} ${data.tickets.length === 1 ? 'ticket' : 'tickets'}`}
              actions={
                <Button onClick={() => window.print()}>
                  <Printer />
                  Print
                </Button>
              }
            />

            {data.tickets.length === 0 ? (
              <EmptyState icon={Users} title="No tickets yet">
                Tickets appear on the door list as soon as an order is confirmed. Nothing has been bought for this performance yet.
              </EmptyState>
            ) : (
              <Table className="print:text-sm">
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-10 print:w-8">
                      <span className="sr-only">Arrived</span>
                    </TableHead>
                    <TableHead>Ticket code</TableHead>
                    <TableHead>Ticket type</TableHead>
                    <TableHead>Buyer's name</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.tickets.map((ticket) => (
                    <TableRow key={ticket.ticketCode}>
                      <TableCell>
                        <span className="block size-4 rounded-sm border border-foreground/60" aria-hidden="true" />
                      </TableCell>
                      <TableCell className="font-mono font-semibold tracking-widest">{ticket.ticketCode}</TableCell>
                      <TableCell>{ticket.ticketTypeName}</TableCell>
                      <TableCell>{ticket.buyersName}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </>
        )}
      </Loadable>
    </div>
  )
}
