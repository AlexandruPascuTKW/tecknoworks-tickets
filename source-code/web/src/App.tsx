import { Lock } from 'lucide-react'
import type { ReactNode } from 'react'
import { BrowserRouter, Link, Navigate, Outlet, Route, Routes, useLocation, useParams } from 'react-router'
import { AppSidebar } from '@/components/app-sidebar'
import { EmptyState, RowsSkeleton } from '@/components/async-states'
import { ThemeToggle } from '@/components/theme-toggle'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { SidebarInset, SidebarProvider, SidebarTrigger } from '@/components/ui/sidebar'
import { ChoosePasswordPage, RegisterPage, SignInPage } from '@/features/accounts/auth-pages'
import { OrganizersPage } from '@/features/accounts/OrganizersPage'
import { TeamPage } from '@/features/accounts/TeamPage'
import { DoorListPage } from '@/features/admission/DoorListPage'
import { MyTicketsPage } from '@/features/admission/MyTicketsPage'
import { EventEditorPage } from '@/features/events/EventEditorPage'
import { EventPage } from '@/features/events/EventPage'
import { EventsPage } from '@/features/events/EventsPage'
import { VenuesPage } from '@/features/events/VenuesPage'
import { WhatsOnPage } from '@/features/events/WhatsOnPage'
import { FinanceSalesPage } from '@/features/sales/FinanceSalesPage'
import { OrderPage } from '@/features/sales/OrderPage'
import { SimulatedPaymentPage } from '@/features/sales/SimulatedPaymentPage'
import { AuthProvider, teamRoleIn, useAuth, type Account } from '@/lib/auth'

function NoAccess({ title = "You don't have access to this page", children }: { title?: string; children?: ReactNode }) {
  return (
    <EmptyState
      icon={Lock}
      title={title}
      action={
        <Button asChild>
          <Link to="/whats-on">Back to what's on</Link>
        </Button>
      }
    >
      {children ?? 'It may belong to someone else, or it may not exist.'}
    </EmptyState>
  )
}

/** Signed-in people only — and, where a platform role is named, only that role. The host checks again. */
function RequireAuth({ role, children }: { role?: NonNullable<Account['platformRole']>; children: ReactNode }) {
  const { account, ready } = useAuth()
  const location = useLocation()

  if (!ready) return <RowsSkeleton rows={2} height="h-24" />
  if (!account) return <Navigate to="/sign-in" state={{ from: location.pathname }} replace />
  if (role && account.platformRole !== role) return <NoAccess />

  return <>{children}</>
}

/** The pages of one Organizer are for its own Team (ACC-R1, EVT-R1). */
function RequireTeam() {
  const { account, ready } = useAuth()
  const { organizerId } = useParams()
  const location = useLocation()

  if (!ready) return <RowsSkeleton rows={2} height="h-24" />
  if (!account) return <Navigate to="/sign-in" state={{ from: location.pathname }} replace />
  if (!teamRoleIn(account, organizerId)) return <NoAccess />

  return <Outlet />
}

function Shell() {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-14 shrink-0 items-center gap-2 border-b px-4 md:px-6 print:hidden">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 data-[orientation=vertical]:h-4" />
          <p className="text-lg font-semibold">TecknoworksTickets</p>
          <div className="ml-auto">
            <ThemeToggle />
          </div>
        </header>

        {/* SidebarInset is itself the page's <main>. */}
        <div className="flex flex-1 flex-col p-4 md:p-6 lg:p-8 print:p-0">
          <div className="mx-auto w-full max-w-6xl">
            <Routes>
              <Route path="/" element={<Navigate to="/whats-on" replace />} />
              <Route path="/whats-on" element={<WhatsOnPage />} />
              <Route path="/events/:eventId" element={<EventPage />} />

              <Route path="/sign-in" element={<SignInPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/choose-password" element={<ChoosePasswordPage />} />

              <Route path="/my-tickets" element={<RequireAuth><MyTicketsPage /></RequireAuth>} />
              <Route path="/orders/:orderId" element={<RequireAuth><OrderPage /></RequireAuth>} />
              <Route path="/simulated-payment/:reference" element={<SimulatedPaymentPage />} />

              <Route path="/organizers" element={<RequireAuth role="PlatformAdmin"><OrganizersPage /></RequireAuth>} />
              <Route path="/finance/sales/:performanceId?" element={<RequireAuth role="FinanceManager"><FinanceSalesPage /></RequireAuth>} />

              <Route path="/team/:organizerId" element={<RequireTeam />}>
                <Route index element={<Navigate to="events" replace />} />
                <Route path="events" element={<EventsPage />} />
                <Route path="events/:eventId" element={<EventEditorPage />} />
                <Route path="venues" element={<VenuesPage />} />
                <Route path="members" element={<TeamPage />} />
                <Route path="door-list/:performanceId" element={<DoorListPage />} />
              </Route>

              <Route path="*" element={<NoAccess title="There is nothing here">That page doesn't exist.</NoAccess>} />
            </Routes>
          </div>
        </div>
      </SidebarInset>
    </SidebarProvider>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Shell />
      </AuthProvider>
    </BrowserRouter>
  )
}
