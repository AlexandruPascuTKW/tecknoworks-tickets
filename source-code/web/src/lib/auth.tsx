import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { api, getToken, setToken, setUnauthorizedHandler } from '@/lib/api'

export type TeamRole = 'Owner' | 'Manager'

export type Team = { organizerId: string; organizerName: string; teamRole: TeamRole }

/** ACC-V* — the signed-in person, their platform role and the Teams they are on. */
export type Account = {
  id: string
  name: string
  email: string
  platformRole: 'PlatformAdmin' | 'FinanceManager' | null
  teams: Team[]
}

type SignInResponse = { token: string }

type AuthState = {
  /** null once we know nobody is signed in. */
  account: Account | null
  ready: boolean
  signIn: (email: string, password: string) => Promise<void>
  register: (name: string, email: string, password: string) => Promise<void>
  signOut: () => void
  refresh: () => Promise<void>
}

const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [account, setAccount] = useState<Account | null>(null)
  const [ready, setReady] = useState(false)

  const signOut = useCallback(() => {
    setToken(null)
    setAccount(null)
  }, [])

  const refresh = useCallback(async () => {
    if (!getToken()) {
      setAccount(null)
      return
    }

    try {
      setAccount(await api.get<Account>('/Accounts/Me'))
    } catch {
      signOut()
    }
  }, [signOut])

  useEffect(() => {
    setUnauthorizedHandler(signOut)
    refresh().finally(() => setReady(true))

    return () => setUnauthorizedHandler(undefined)
  }, [refresh, signOut])

  const value = useMemo<AuthState>(
    () => ({
      account,
      ready,
      refresh,
      signOut,
      // ACC-A2
      signIn: async (email, password) => {
        const response = await api.post<SignInResponse>('/Accounts/SignIn', { email, password })
        setToken(response.token)
        await refresh()
      },
      // ACC-A1 — registering signs the person in
      register: async (name, email, password) => {
        const response = await api.post<SignInResponse>('/Accounts/Register', { name, email, password })
        setToken(response.token)
        await refresh()
      },
    }),
    [account, ready, refresh, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthState {
  const value = useContext(AuthContext)
  if (!value) throw new Error('useAuth must be used inside AuthProvider')
  return value
}

/** The person's Team role for one Organizer, or null if they are not on its Team. */
export function teamRoleIn(account: Account | null, organizerId: string | undefined): TeamRole | null {
  return account?.teams.find((t) => t.organizerId === organizerId)?.teamRole ?? null
}
