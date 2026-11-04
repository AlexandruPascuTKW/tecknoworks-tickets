import { UserPlus, Users } from 'lucide-react'
import { useState } from 'react'
import { useParams } from 'react-router'
import { toast } from 'sonner'
import { EmptyState, Loadable } from '@/components/async-states'
import { FormDialog } from '@/components/form-dialog'
import { FormField, TextField } from '@/components/form-field'
import { PageHeader } from '@/components/page-header'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { api } from '@/lib/api'
import { teamRoleIn, useAuth, type TeamRole } from '@/lib/auth'
import { useAsync } from '@/lib/use-async'
import { useSubmit } from '@/lib/use-submit'

type Member = { accountId: string; name: string; email: string; teamRole: TeamRole }

function RoleSelect({ control, value, onChange }: { control: object; value: string; onChange: (value: string) => void }) {
  return (
    <Select value={value} onValueChange={onChange}>
      <SelectTrigger {...control} className="w-full">
        <SelectValue placeholder="Choose a role" />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value="Owner">Owner</SelectItem>
        <SelectItem value="Manager">Manager</SelectItem>
      </SelectContent>
    </Select>
  )
}

/**
 * ACC-V2 — See the Team (Owner, Manager). ACC-A4, ACC-A5, ACC-A6 — only an Owner adds, removes or changes
 * the Team role of anyone; a Manager sees the list and no controls (the host refuses them anyway).
 */
export function TeamPage() {
  const { organizerId } = useParams()
  const { account, refresh } = useAuth()
  const isOwner = teamRoleIn(account, organizerId) === 'Owner'
  const organizerName = account?.teams.find((t) => t.organizerId === organizerId)?.organizerName

  const team = useAsync(() => api.get<Member[]>(`/Organizers/Team/${organizerId}`), [organizerId])
  const [adding, setAdding] = useState(false)
  const [changing, setChanging] = useState<Member>()
  const [removingId, setRemovingId] = useState<string>()

  async function remove(member: Member) {
    setRemovingId(member.accountId)

    try {
      await api.post('/Organizers/RemoveTeamMember', { organizerId, accountId: member.accountId })
      toast.success('Team member removed')
      team.reload()
      await refresh()
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'That did not work.')
    } finally {
      setRemovingId(undefined)
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader
        title="Team"
        description={`The people who work on ${organizerName ?? 'this organizer'}'s behalf. Owners run the Team and publish events; Managers set up the shows.`}
        actions={
          isOwner && (
            <Button onClick={() => setAdding(true)}>
              <UserPlus />
              Add team member
            </Button>
          )
        }
      />

      <Loadable state={team}>
        {(members) =>
          members.length === 0 ? (
            <EmptyState icon={Users} title="No one on the Team">An organizer always has an owner, so this should not happen.</EmptyState>
          ) : (
            <Card className="py-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Email address</TableHead>
                    <TableHead>Team role</TableHead>
                    {isOwner && <TableHead className="text-right">Actions</TableHead>}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {members.map((member) => (
                    <TableRow key={member.accountId}>
                      <TableCell className="font-medium">{member.name}</TableCell>
                      <TableCell className="text-muted-foreground">{member.email}</TableCell>
                      <TableCell>
                        <Badge variant={member.teamRole === 'Owner' ? 'default' : 'secondary'}>{member.teamRole}</Badge>
                      </TableCell>
                      {isOwner && (
                        <TableCell className="text-right">
                          <div className="flex justify-end gap-2">
                            <Button variant="outline" size="sm" onClick={() => setChanging(member)}>
                              Change role
                            </Button>
                            <Button variant="outline" size="sm" disabled={removingId === member.accountId} onClick={() => remove(member)}>
                              Remove
                            </Button>
                          </div>
                        </TableCell>
                      )}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Card>
          )
        }
      </Loadable>

      {adding && (
        <AddMemberDialog
          organizerId={organizerId!}
          onClose={() => setAdding(false)}
          onDone={() => {
            setAdding(false)
            team.reload()
          }}
        />
      )}
      {changing && (
        <ChangeRoleDialog
          organizerId={organizerId!}
          member={changing}
          onClose={() => setChanging(undefined)}
          onDone={async () => {
            setChanging(undefined)
            team.reload()
            await refresh()
          }}
        />
      )}
    </div>
  )
}

/** ACC-A4 — Email address and Team role; a Name only when the person has no account yet (ACC-R5). */
function AddMemberDialog({ organizerId, onClose, onDone }: { organizerId: string; onClose: () => void; onDone: () => void }) {
  const [email, setEmail] = useState('')
  const [name, setName] = useState('')
  const [teamRole, setTeamRole] = useState('Manager')
  const submit = useSubmit()

  async function onSubmit() {
    if (await submit.run(() => api.post('/Organizers/AddTeamMember', { organizerId, email, name: name || null, teamRole }))) {
      toast.success('Team member added')
      onDone()
    }
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title="Add team member"
      description="Someone who already has an account joins with it. Someone new is invited to choose their own password."
      submitLabel="Add"
      pending={submit.pending}
      error={submit.error}
      onSubmit={onSubmit}
    >
      <TextField label="Email address" type="email" value={email} onChange={(e) => setEmail(e.target.value)} error={submit.fieldError('email')} />
      <TextField label="Name" hint="Needed only if they have no account yet." value={name} onChange={(e) => setName(e.target.value)} error={submit.fieldError('name')} />
      <FormField label="Team role" error={submit.fieldError('teamrole')}>
        {(control) => <RoleSelect control={control} value={teamRole} onChange={setTeamRole} />}
      </FormField>
    </FormDialog>
  )
}

/** ACC-A6 — Make a Manager an Owner, or an Owner a Manager. */
function ChangeRoleDialog({
  organizerId,
  member,
  onClose,
  onDone,
}: {
  organizerId: string
  member: Member
  onClose: () => void
  onDone: () => void
}) {
  const [teamRole, setTeamRole] = useState<string>(member.teamRole)
  const submit = useSubmit()

  async function onSubmit() {
    if (await submit.run(() => api.post('/Organizers/ChangeTeamRole', { organizerId, accountId: member.accountId, teamRole }))) {
      toast.success('Team role changed')
      onDone()
    }
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={`Change role for ${member.name}`}
      submitLabel="Save"
      pending={submit.pending}
      error={submit.error}
      onSubmit={onSubmit}
    >
      <FormField label="Team role" error={submit.fieldError('teamrole')}>
        {(control) => <RoleSelect control={control} value={teamRole} onChange={setTeamRole} />}
      </FormField>
    </FormDialog>
  )
}
