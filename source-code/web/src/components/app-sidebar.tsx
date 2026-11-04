import { House } from 'lucide-react'
import logo from '@/assets/tecknoworks-logo-white.svg'
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from '@/components/ui/sidebar'

// Navigation grows with the product: each spec's views add an entry here.
const navigation = [{ title: 'Home', icon: House, href: '/' }]

export function AppSidebar() {
  return (
    <Sidebar>
      <SidebarHeader className="px-4 py-5">
        <img src={logo} alt="Tecknoworks" className="h-7 w-auto self-start" />
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {navigation.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild isActive={item.href === '/'}>
                    <a href={item.href}>
                      <item.icon />
                      <span>{item.title}</span>
                    </a>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  )
}
