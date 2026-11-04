import { ArrowLeft, CalendarPlus, Loader2, Plus, Ticket } from 'lucide-react'
import { useState } from 'react'
import { Link, useParams } from 'react-router'
import { toast } from 'sonner'
import { EmptyState, Loadable, RowsSkeleton } from '@/components/async-states'
import { EventImage } from '@/components/event-image'
import { FormDialog, FormError } from '@/components/form-dialog'
import { FormField, TextareaField, TextField } from '@/components/form-field'
import { PageHeader } from '@/components/page-header'
import { StatusBadge } from '@/components/status-badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { api } from '@/lib/api'
import { teamRoleIn, useAuth } from '@/lib/auth'
import { formatDate, formatMoney, formatTime } from '@/lib/format'
import { useAsync } from '@/lib/use-async'
import { readImage, useSubmit } from '@/lib/use-submit'
import type { EventTeam, PerformanceDto, TicketTypeDto, Venue } from './types'

/**
 * EVT-V3 — See an Event as its Team, and everything a Team does to it: EVT-A3 to EVT-A9, EVT-A11 and, for
 * an Owner, EVT-A10. What may change once Published is EVT-R8; the host decides, this screen shows its answer.
 */
export function EventEditorPage() {
  const { organizerId, eventId } = useParams()
  const { account } = useAuth()
  const isOwner = teamRoleIn(account, organizerId) === 'Owner'
  const event = useAsync(() => api.get<EventTeam>(`/Events/TeamDetails/${eventId}`), [eventId])
  const [publishing, setPublishing] = useState(false)

  async function publish() {
    setPublishing(true)

    try {
      await api.post('/Events/Publish', { eventId })
      toast.success('Event published')
      event.reload()
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'That did not work.')
    } finally {
      setPublishing(false)
    }
  }

  return (
    <div className="grid gap-6">
      <Link to={`/team/${organizerId}/events`} className="inline-flex w-fit items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="size-4" />
        All events
      </Link>

      <Loadable state={event} skeleton={<RowsSkeleton rows={3} height="h-24" />}>
        {(data) => (
          <>
            <PageHeader
              title={data.eventName}
              description={
                <span className="flex flex-wrap items-center gap-2">
                  <StatusBadge status={data.eventStatus} />
                  <span>
                    {data.venueName} · capacity {data.capacity}
                  </span>
                </span>
              }
              actions={
                data.eventStatus === 'Draft' &&
                (isOwner ? (
                  <Button onClick={publish} disabled={publishing}>
                    {publishing && <Loader2 className="animate-spin" />}
                    Publish
                  </Button>
                ) : (
                  <p className="text-sm text-muted-foreground">Only an owner can publish this event.</p>
                ))
              }
            />

            <Tabs defaultValue="details">
              <TabsList>
                <TabsTrigger value="details">Details</TabsTrigger>
                <TabsTrigger value="performances">Performances</TabsTrigger>
                <TabsTrigger value="ticket-types">Ticket types</TabsTrigger>
              </TabsList>
              <TabsContent value="details" className="mt-4">
                <DetailsTab event={data} organizerId={organizerId!} onChanged={event.reload} />
              </TabsContent>
              <TabsContent value="performances" className="mt-4">
                <PerformancesTab event={data} organizerId={organizerId!} onChanged={event.reload} />
              </TabsContent>
              <TabsContent value="ticket-types" className="mt-4">
                <TicketTypesTab event={data} onChanged={event.reload} />
              </TabsContent>
            </Tabs>
          </>
        )}
      </Loadable>
    </div>
  )
}

// ------------------------------------------------------------------------------------------ Details

/** EVT-A3 — name, description and picture any time; the venue only while Draft (EVT-R8). */
function DetailsTab({ event, organizerId, onChanged }: { event: EventTeam; organizerId: string; onChanged: () => void }) {
  const venues = useAsync(() => api.get<Venue[]>(`/Venues/All?organizerId=${organizerId}`), [organizerId])
  const [eventName, setEventName] = useState(event.eventName)
  const [description, setDescription] = useState(event.description ?? '')
  const [venueId, setVenueId] = useState(event.venueId)
  const [file, setFile] = useState<File>()
  const submit = useSubmit()
  const isDraft = event.eventStatus === 'Draft'

  async function onSubmit() {
    const saved = await submit.run(async () => {
      await api.post('/Events/ChangeDetails', {
        eventId: event.id,
        eventName,
        description,
        image: file ? await readImage(file) : null,
        venueId: isDraft && venueId !== event.venueId ? venueId : null,
      })
    })

    if (saved) {
      toast.success('Event saved')
      setFile(undefined)
      onChanged()
    }
  }

  return (
    <Card>
      <CardContent className="grid gap-6 md:grid-cols-[minmax(0,18rem)_1fr]">
        <div className="grid content-start gap-2">
          <EventImage imageId={event.imageId} authenticated alt={`Picture of ${event.eventName}`} className="aspect-video w-full rounded-lg" />
          {!event.imageId && <p className="text-sm text-muted-foreground">No picture yet. One is needed before this event can be published.</p>}
        </div>

        <form
          className="grid gap-4"
          noValidate
          onSubmit={(e) => {
            e.preventDefault()
            void onSubmit()
          }}
        >
          <FormError error={submit.error} />
          <TextField label="Event name" value={eventName} onChange={(e) => setEventName(e.target.value)} error={submit.fieldError('eventname')} />
          <TextareaField label="Description" rows={4} value={description} onChange={(e) => setDescription(e.target.value)} error={submit.fieldError('description')} />

          <FormField
            label="Venue"
            hint={isDraft ? undefined : "A published event's venue can't be changed."}
            error={submit.fieldError('venueid')}
          >
            {(control) => (
              <Select value={venueId} onValueChange={setVenueId} disabled={!isDraft}>
                <SelectTrigger {...control} className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {(venues.data ?? [{ id: event.venueId, venueName: event.venueName, address: event.address, capacity: event.capacity }]).map((venue) => (
                    <SelectItem key={venue.id} value={venue.id}>
                      {venue.venueName} · {venue.capacity} people
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          </FormField>

          <FormField label="Image" hint="Choose a new picture to replace the current one. JPEG, PNG or WebP, up to 2 MB." error={submit.fieldError('image')}>
            {(control) => <Input {...control} type="file" accept="image/jpeg,image/png,image/webp" onChange={(e) => setFile(e.target.files?.[0])} />}
          </FormField>

          <div>
            <Button type="submit" disabled={submit.pending}>
              {submit.pending && <Loader2 className="animate-spin" />}
              Save
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

// ------------------------------------------------------------------------------------------ Performances

function PerformancesTab({ event, organizerId, onChanged }: { event: EventTeam; organizerId: string; onChanged: () => void }) {
  const [adding, setAdding] = useState(false)
  const [editing, setEditing] = useState<PerformanceDto>()

  /** EVT-A6 — removing is undone by adding the same date and times again. */
  async function remove(performance: PerformanceDto) {
    try {
      await api.post('/Events/RemovePerformance', { eventId: event.id, performanceId: performance.id })
      onChanged()
      toast.success('Performance removed', {
        action: {
          label: 'Undo',
          onClick: async () => {
            try {
              await api.post('/Events/AddPerformance', {
                eventId: event.id,
                date: performance.date,
                doorsTime: performance.doorsTime,
                startTime: performance.startTime,
              })
              onChanged()
            } catch (e) {
              toast.error(e instanceof Error ? e.message : "That couldn't be undone.")
            }
          },
        },
      })
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'That did not work.')
    }
  }

  return (
    <div className="grid gap-4">
      <div className="flex justify-end">
        <Button onClick={() => setAdding(true)}>
          <Plus />
          Add performance
        </Button>
      </div>

      {event.performances.length === 0 ? (
        <EmptyState icon={CalendarPlus} title="No performances yet" action={<Button onClick={() => setAdding(true)}>Add performance</Button>}>
          A performance is one dated occurrence of the event — "Friday 14th, 19:30". An event needs at least one before it can be published.
        </EmptyState>
      ) : (
        <Card className="py-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Date</TableHead>
                <TableHead>Doors time</TableHead>
                <TableHead>Start time</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {event.performances.map((performance) => (
                <TableRow key={performance.id}>
                  <TableCell className="font-medium">{formatDate(performance.date)}</TableCell>
                  <TableCell className="tabular-nums">{formatTime(performance.doorsTime)}</TableCell>
                  <TableCell className="tabular-nums">{formatTime(performance.startTime)}</TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-2">
                      {event.eventStatus === 'Published' && (
                        <Button variant="outline" size="sm" asChild>
                          <Link to={`/team/${organizerId}/door-list/${performance.id}`}>Door list</Link>
                        </Button>
                      )}
                      <Button variant="outline" size="sm" onClick={() => setEditing(performance)}>
                        Edit
                      </Button>
                      <Button variant="outline" size="sm" onClick={() => remove(performance)}>
                        Remove
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}

      {adding && (
        <PerformanceDialog
          eventId={event.id}
          onClose={() => setAdding(false)}
          onDone={() => {
            setAdding(false)
            onChanged()
          }}
        />
      )}
      {editing && (
        <PerformanceDialog
          eventId={event.id}
          performance={editing}
          onClose={() => setEditing(undefined)}
          onDone={() => {
            setEditing(undefined)
            onChanged()
          }}
        />
      )}
    </div>
  )
}

/** EVT-A4 (add) and EVT-A5 (move). Times are local at the venue. */
function PerformanceDialog({
  eventId,
  performance,
  onClose,
  onDone,
}: {
  eventId: string
  performance?: PerformanceDto
  onClose: () => void
  onDone: () => void
}) {
  const [date, setDate] = useState(performance?.date ?? '')
  const [doorsTime, setDoorsTime] = useState(performance ? formatTime(performance.doorsTime) : '')
  const [startTime, setStartTime] = useState(performance ? formatTime(performance.startTime) : '')
  const submit = useSubmit()

  async function onSubmit() {
    const body = { eventId, date, doorsTime: `${doorsTime}:00`, startTime: `${startTime}:00` }

    const saved = await submit.run(() =>
      performance
        ? api.post('/Events/ChangePerformance', { ...body, performanceId: performance.id })
        : api.post('/Events/AddPerformance', body),
    )

    if (saved) {
      toast.success(performance ? 'Performance saved' : 'Performance added')
      onDone()
    }
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={performance ? 'Edit performance' : 'Add performance'}
      description="Dates and times are local at the venue, and must still be to come."
      submitLabel="Save"
      pending={submit.pending}
      error={submit.error}
      onSubmit={onSubmit}
    >
      <TextField label="Date" type="date" value={date} onChange={(e) => setDate(e.target.value)} error={submit.fieldError('date')} />
      <div className="grid grid-cols-2 gap-4">
        <TextField label="Doors time" type="time" value={doorsTime} onChange={(e) => setDoorsTime(e.target.value)} error={submit.fieldError('doorstime')} />
        <TextField label="Start time" type="time" value={startTime} onChange={(e) => setStartTime(e.target.value)} error={submit.fieldError('starttime')} />
      </div>
    </FormDialog>
  )
}

// ------------------------------------------------------------------------------------------ Ticket types

function TicketTypesTab({ event, onChanged }: { event: EventTeam; onChanged: () => void }) {
  const [adding, setAdding] = useState(false)
  const [editing, setEditing] = useState<TicketTypeDto>()
  const allocated = event.ticketTypes.reduce((sum, t) => sum + t.quantity, 0)

  /** EVT-A9 — removing is undone by adding the same Ticket type again. */
  async function remove(ticketType: TicketTypeDto) {
    try {
      await api.post('/Events/RemoveTicketType', { eventId: event.id, ticketTypeId: ticketType.id })
      onChanged()
      toast.success('Ticket type removed', {
        action: {
          label: 'Undo',
          onClick: async () => {
            try {
              await api.post('/Events/AddTicketType', {
                eventId: event.id,
                ticketTypeName: ticketType.ticketTypeName,
                price: ticketType.price,
                quantity: ticketType.quantity,
              })
              onChanged()
            } catch (e) {
              toast.error(e instanceof Error ? e.message : "That couldn't be undone.")
            }
          },
        },
      })
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'That did not work.')
    }
  }

  /** EVT-A11 — the Ticket type stays on the Event for the tickets already bought. */
  async function takeOffSale(ticketType: TicketTypeDto) {
    try {
      await api.post('/Events/TakeTicketTypeOffSale', { eventId: event.id, ticketTypeId: ticketType.id })
      toast.success('Ticket type taken off sale')
      onChanged()
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'That did not work.')
    }
  }

  return (
    <div className="grid gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <p className="text-sm text-muted-foreground">
          Every performance offers each ticket type in full. Quantities add up to{' '}
          <span className="font-medium text-foreground tabular-nums">
            {allocated} of {event.capacity}
          </span>{' '}
          — never more than the venue holds.
        </p>
        <Button onClick={() => setAdding(true)}>
          <Plus />
          Add ticket type
        </Button>
      </div>

      {event.ticketTypes.length === 0 ? (
        <EmptyState icon={Ticket} title="No ticket types yet" action={<Button onClick={() => setAdding(true)}>Add ticket type</Button>}>
          A ticket type is a price level — "Standard €28", "Concession €22". An event needs at least one before it can be published.
        </EmptyState>
      ) : (
        <Card className="py-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Ticket type name</TableHead>
                <TableHead className="text-right">Price</TableHead>
                <TableHead className="text-right">Quantity</TableHead>
                <TableHead>Sale</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {event.ticketTypes.map((ticketType) => (
                <TableRow key={ticketType.id}>
                  <TableCell className="font-medium">{ticketType.ticketTypeName}</TableCell>
                  <TableCell className="text-right tabular-nums">{formatMoney(ticketType.price)}</TableCell>
                  <TableCell className="text-right tabular-nums">{ticketType.quantity}</TableCell>
                  <TableCell>
                    <StatusBadge status={ticketType.onSale ? 'On sale' : 'Off sale'} />
                  </TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-2">
                      <Button variant="outline" size="sm" onClick={() => setEditing(ticketType)}>
                        Edit
                      </Button>
                      {ticketType.onSale && (
                        <Button variant="outline" size="sm" onClick={() => takeOffSale(ticketType)}>
                          Take off sale
                        </Button>
                      )}
                      <Button variant="outline" size="sm" onClick={() => remove(ticketType)}>
                        Remove
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}

      {adding && (
        <TicketTypeDialog
          eventId={event.id}
          onClose={() => setAdding(false)}
          onDone={() => {
            setAdding(false)
            onChanged()
          }}
        />
      )}
      {editing && (
        <TicketTypeDialog
          eventId={event.id}
          ticketType={editing}
          onClose={() => setEditing(undefined)}
          onDone={() => {
            setEditing(undefined)
            onChanged()
          }}
        />
      )}
    </div>
  )
}

/** EVT-A7 (add) and EVT-A8 (change). */
function TicketTypeDialog({
  eventId,
  ticketType,
  onClose,
  onDone,
}: {
  eventId: string
  ticketType?: TicketTypeDto
  onClose: () => void
  onDone: () => void
}) {
  const [name, setName] = useState(ticketType?.ticketTypeName ?? '')
  const [price, setPrice] = useState(ticketType ? String(ticketType.price) : '')
  const [quantity, setQuantity] = useState(ticketType ? String(ticketType.quantity) : '')
  const submit = useSubmit()

  async function onSubmit() {
    const body = { eventId, ticketTypeName: name, price: Number(price), quantity: Number(quantity) }

    const saved = await submit.run(() =>
      ticketType
        ? api.post('/Events/ChangeTicketType', { ...body, ticketTypeId: ticketType.id })
        : api.post('/Events/AddTicketType', body),
    )

    if (saved) {
      toast.success(ticketType ? 'Ticket type saved' : 'Ticket type added')
      onDone()
    }
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={ticketType ? 'Edit ticket type' : 'Add ticket type'}
      submitLabel="Save"
      pending={submit.pending}
      error={submit.error}
      onSubmit={onSubmit}
    >
      <TextField label="Ticket type name" value={name} onChange={(e) => setName(e.target.value)} error={submit.fieldError('tickettypename')} />
      <div className="grid grid-cols-2 gap-4">
        <TextField
          label="Price"
          type="number"
          inputMode="decimal"
          step="0.01"
          min="0"
          hint="In euros, to two decimals."
          value={price}
          onChange={(e) => setPrice(e.target.value)}
          error={submit.fieldError('price')}
        />
        <TextField
          label="Quantity"
          type="number"
          inputMode="numeric"
          min="1"
          hint="Per performance."
          value={quantity}
          onChange={(e) => setQuantity(e.target.value)}
          error={submit.fieldError('quantity')}
        />
      </div>
    </FormDialog>
  )
}
