import { AlertCircle, Loader2 } from 'lucide-react'
import { useState, type FormEvent, type ReactNode } from 'react'
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router'
import { toast } from 'sonner'
import { FormError } from '@/components/form-dialog'
import { TextField } from '@/components/form-field'
import { PageHeader } from '@/components/page-header'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter } from '@/components/ui/card'
import { api } from '@/lib/api'
import { useAuth } from '@/lib/auth'
import { useSubmit } from '@/lib/use-submit'

function AuthCard({ title, description, children }: { title: string; description: string; children: ReactNode }) {
  return (
    <div className="mx-auto grid w-full max-w-md gap-6 pt-4 md:pt-10">
      <PageHeader title={title} description={description} />
      <Card>{children}</Card>
    </div>
  )
}

/** ACC-A2 — Sign in. */
export function SignInPage() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const submit = useSubmit()

  async function onSubmit(event: FormEvent) {
    event.preventDefault()

    if (await submit.run(() => signIn(email, password))) {
      navigate(from ?? '/whats-on', { replace: true })
    }
  }

  return (
    <AuthCard
      title="Sign in"
      description={from ? 'Sign in to buy tickets. New here? Register first.' : 'Use the email address and password of your account.'}
    >
      <form onSubmit={onSubmit} noValidate>
        <CardContent className="grid gap-4">
          <FormError error={submit.error} />
          <TextField
            label="Email address"
            type="email"
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            error={submit.fieldError('email')}
          />
          <TextField
            label="Password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            error={submit.fieldError('password')}
          />
        </CardContent>
        <CardFooter className="mt-6 flex-col items-stretch gap-3">
          <Button type="submit" disabled={submit.pending}>
            {submit.pending && <Loader2 className="animate-spin" />}
            Sign in
          </Button>
          <p className="text-center text-sm text-muted-foreground">
            No account yet?{' '}
            <Link to="/register" state={location.state} className="font-medium text-primary underline-offset-4 hover:underline">
              Register
            </Link>
          </p>
        </CardFooter>
      </form>
    </AuthCard>
  )
}

/** ACC-A1 — Register. The new Account is signed in straight away. */
export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const submit = useSubmit()

  async function onSubmit(event: FormEvent) {
    event.preventDefault()

    if (await submit.run(() => register(name, email, password))) {
      toast.success('Account created')
      navigate(from ?? '/whats-on', { replace: true })
    }
  }

  return (
    <AuthCard title="Register" description="An account is where your tickets live. Browsing what's on needs none.">
      <form onSubmit={onSubmit} noValidate>
        <CardContent className="grid gap-4">
          <FormError error={submit.error} />
          <TextField
            label="Name"
            autoComplete="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            error={submit.fieldError('name')}
          />
          <TextField
            label="Email address"
            type="email"
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            error={submit.fieldError('email')}
          />
          <TextField
            label="Password"
            type="password"
            autoComplete="new-password"
            hint="10 to 128 characters."
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            error={submit.fieldError('password')}
          />
        </CardContent>
        <CardFooter className="mt-6 flex-col items-stretch gap-3">
          <Button type="submit" disabled={submit.pending}>
            {submit.pending && <Loader2 className="animate-spin" />}
            Create account
          </Button>
          <p className="text-center text-sm text-muted-foreground">
            Already registered?{' '}
            <Link to="/sign-in" state={location.state} className="font-medium text-primary underline-offset-4 hover:underline">
              Sign in
            </Link>
          </p>
        </CardFooter>
      </form>
    </AuthCard>
  )
}

/** ACC-R3 — someone whose Account was made for them chooses their own Password from the emailed link. */
export function ChoosePasswordPage() {
  const [params] = useSearchParams()
  const token = params.get('token') ?? ''
  const navigate = useNavigate()
  const [password, setPassword] = useState('')
  const submit = useSubmit()

  async function onSubmit(event: FormEvent) {
    event.preventDefault()

    if (await submit.run(() => api.post('/Accounts/AcceptInvitation', { token, password }))) {
      toast.success('Password set')
      navigate('/sign-in', { replace: true })
    }
  }

  return (
    <AuthCard title="Choose your password" description="You were invited to TecknoworksTickets. Choose a password to finish setting up your account.">
      <form onSubmit={onSubmit} noValidate>
        <CardContent className="grid gap-4">
          {!token && (
            <Alert variant="destructive">
              <AlertCircle />
              <AlertDescription>This invitation link is incomplete. Open the link from your email again.</AlertDescription>
            </Alert>
          )}
          <FormError error={submit.error} />
          <TextField
            label="Password"
            type="password"
            autoComplete="new-password"
            hint="10 to 128 characters."
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            error={submit.fieldError('password')}
          />
        </CardContent>
        <CardFooter className="mt-6">
          <Button type="submit" className="w-full" disabled={submit.pending || !token}>
            {submit.pending && <Loader2 className="animate-spin" />}
            Set password
          </Button>
        </CardFooter>
      </form>
    </AuthCard>
  )
}
