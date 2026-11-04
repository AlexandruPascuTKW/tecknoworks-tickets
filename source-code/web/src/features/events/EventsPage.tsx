import { CalendarPlus, Plus } from 'lucide-react'
import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { toast } from 'sonner'
import { EmptyState, Loadable } from '@/components/async-states'
import { FormDialog } from '@/components/form-dialog'
import { FormField, TextareaField, TextField } from '@/components/form-field'
import { PageHeader } from '@/components/page-header'
import { StatusBadge } from '@/components/status-badge'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { api } from '@/lib/api'
import { formatDate } from '@/lib/format'
import { useAsync } from '@/lib/use-async'
import { readImage, useSubmit } from '@/lib/use-submit'
import type { OrganizerEvent, Venue } from './types'

/** EVT-V2 — See the Organizer's Events, Draft or Published; EVT-A2 — Create an Event. Managers and Owners. */
export function EventsPage() {
  const { organizerId } = useParams()
  const events = useAsync(() => api.get<OrganizerEvent[]>(`/Events/Team?organizerId=${organizerId}`), [organizerId])
  const [creating, setCreating] = useState(false)

  return (
    <div className="grid gap-6">
      <PageHeader
        title="Events"
        description="Everything you are putting on, Draft or Published. Managers build the drafts; an owner publishes them."
        actions={
          <Button onClick={() => setCreating(true)}>
            <Plus />
            Create event
          </Button>
        }
      />

      <Loadable state={events}>
        {(list) =>
          list.length === 0 ? (
            <EmptyState
              icon={CalendarPlus}
              title="No events yet"
              action={<Button onClick={() => setCreating(true)}>Create event</Button>}
            >
              Create an event at one of your venues, then give it performances and ticket types.
            </EmptyState>
          ) : (
            <Card className="py-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Event name</TableHead>
                    <TableHead>Venue</TableHead>
                    <TableHead>Dates</TableHead>
                    <TableHead>Event status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {list.map((event) => (
                    <TableRow key={event.id}>
                      <TableCell className="font-medium">
                        <Link to={`/team/${organizerId}/events/${event.id}`} className="text-primary underline-offset-4 hover:underline">
                          {event.eventName}
                        </Link>
                      </TableCell>
                      <TableCell className="text-muted-foreground">{event.venueName}</TableCell>
                      <TableCell className="text-muted-foreground">
                        {event.performanceDates.length === 0
                          ? 'No performances yet'
                          : event.performanceDates.slice(0, 3).map(formatDate).join(' · ') +
                            (event.performanceDates.length > 3 ? ` · +${event.performanceDates.length - 3} more` : '')}
                      </TableCell>
                      <TableCell>
                        <StatusBadge status={event.eventStatus} />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Card>
          )
        }
      </Loadable>

      {creating && <CreateEventDialog organizerId={organizerId!} onClose={() => setCreating(false)} />}
    </div>
  )
}

function CreateEventDialog({ organizerId, onClose }: { organizerId: string; onClose: () => void }) {
  const navigate = useNavigate()
  const venues = useAsync(() => api.get<Venue[]>(`/Venues/All?organizerId=${organizerId}`), [organizerId])
  const [eventName, setEventName] = useState('')
  const [venueId, setVenueId] = useState('')
  const [description, setDescription] = useState('')
  const [file, setFile] = useState<File>()
  const submit = useSubmit()

  async function onSubmit() {
    const created = await submit.run(async () => {
      const image = file ? await readImage(file) : null
      const response = await api.post<{ id: string }>('/Events/Create', {
        organizerId,
        eventName,
        venueId: venueId || '00000000-0000-0000-0000-000000000000',
        description: description || null,
        image,
      })
      toast.success('Event created')
      navigate(`/team/${organizerId}/events/${response.id}`)
    })

    if (created) onClose()
  }

  const noVenues = venues.data !== undefined && venues.data.length === 0

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title="Create event"
      description="Start setting up a show at one of your venues. It stays a draft until an owner publishes it."
      submitLabel="Save"
      pending={submit.pending}
      error={submit.error}
      onSubmit={onSubmit}
    >
      <TextField label="Event name" value={eventName} onChange={(e) => setEventName(e.target.value)} error={submit.fieldError('eventname')} />

      <FormField
        label="Venue"
        error={submit.fieldError('venueid')}
        hint={noVenues ? 'Add a venue first — events are set up at one of your venues.' : undefined}
      >
        {(control) => (
          <Select value={venueId} onValueChange={setVenueId} disabled={noVenues}>
            <SelectTrigger {...control} className="w-full">
              <SelectValue placeholder={venues.loading ? 'Loading venues…' : 'Choose a venue'} />
            </SelectTrigger>
            <SelectContent>
              {venues.data?.map((venue) => (
                <SelectItem key={venue.id} value={venue.id}>
                  {venue.venueName} · {venue.capacity} people
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}
      </FormField>

      <TextareaField label="Description" rows={3} value={description} onChange={(e) => setDescription(e.target.value)} error={submit.fieldError('description')} />

      <FormField label="Image" hint="JPEG, PNG or WebP, up to 2 MB. Needed before the event can be published." error={submit.fieldError('image')}>
        {(control) => <Input {...control} type="file" accept="image/jpeg,image/png,image/webp" onChange={(e) => setFile(e.target.files?.[0])} />}
      </FormField>
    </FormDialog>
  )
}
