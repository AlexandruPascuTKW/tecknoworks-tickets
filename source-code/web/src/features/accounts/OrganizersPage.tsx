import { Building2, Plus } from 'lucide-react'
import { useState } from 'react'
import { toast } from 'sonner'
import { EmptyState, Loadable, RowsSkeleton } from '@/components/async-states'
import { FormDialog } from '@/components/form-dialog'
import { TextField } from '@/components/form-field'
import { PageHeader } from '@/components/page-header'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { api } from '@/lib/api'
import { useAsync } from '@/lib/use-async'
import { useSubmit } from '@/lib/use-submit'

type Member = { accountId: string; name: string; email: string; teamRole: string }
type Organizer = { id: string; organizerName: string; teamMembers: Member[] }

/** ACC-V1 — See all Organizers and who is on their Team; ACC-A3 — Onboard an Organizer. Platform admin only. */
export function OrganizersPage() {
  const organizers = useAsync(() => api.get<Organizer[]>('/Organizers/All'), [])
  const [onboarding, setOnboarding] = useState(false)

  return (
    <div className="grid gap-6">
      <PageHeader
        title="Organizers"
        description="Every organizer on the platform and the people on its Team."
        actions={
          <Button onClick={() => setOnboarding(true)}>
            <Plus />
            Onboard organizer
          </Button>
        }
      />

      <Loadable state={organizers} skeleton={<RowsSkeleton rows={2} height="h-40" />}>
        {(list) =>
          list.length === 0 ? (
            <EmptyState icon={Building2} title="No organizers yet">
              Onboard the first organizer to give it an owner and a Team.
            </EmptyState>
          ) : (
            <div className="grid gap-4">
              {list.map((organizer) => (
                <Card key={organizer.id}>
                  <CardHeader>
                    <CardTitle className="text-lg">{organizer.organizerName}</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Name</TableHead>
                          <TableHead>Email address</TableHead>
                          <TableHead>Team role</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {organizer.teamMembers.map((member) => (
                          <TableRow key={member.accountId}>
                            <TableCell className="font-medium">{member.name}</TableCell>
                            <TableCell className="text-muted-foreground">{member.email}</TableCell>
                            <TableCell>
                              <Badge variant={member.teamRole === 'Owner' ? 'default' : 'secondary'}>{member.teamRole}</Badge>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </CardContent>
                </Card>
              ))}
            </div>
          )
        }
      </Loadable>

      {onboarding && (
        <OnboardDialog
          onClose={() => setOnboarding(false)}
          onDone={() => {
            setOnboarding(false)
            organizers.reload()
          }}
        />
      )}
    </div>
  )
}

/** ACC-A3 — the Name and Email address are the first Owner's. */
function OnboardDialog({ onClose, onDone }: { onClose: () => void; onDone: () => void }) {
  const [organizerName, setOrganizerName] = useState('')
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const submit = useSubmit()

  async function onSubmit() {
    if (await submit.run(() => api.post('/Organizers/Onboard', { organizerName, name, email }))) {
      toast.success('Organizer onboarded')
      onDone()
    }
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title="Onboard organizer"
      description="Set up a newly contracted organizer and make the person named its first owner. They are invited to choose their own password."
      submitLabel="Onboard"
      pending={submit.pending}
      error={submit.error}
      onSubmit={onSubmit}
    >
      <TextField label="Organizer name" value={organizerName} onChange={(e) => setOrganizerName(e.target.value)} error={submit.fieldError('organizername')} />
      <TextField label="Name" hint="The first owner's name. Needed unless they already have an account." value={name} onChange={(e) => setName(e.target.value)} error={submit.fieldError('name')} />
      <TextField label="Email address" type="email" value={email} onChange={(e) => setEmail(e.target.value)} error={submit.fieldError('email')} />
    </FormDialog>
  )
}
