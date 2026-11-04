import { CalendarDays, MapPin, Ticket } from 'lucide-react'
import { Link } from 'react-router'
import { EmptyState, Loadable } from '@/components/async-states'
import { EventImage } from '@/components/event-image'
import { PageHeader } from '@/components/page-header'
import { Skeleton } from '@/components/ui/skeleton'
import { api } from '@/lib/api'
import { formatDate } from '@/lib/format'
import { useAsync } from '@/lib/use-async'
import type { WhatsOnItem } from './types'

function CardsSkeleton() {
  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3" aria-busy="true" aria-label="Loading">
      {Array.from({ length: 3 }, (_, i) => (
        <Skeleton key={i} className="h-64 w-full rounded-xl" />
      ))}
    </div>
  )
}

/** EVT-V4 — Find what's on: the Published Events, for anyone, signed in or not. */
export function WhatsOnPage() {
  const events = useAsync(() => api.get<WhatsOnItem[]>('/WhatsOn/List'), [])

  return (
    <div className="grid gap-6">
      <PageHeader title="What's on" description="Shows on sale now. Pick one to see its dates and choose your tickets." />

      <Loadable state={events} skeleton={<CardsSkeleton />}>
        {(list) =>
          list.length === 0 ? (
            <EmptyState icon={Ticket} title="Nothing on sale yet">
              Organizers publish their events here. Check back soon.
            </EmptyState>
          ) : (
            <ul className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
              {list.map((event) => (
                <li key={event.id}>
                  <Link
                    to={`/events/${event.id}`}
                    className="group flex h-full flex-col overflow-hidden rounded-xl border bg-card transition-shadow hover:shadow-md focus-visible:ring-3 focus-visible:ring-ring/50 focus-visible:outline-none motion-reduce:transition-none"
                  >
                    <EventImage imageId={event.imageId} alt="" className="aspect-video w-full" />
                    <div className="grid flex-1 content-start gap-2 p-4">
                      <h3 className="text-lg font-semibold text-heading group-hover:text-primary">{event.eventName}</h3>
                      <p className="flex items-center gap-1.5 text-sm text-muted-foreground">
                        <MapPin className="size-4 shrink-0" />
                        {event.venueName}
                      </p>
                      <p className="flex items-start gap-1.5 text-sm text-muted-foreground">
                        <CalendarDays className="mt-0.5 size-4 shrink-0" />
                        {event.performanceDates.length === 0
                          ? 'Dates to be announced'
                          : event.performanceDates.slice(0, 3).map(formatDate).join(' · ') +
                            (event.performanceDates.length > 3 ? ` · +${event.performanceDates.length - 3} more` : '')}
                      </p>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )
        }
      </Loadable>
    </div>
  )
}
