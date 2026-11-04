import { Building2, CalendarDays, LogIn, LogOut, MapPin, Receipt, Ticket, UserPlus, Users, type LucideIcon } from 'lucide-react'
import { Link, NavLink, useLocation, useNavigate } from 'react-router'
import logo from '@/assets/tecknoworks-logo-white.svg'
import { useAuth } from '@/lib/auth'
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from '@/components/ui/sidebar'

function NavItem({ to, icon: Icon, label }: { to: string; icon: LucideIcon; label: string }) {
  const { pathname } = useLocation()
  const { setOpenMobile } = useSidebar()
  const active = pathname === to || pathname.startsWith(`${to}/`)

  return (
    <SidebarMenuItem>
      <SidebarMenuButton asChild isActive={active} onClick={() => setOpenMobile(false)}>
        <NavLink to={to}>
          <Icon />
          <span>{label}</span>
        </NavLink>
      </SidebarMenuButton>
    </SidebarMenuItem>
  )
}

function NavGroup({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <SidebarGroup>
      <SidebarGroupLabel>{label}</SidebarGroupLabel>
      <SidebarGroupContent>
        <SidebarMenu>{children}</SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  )
}

/** Navigation follows who the person is: anyone browses; an Attendee has tickets; a Team works on its Organizer. */
export function AppSidebar() {
  const { account, signOut } = useAuth()
  const navigate = useNavigate()
  const { setOpenMobile } = useSidebar()

  return (
    <Sidebar className="print:hidden">
      <SidebarHeader className="px-4 py-5">
        <Link to="/whats-on" aria-label="TecknoworksTickets — what's on">
          <img src={logo} alt="Tecknoworks" className="h-7 w-auto self-start" />
        </Link>
      </SidebarHeader>

      <SidebarContent>
        <NavGroup label="Tickets">
          <NavItem to="/whats-on" icon={CalendarDays} label="What's on" />
          {account && <NavItem to="/my-tickets" icon={Ticket} label="My tickets" />}
        </NavGroup>

        {account?.teams.map((team) => (
          <NavGroup key={team.organizerId} label={team.organizerName}>
            <NavItem to={`/team/${team.organizerId}/events`} icon={CalendarDays} label="Events" />
            <NavItem to={`/team/${team.organizerId}/venues`} icon={MapPin} label="Venues" />
            <NavItem to={`/team/${team.organizerId}/members`} icon={Users} label="Team" />
          </NavGroup>
        ))}

        {(account?.platformRole === 'PlatformAdmin' || account?.platformRole === 'FinanceManager') && (
          <NavGroup label="Platform">
            {account.platformRole === 'PlatformAdmin' && <NavItem to="/organizers" icon={Building2} label="Organizers" />}
            {account.platformRole === 'FinanceManager' && <NavItem to="/finance/sales" icon={Receipt} label="Sales" />}
          </NavGroup>
        )}
      </SidebarContent>

      <SidebarFooter className="gap-1 border-t border-sidebar-border p-3">
        {account ? (
          <>
            <div className="px-2 pb-1">
              <p className="truncate text-sm font-medium">{account.name}</p>
              <p className="truncate text-xs text-sidebar-foreground/70">{account.email}</p>
            </div>
            <SidebarMenu>
              <SidebarMenuItem>
                <SidebarMenuButton
                  onClick={() => {
                    signOut()
                    setOpenMobile(false)
                    navigate('/whats-on')
                  }}
                >
                  <LogOut />
                  <span>Sign out</span>
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </>
        ) : (
          <SidebarMenu>
            <NavItem to="/sign-in" icon={LogIn} label="Sign in" />
            <NavItem to="/register" icon={UserPlus} label="Register" />
          </SidebarMenu>
        )}
      </SidebarFooter>
    </Sidebar>
  )
}
