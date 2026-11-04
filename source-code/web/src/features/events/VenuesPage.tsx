import { MapPin, Plus } from 'lucide-react'
import { useState } from 'react'
import { useParams } from 'react-router'
import { toast } from 'sonner'
import { EmptyState, Loadable } from '@/components/async-states'
import { FormDialog } from '@/components/form-dialog'
import { TextField } from '@/components/form-field'
import { PageHeader } from '@/components/page-header'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { api } from '@/lib/api'
import { useAsync } from '@/lib/use-async'
import { useSubmit } from '@/lib/use-submit'
import type { Venue } from './types'

/** EVT-V1 — See the Organizer's Venues; EVT-A1 — Add a Venue. Managers and Owners. */
export function VenuesPage() {
  const { organizerId } = useParams()
  const venues = useAsync(() => api.get<Venue[]>(`/Venues/All?organizerId=${organizerId}`), [organizerId])
  const [adding, setAdding] = useState(false)

  return (
    <div className="grid gap-6">
      <PageHeader
        title="Venues"
        description="The places you hold performances. Each venue is yours alone, and can't be changed once added."
        actions={
          <Button onClick={() => setAdding(true)}>
            <Plus />
            Add venue
          </Button>
        }
      />

      <Loadable state={venues}>
        {(list) =>
          list.length === 0 ? (
            <EmptyState
              icon={MapPin}
              title="No venues yet"
              action={<Button onClick={() => setAdding(true)}>Add venue</Button>}
            >
              Add the place you hold performances, with how many people it lets in. Events are set up at a venue.
            </EmptyState>
          ) : (
            <Card className="py-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Venue name</TableHead>
                    <TableHead>Address</TableHead>
                    <TableHead className="text-right">Capacity</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {list.map((venue) => (
                    <TableRow key={venue.id}>
                      <TableCell className="font-medium">{venue.venueName}</TableCell>
                      <TableCell className="text-muted-foreground">{venue.address}</TableCell>
                      <TableCell className="text-right tabular-nums">{venue.capacity}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Card>
          )
        }
      </Loadable>

      {adding && (
        <AddVenueDialog
          organizerId={organizerId!}
          onClose={() => setAdding(false)}
          onDone={() => {
            setAdding(false)
            venues.reload()
          }}
        />
      )}
    </div>
  )
}

function AddVenueDialog({ organizerId, onClose, onDone }: { organizerId: string; onClose: () => void; onDone: () => void }) {
  const [venueName, setVenueName] = useState('')
  const [address, setAddress] = useState('')
  const [capacity, setCapacity] = useState('')
  const submit = useSubmit()

  async function onSubmit() {
    if (await submit.run(() => api.post('/Venues/Add', { organizerId, venueName, address, capacity: Number(capacity) }))) {
      toast.success('Venue added')
      onDone()
    }
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title="Add venue"
      submitLabel="Save"
      pending={submit.pending}
      error={submit.error}
      onSubmit={onSubmit}
    >
      <TextField label="Venue name" value={venueName} onChange={(e) => setVenueName(e.target.value)} error={submit.fieldError('venuename')} />
      <TextField label="Address" value={address} onChange={(e) => setAddress(e.target.value)} error={submit.fieldError('address')} />
      <TextField
        label="Capacity"
        type="number"
        inputMode="numeric"
        min={1}
        hint="The most people the venue lets in at one performance."
        value={capacity}
        onChange={(e) => setCapacity(e.target.value)}
        error={submit.fieldError('capacity')}
      />
    </FormDialog>
  )
}
