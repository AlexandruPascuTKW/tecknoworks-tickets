# source-code

The TecknoworksTickets implementation. A .NET modular monolith and a React frontend, built from the specs in
`../external-context/specs/`: Accounts, Events, Sales and Admission (increment 1).

```
source-code/
  TecknoworksStore.sln
  Common/            shared kernel: Common.Domain · .Application · .Infrastructure · .Web
  ProjectStartup/    the single ASP.NET host — the only project allowed to see several contexts
  tests/
    ArchitectureTests/   fails the build if a project reference breaks the context boundaries
  web/               Vite + React + TypeScript + Tailwind v4 + shadcn/ui, Tecknoworks-branded
```

## Backend — modular monolith of bounded contexts

Structure follows the `dotnet-ddd-architecture` skill: each bounded context is its own folder with
four projects (`<Ctx>.Domain`, `.Application`, `.Infrastructure`, `.Web`) that compile into the one
host. There are four contexts — `Accounts`, `Events`, `Sales`, `Admission` — one per spec; how they fit
together is in `../external-context/docs/architecture.md`. The solution comes with .NET 10, MediatR, EF Core on
**SQLite** (a single local file, so no database server is needed), Swagger in development, and
Problem Details error handling.

```
cd source-code
dotnet build
dotnet test                         # architecture tests
dotnet run --project ProjectStartup # http://localhost:5151, Swagger at /swagger
```

Sign-in tokens are issued by Accounts. In Development the host seeds two staff accounts from
`ProjectStartup/appsettings.Development.json` (a Platform admin and the Finance manager), and the
payment provider is simulated. Invitation links, which stand in for emails, are written to the host's log.

## Frontend — `web/`

Vite + React + TypeScript, Tailwind v4 and shadcn/ui (radix, `radix-nova` preset). It was set up
with the `shadcn-craft` skill and themed with the `tecknoworks-brand` skill: Deep Purple, Plum,
Magenta and Coral as tokens in `src/index.css` (light and dark), Nunito as the typeface, the
Tecknoworks logo in the purple sidebar. `--primary` is Magenta darkened slightly so white text on
it passes contrast; the exact brand Magenta is kept as `--brand-magenta`.

```
cd source-code/web
npm install
npm run dev     # http://localhost:5173, /api is proxied to the .NET host on :5151
npm run build
```

Components live in `src/components/ui` (add more with `npx shadcn@latest add <name>`). The app
shell (`src/App.tsx`, `src/components/app-sidebar.tsx`) has one empty "Home" screen.

## Working rule

Read the domain spec in [`../external-context/specs/`](../external-context/specs/) before writing
code, and update the spec first if a requirement changes. Reference spec IDs in code comments. Don't
implement anything a spec doesn't say. Exercise context:
[`../external-context/context.md`](../external-context/context.md).
