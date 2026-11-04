# source-code

The TecknoworksTickets implementation. Bootstrapped, not built: a .NET backend foundation and a React
frontend foundation, with no domain features. The workshop builds those one spec at a time.

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
host. There are **no contexts yet** — each is created from its spec (the skill's "Adding a new
bounded context" checklist is the recipe). The solution comes with .NET 10, MediatR, EF Core on
**SQLite** (a single local file, so no database server is needed), Swagger in development, and
Problem Details error handling.

```
cd source-code
dotnet build
dotnet test                         # architecture tests
dotnet run --project ProjectStartup # http://localhost:5151, Swagger at /swagger
```

Authentication is not wired up yet: when the Access context is built, add
`AddTokenAuthentication` and `UseAuthentication`/`UseAuthorization` in the host (see the comments in
`ProjectStartup/Program.cs`).

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
