import { Store } from 'lucide-react'
import { AppSidebar } from '@/components/app-sidebar'
import { ThemeToggle } from '@/components/theme-toggle'
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from '@/components/ui/empty'
import { Separator } from '@/components/ui/separator'
import { SidebarInset, SidebarProvider, SidebarTrigger } from '@/components/ui/sidebar'

export default function App() {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-14 shrink-0 items-center gap-2 border-b px-4 md:px-6">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 data-[orientation=vertical]:h-4" />
          <h1 className="text-lg font-semibold">TecknoworksTickets</h1>
          <div className="ml-auto">
            <ThemeToggle />
          </div>
        </header>
        <main className="flex flex-1 flex-col p-4 md:p-6 lg:p-8">
          <Empty className="flex-1">
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <Store />
              </EmptyMedia>
              <EmptyTitle>Nothing here yet</EmptyTitle>
              <EmptyDescription>
                The shell is ready. Screens are added one spec at a time under src/, using the
                components in src/components/ui and the Tecknoworks tokens in src/index.css.
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        </main>
      </SidebarInset>
    </SidebarProvider>
  )
}
