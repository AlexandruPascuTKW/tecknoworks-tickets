# Technical architecture — TecknoworksTickets, increment 1

**Status:** Proposed · **Date:** 2026-11-04
**Built from:** the specs in `specs/` — Accounts (ACC-001), Events (EVT-001), Sales (SAL-001),
Admission (ADM-001) — and the split in [`bounded-contexts.md`](bounded-contexts.md)
**Foundation:** `source-code/` as bootstrapped — one `ProjectStartup` host, EF Core on one SQLite
file, the `Common` shared kernel, in-process domain events. Nothing here adds infrastructure: no
message broker, no mail server, no blob store, no second database.

This document says where each part of the specs lives in code: one bounded context per spec, the
projects and files each context needs, the events that cross between contexts, and where each rule
is enforced. It describes structure, not behaviour. What the product does stays in the specs, and
spec IDs are cited so the code can cite them too.

## Contents

- [The four contexts](#the-four-contexts)
- [Decisions that apply to every context](#decisions-that-apply-to-every-context)
- [Changes to the foundation](#changes-to-the-foundation)
- [Accounts](#accounts)
- [Events](#events)
- [Sales](#sales)
- [Admission](#admission)
- [Seams — what crosses between contexts](#seams--what-crosses-between-contexts)
- [Two flows end to end](#two-flows-end-to-end)
- [Frontend](#frontend)
- [Risks and the tests that guard them](#risks-and-the-tests-that-guard-them)
- [Decisions to record as ADRs](#decisions-to-record-as-adrs)
- [Build order](#build-order)

## The four contexts

One spec, one bounded context, one folder under `source-code/` with four projects
(`<Ctx>.Domain`, `.Application`, `.Infrastructure`, `.Web`). The split and the reasons for it are in
`bounded-contexts.md`. The table below adds the aggregates each context owns.

| Spec | Context (folder) | Owns | Aggregates | Local copies it keeps of other contexts' data |
|---|---|---|---|---|
| ACC-001 | `Accounts` | Accounts, sign-in, Organizers and their Teams, platform roles | `Account`, `Organizer` (with `TeamMember`s) | — |
| EVT-001 | `Events` | Venues, Events, Performances, Ticket types, publishing | `Venue`, `Event` (with `Performance`s and `TicketType`s), `EventImage` | Team members (from Accounts); which Performances and Ticket types have sales (from Sales) |
| SAL-001 | `Sales` | Availability, Holds, Orders, the Booking fee, Payments | `Performance` (Sales' own view of what is on sale), `Order` (with `OrderLine`s and `Payment`s) | Published Events' Performances and Ticket types (from Events) |
| ADM-001 | `Admission` | Tickets, Ticket codes, the Door list | `Ticket` | Team members (from Accounts); Performance details (from Events) |

A type with the same name in two contexts (`Performance`, `TeamMember`) is two different models of
the same thing, each shaped by its own context's needs. That is deliberate. Never merge them into
`Common`.

## Decisions that apply to every context

### Identity and authorization

- **Accounts issues the token.** Sign-in (ACC-A2) returns a JWT signed with
  `ApplicationSettings.Secret`. The host validates it with the existing `AddTokenAuthentication`.
  The token carries `sub` (the Account id), `name`, `email`, and a `role` claim for the platform
  roles (`PlatformAdmin`, `FinanceManager`). These change rarely, and no action in the specs changes
  them.
- **Team memberships are *not* in the token.** Removing someone from a Team (ACC-A5) or changing
  their Team role (ACC-A6) has to take effect at once. A removed Manager must not keep working for
  the Organizer, and a demoted Owner must not keep publishing (EVT-A10). A token issued earlier would
  keep the old membership until it expired. So Accounts raises `TeamMemberAdded`, `TeamMemberRemoved`
  and `TeamRoleChanged` events, and each context that checks Team roles (Events, Admission) keeps its
  own `TeamMember` table up to date from them. A context checks permissions against its own table,
  never against Accounts. **This replaces the line in `bounded-contexts.md` that put memberships in
  the login session's claims.**
- **The check lives in the Application handler.** It runs after the aggregate is loaded, because
  that is when its `OrganizerId` is known. Each context has one small helper,
  `Application/Common/TeamAccess.cs`. It takes the current user, an Organizer id and the Team role
  required, and throws.
- **Out of scope → 404. In scope but not allowed → 403.** Someone on another Organizer's Team gets
  `NotFoundException` for a Draft Event, a Venue, a Door list or a Team. A 404 doesn't reveal that
  the thing exists (EVT-R1, ACC-R1, ADM-R1). A Manager trying to publish (EVT-A10) or to run the
  Team (ACC-A4/A5/A6) gets a new `ForbiddenException`. Either way the refusal comes from the handler,
  so it holds however the request arrives, not only when a button is hidden.
- **Controllers** carry `[Authorize]`. The actions a Visitor may use are marked `[AllowAnonymous]`:
  ACC-A1, ACC-A2, accepting an invitation, EVT-V4, EVT-V5, the event image, SAL-V2 and the payment
  provider's callback. A platform-only action also gets `[Authorize(Roles = …)]`: ACC-A3 and ACC-V1
  for `PlatformAdmin`, SAL-V3 for `FinanceManager`.

### Failure

As the skill's rule says: an invariant the domain refuses throws a `BaseDomainException` subclass,
which becomes a 400 ProblemDetails carrying the message the UI shows. Examples: "You can buy up to
8 tickets in one order", "An organizer must keep at least one owner". A missing or out-of-scope
entity throws `NotFoundException`. A request with the wrong shape fails its `AbstractValidator`.
Handlers that return nothing return `Result`. None of the specs' refusals is a `Result.Failure`
with a code. They are all domain exceptions with a readable message, so every context refuses in
the same way.

### Time, money and identifiers on SQLite

- **Clock:** handlers take the built-in `TimeProvider`, registered once in the host. Domain
  methods that depend on time take `now` as a parameter. Nothing in Domain reads the clock.
- **Venue-local times (EVT-R7):** Date, Doors time and Start time are `DateOnly` / `TimeOnly`, in
  local time at the Venue. All pilots are in one country, so "still to come" compares against *now*
  in one time zone, set as `Events:TimeZone` (and the same setting in Sales).
- **Instants:** Placed at and Hold ends at are UTC `DateTime`, not `DateTimeOffset`. EF Core on
  SQLite can't compare or order `DateTimeOffset` in SQL, and the hold queries do both.
- **Money:** the Domain uses `decimal` inside value objects (`Price` in Events, `Money` in Sales):
  euros, two decimals. EF stores amounts as **integer cents**, through a value converter in each
  configuration. SQLite stores `decimal` as text, which can't be summed or compared in SQL, and
  the sales totals (SAL-V3) need both.
- **Other contexts' ids** are plain `Guid` properties: `OrganizerId`, `AccountId`, `PerformanceId`,
  `TicketTypeId`. There is no foreign key across contexts and no navigation property. Each context
  still gets its own `Id` from `Entity`, so a local copy keeps the original id in a separate
  property.

### Unit of work

`DataRepository.Save` calls `SaveChangesAsync`. Repositories within one context share the
request-scoped `DbContext`. So when a handler loads one aggregate tracked, changes it, and then saves
a second aggregate, both are written in **one** SaveChanges, and therefore in one SQLite
transaction. Sales relies on this to place an Order and reserve its tickets atomically (see Sales).
In other contexts, a handler that writes two aggregates orders the saves so a retry is safe
(onboarding, ACC-A3).

### Namespaces

The `Common` projects use the global namespace. Each context project uses file-scoped namespaces
rooted at its project name: `Sales.Domain.Models.Orders`, `Events.Application.Events.Commands.Publish`.
Without them, the same names would clash in the host: `DomainConfiguration`,
`InfrastructureConfiguration`, `Performance`, `TeamMember`. Swagger also gets
`CustomSchemaIds(t => t.FullName)`, so two contexts' `…Response` types with the same name don't
collide.

## Changes to the foundation

Every change to `Common` here is plumbing that any context would use. None is a business type,
except the cross-context event contracts, which the skill puts in `Common.Domain/Events/` by
design.

| File | Change |
|---|---|
| `Common/Common.Application/Contracts/ICurrentUser.cs` | MOD — `UserId` becomes nullable (Visitors), and it gains `Name`, `Email` and `IsInRole(string)` |
| `Common/Common.Web/Services/CurrentUserService.cs` | MOD — reads those claims; no longer throws for an anonymous request |
| `Common/Common.Application/Exceptions/ForbiddenException.cs` | NEW |
| `Common/Common.Web/ExceptionHandlers/ForbiddenExceptionHandler.cs` | NEW — 403 ProblemDetails |
| `Common/Common.Web/WebConfiguration.cs` | MOD — registers `ForbiddenExceptionHandler` |
| `Common/Common.Infrastructure/DBContext/BaseDBContext.cs` | MOD — writes raised events to the context's `__Outbox_<DbContext>` table in the same save, instead of dispatching after it ([ADR-005](adr/005-event-delivery-outbox.md)) |
| `Common/Common.Infrastructure/Events/OutboxDispatcher.cs` | NEW — one `BackgroundService` per context, registered by `AddDBStorage`; delivers outbox rows through `IEventDispatcher` ([ADR-005](adr/005-event-delivery-outbox.md)) |
| `Common/Common.Infrastructure/Persistence/DbInitializer.cs` | MOD — sets SQLite's WAL journal mode, so reads don't wait on the writer ([ADR-002](adr/002-no-overselling-on-sqlite.md)) |
| `Common/Common.Domain/Events/Organizers/*.cs` | NEW — Team membership events (see [Seams](#seams--what-crosses-between-contexts)) |
| `Common/Common.Domain/Events/Events/*.cs` | NEW — published-Event snapshot events |
| `Common/Common.Domain/Events/Orders/*.cs` | NEW — Order events |
| `ProjectStartup/ProjectStartup.csproj` | MOD — references the four `<Ctx>.Infrastructure` and four `<Ctx>.Web` projects |
| `ProjectStartup/Program.cs` | MOD — the four chained `Add<Ctx>…` blocks; `AddTokenAuthentication`; `TimeProvider.System`; Swagger `CustomSchemaIds` and the bearer scheme |
| `ProjectStartup/ApplicationBuilderExtensions.cs` | MOD — `UseAuthentication()` and `UseAuthorization()` between `UseRouting()` and `UseEndpoints()` |
| `ProjectStartup/appsettings.json` | MOD — a development `Secret`; the `Accounts`, `Events` and `Sales` settings sections |
| `TecknoworksStore.sln` | MOD — one solution folder per context |
| `tests/ArchitectureTests/EventContractTests.cs` | NEW — see [Risks](#risks-and-the-tests-that-guard-them) |

`ProjectReferenceTests` finds contexts from the folder layout, so the four new contexts are checked
from the moment their folders exist.

---

## Accounts

**Ownership.** Accounts owns who people are and which roles they hold. Onboarding (ACC-A3) and
adding a Team member (ACC-A4) both find or create an Account *and* change an Organizer in one
action. That is why logins and Organizers sit in one context (`bounded-contexts.md`).

### Files

```
Accounts/
  Accounts.Domain/
    Models/
      Accounts/
        Account.cs                     NEW  root: Name, Email, PasswordHash?, PlatformRole?, Invitation?
        EmailAddress.cs                NEW  value object — ACC-F2 format; stored lower-cased
        Invitation.cs                  NEW  value object — token hash + expiry (ACC-R3)
        PlatformRole.cs                NEW  Enumeration: PlatformAdmin, FinanceManager
        ModelConstants.cs              NEW  AccountModelConstants (name/email lengths)
      Organizers/
        Organizer.cs                   NEW  root: OrganizerName, TeamMembers — ACC-R4, R5, R6
        TeamMember.cs                  NEW  child: AccountId, TeamRole
        TeamRole.cs                    NEW  Enumeration: Owner, Manager (ACC-F5)
        ModelConstants.cs              NEW  OrganizerModelConstants
    Factories/
      IAccountFactory.cs · AccountFactory.cs          NEW  registered or invited Account
      IOrganizerFactory.cs · OrganizerFactory.cs      NEW  Organizer with its first Owner (ACC-R4)
    Repositories/
      IAccountDomainRepository.cs      NEW  Find, FindByEmail
      IOrganizerDomainRepository.cs    NEW  Find
    Exceptions/
      InvalidAccountException.cs       NEW
      InvalidOrganizerException.cs     NEW
    DomainConfiguration.cs             NEW  AddAccountsDomain()
  Accounts.Application/
    Accounts/
      Commands/
        Register/          RegisterCommand.cs · RegisterCommandValidator.cs · RegisterResponse.cs        ACC-A1 (signs in)
        SignIn/            SignInCommand.cs · SignInCommandValidator.cs · SignInResponse.cs              ACC-A2
        AcceptInvitation/  AcceptInvitationCommand.cs · AcceptInvitationCommandValidator.cs              ACC-R3
      Queries/
        Me/                CurrentAccountQuery.cs · CurrentAccountResponse.cs     own Account, platform role, Teams — for the UI
      IAccountQueryRepository.cs
    Organizers/
      Commands/
        Onboard/           OnboardOrganizerCommand.cs · OnboardOrganizerCommandValidator.cs · OnboardOrganizerResponse.cs   ACC-A3
        AddTeamMember/     AddTeamMemberCommand.cs · AddTeamMemberCommandValidator.cs                    ACC-A4
        RemoveTeamMember/  RemoveTeamMemberCommand.cs                                                    ACC-A5
        ChangeTeamRole/    ChangeTeamRoleCommand.cs · ChangeTeamRoleCommandValidator.cs                  ACC-A6
      Queries/
        GetAll/            GetAllOrganizersQuery.cs · OrganizerListItem.cs                               ACC-V1
        Team/              OrganizerTeamQuery.cs · TeamMemberResponse.cs                                 ACC-V2
      IOrganizerQueryRepository.cs
    Common/
      TeamAccess.cs                    NEW  is the caller an Owner/Manager of this Organizer?
    Services/
      IPasswordHasher.cs               NEW
      ITokenIssuer.cs                  NEW  JWT for a signed-in Account
      IInvitationSender.cs             NEW  delivers the choose-your-password link
    Settings/
      AccountsSettings.cs              NEW  token lifetime, invitation lifetime, seeded staff accounts
    AccountsApplicationConfiguration.cs
  Accounts.Infrastructure/
    Persistence/
      AccountsDbContext.cs             NEW
      AccountsDbInitializer.cs         NEW  migrates; seeds the Platform admin and Finance manager from settings
    Configurations/
      AccountConfiguration.cs          NEW  unique index on Email (ACC-R2); Invitation as owned type
      OrganizerConfiguration.cs        NEW
      TeamMemberConfiguration.cs       NEW  unique (OrganizerId, AccountId) — ACC-R5
    Repositories/
      AccountRepository.cs             NEW
      OrganizerRepository.cs           NEW
    Services/
      Pbkdf2PasswordHasher.cs          NEW  BCL Rfc2898DeriveBytes — no new package
      JwtTokenIssuer.cs                NEW  same signing key AddTokenAuthentication validates
      LoggingInvitationSender.cs       NEW  writes the invitation link to the log — no mail server yet
    Migrations/
    InfrastructureConfiguration.cs     NEW  AddAccountsInfrastructure()
  Accounts.Web/
    Features/
      AccountsController.cs            NEW  Register, SignIn, AcceptInvitation, Me
      OrganizersController.cs          NEW  Onboard, All, Team, AddTeamMember, RemoveTeamMember, ChangeTeamRole
    WebConfiguration.cs                NEW  AddAccountsWebComponents()
```

The three `Services/` interfaces wrap things a test has to replace and that belong to a vendor or a
channel: the hashing algorithm, the token format, the delivery of mail. Use cases themselves stay in
their handlers. There is no `IAccountService`.

**Onboarding (ACC-A3), save order.** First find the Account by Email, or create and save an invited
one (ACC-R4). Then build and save the Organizer with that Account as its Owner. If the second save
fails, the retry finds the Account already there and uses it, exactly as ACC-R4 says for an
existing Account. So the two saves need no shared transaction. Adding a Team member (ACC-A4)
follows the same order.

### Invariants

| Rule | Domain | App validator | EF |
|---|---|---|---|
| One Account per Email address (ACC-R2) | `EmailAddress` normalises; handler refuses if `FindByEmail` finds one | email format | unique index on Email — the final guard against a race |
| An Account made for someone else gets an invitation, not a password (ACC-R3) | `AccountFactory` builds an invited Account with an `Invitation`; `Account.AcceptInvitation(…)` sets the password and clears it | password present | — |
| An Organizer starts with its Owner (ACC-R4) | `OrganizerFactory.Build()` requires a first Owner | name required | — |
| Nobody added twice; either Team role may be given (ACC-R5) | `Organizer.AddTeamMember` | role is Owner or Manager | unique (OrganizerId, AccountId) |
| Always an Owner (ACC-R6) | `Organizer.RemoveTeamMember` and `Organizer.ChangeTeamRole` refuse for the last Owner | — | — |
| Only an Owner runs the Team (ACC-A4/A5/A6 *Who can*) | — | — | — · `TeamAccess` in each handler → 403 |

**Events raised:** `Organizer` raises `TeamMemberAddedEvent` (for the first Owner at onboarding as
well), `TeamMemberRemovedEvent` and `TeamRoleChangedEvent`.

---

## Events

**Ownership.** Events owns what an Organizer puts on, up to and including publishing. Its key
invariants — what a Venue holds (EVT-R6), and what may change once Published (EVT-R8) — are checked
inside the `Event` aggregate.

**Copying the Venue onto the Event.** When an Event is created, it copies its Venue's name, address
and Capacity into an `EventVenue` value object. The capacity rule (EVT-R6) is then checked inside
the Event alone. The published-Event snapshot (see Seams) can also carry the Venue name without a
second lookup. This is safe while a Venue can't be changed once added (the EVT-A1 assumption) and an
Event's Venue can't change once Published (EVT-R8). If the PO allows editing a Venue, a Venue change
has to be pushed to its Events.

**Knowing what has sales.** EVT-R8 depends on whether a Performance or Ticket type *has sales*:
tickets on Confirmed or Pending payment Orders. Those Orders are Sales' data. Events keeps its own
`Sale` record per Order, updated from Sales' `OrderPlaced`, `OrderConfirmed` and `OrderExpired`
events. Before changing a Published Event, the handler reads these into a `SalesFigures` value object
and passes it to the domain method. Events never calls Sales.

### Files

```
Events/
  Events.Domain/
    Models/
      Venues/
        Venue.cs                       NEW  root: OrganizerId, VenueName, Address, Capacity — EVT-R2
        ModelConstants.cs              NEW  VenueModelConstants
      Events/
        Event.cs                       NEW  root: OrganizerId, name, description, ImageId?, EventVenue, Status,
                                            Performances, TicketTypes — EVT-R3–R10
        EventVenue.cs                  NEW  value object: VenueId, name, address, Capacity (copied at creation)
        Performance.cs                 NEW  child: PerformanceSchedule
        PerformanceSchedule.cs         NEW  value object: Date, DoorsTime, StartTime — doors ≤ start (EVT-R7)
        TicketType.cs                  NEW  child: name, Price, Quantity, OnSale (EVT-F12–F15)
        Price.cs                       NEW  value object: euros, > 0, two decimals (EVT-F13)
        EventStatus.cs                 NEW  Enumeration: Draft, Published (EVT-F8)
        SalesFigures.cs                NEW  value object: tickets with sales per Performance and per
                                            (Performance, Ticket type) — fed to EVT-R8 checks
        ModelConstants.cs              NEW  EventModelConstants
      Images/
        EventImage.cs                  NEW  root: content type + bytes (EVT-F6), kept apart so an Event loads light
      Sales/
        Sale.cs                        NEW  root: Sales' OrderId, PerformanceId, lines, HasSales flag
        SaleLine.cs                    NEW  child: TicketTypeId, number of tickets
      Teams/
        TeamMember.cs                  NEW  root: OrganizerId, AccountId, TeamRole — copy of Accounts'
        TeamRole.cs                    NEW  Enumeration: Owner, Manager; CanPublish
    Factories/
      IVenueFactory.cs · VenueFactory.cs              NEW
      IEventFactory.cs · EventFactory.cs              NEW  new Event is Draft (EVT-R3) at an own Venue (EVT-R4)
    Repositories/
      IVenueDomainRepository.cs        NEW
      IEventDomainRepository.cs        NEW
      IEventImageDomainRepository.cs   NEW
      ISaleDomainRepository.cs         NEW  Find by OrderId; FiguresForEvent(eventId) → SalesFigures
      ITeamMemberDomainRepository.cs   NEW  RoleOf(organizerId, accountId)
    Exceptions/
      InvalidVenueException.cs         NEW
      InvalidEventException.cs         NEW
    DomainConfiguration.cs             NEW  AddEventsDomain()
  Events.Application/
    Venues/
      Commands/Add/        AddVenueCommand.cs · AddVenueCommandValidator.cs · AddVenueResponse.cs        EVT-A1
      Queries/GetAll/      GetAllVenuesQuery.cs · VenueListItem.cs                                       EVT-V1
      IVenueQueryRepository.cs
    Events/
      Commands/
        Common/            ImageUpload.cs  (content type + bytes; shared by Create and ChangeDetails)
        Create/            CreateEventCommand.cs · CreateEventCommandValidator.cs · CreateEventResponse.cs   EVT-A2
        ChangeDetails/     ChangeEventDetailsCommand.cs · ChangeEventDetailsCommandValidator.cs              EVT-A3
        AddPerformance/    AddPerformanceCommand.cs · AddPerformanceCommandValidator.cs · AddPerformanceResponse.cs   EVT-A4
        ChangePerformance/ ChangePerformanceCommand.cs · ChangePerformanceCommandValidator.cs                EVT-A5
        RemovePerformance/ RemovePerformanceCommand.cs                                                       EVT-A6
        AddTicketType/     AddTicketTypeCommand.cs · AddTicketTypeCommandValidator.cs · AddTicketTypeResponse.cs   EVT-A7
        ChangeTicketType/  ChangeTicketTypeCommand.cs · ChangeTicketTypeCommandValidator.cs                  EVT-A8
        RemoveTicketType/  RemoveTicketTypeCommand.cs                                                        EVT-A9
        TakeTicketTypeOffSale/ TakeTicketTypeOffSaleCommand.cs                                               EVT-A11
        Publish/           PublishEventCommand.cs                                                            EVT-A10
      Queries/
        Team/              OrganizerEventsQuery.cs · OrganizerEventListItem.cs   (filter: Event status)      EVT-V2
        TeamDetails/       EventTeamDetailsQuery.cs · EventTeamResponse.cs                                   EVT-V3
        WhatsOn/           WhatsOnQuery.cs · WhatsOnListItem.cs                                              EVT-V4
        PublicDetails/     PublicEventQuery.cs · PublicEventResponse.cs                                      EVT-V5
        Image/             EventImageQuery.cs  (IRequest<Stream>)
      IEventQueryRepository.cs
    Common/
      TeamAccess.cs                    NEW
    Handlers/
      TeamMemberAddedEventHandler.cs · TeamMemberRemovedEventHandler.cs · TeamRoleChangedEventHandler.cs
      OrderPlacedEventHandler.cs · OrderConfirmedEventHandler.cs · OrderExpiredEventHandler.cs   (keep Sale up to date)
    Settings/
      EventsSettings.cs                NEW  time zone; image formats and max size (EVT-F6)
    EventsApplicationConfiguration.cs
  Events.Infrastructure/
    Persistence/
      EventsDbContext.cs               NEW
      EventsDbInitializer.cs           NEW
    Configurations/
      VenueConfiguration.cs · EventConfiguration.cs (EventVenue owned) · PerformanceConfiguration.cs
      TicketTypeConfiguration.cs (Price → cents) · EventImageConfiguration.cs · SaleConfiguration.cs
      SaleLineConfiguration.cs · TeamMemberConfiguration.cs (unique OrganizerId+AccountId)
    Repositories/
      VenueRepository.cs · EventRepository.cs · EventImageRepository.cs · SaleRepository.cs · TeamMemberRepository.cs
    Migrations/
    InfrastructureConfiguration.cs     NEW  AddEventsInfrastructure()
  Events.Web/
    Features/
      VenuesController.cs              NEW  Add, All
      EventsController.cs              NEW  team actions and views (EVT-A2–A11, V2, V3); binds the image from a multipart form
      WhatsOnController.cs             NEW  [AllowAnonymous]: WhatsOn, Details, Image
    WebConfiguration.cs                NEW  AddEventsWebComponents()
```

### Invariants

| Rule | Domain | App validator | EF |
|---|---|---|---|
| Scope: Team sees own Venues and Events; Draft only to its Team (EVT-R1) | — | — | — · `TeamAccess` (404 out of scope); public queries filter `Published` |
| Own Venues only (EVT-R2, EVT-R4) | `EventFactory` takes a Venue and refuses another Organizer's | — | — |
| Ticket types serve every Performance (EVT-R5) | quantities live on `TicketType`, not per Performance | — | — |
| Never more than the Venue holds (EVT-R6) | `Event.AddTicketType` / `ChangeTicketType` check the sum against `EventVenue.Capacity` | Quantity > 0 | — |
| Performance times (EVT-R7) | `PerformanceSchedule` (doors ≤ start); `Event.AddPerformance` / `MovePerformance` take `now` and refuse the past | required | `DateOnly` / `TimeOnly` |
| What can change once Published (EVT-R8) | each `Event` mutation takes `SalesFigures` and applies the EVT-R8 table when `Published` | — | — |
| Off sale (EVT-R10) | `TicketType.TakeOffSale()`; `RemoveTicketType` refuses one with sales | — | — |
| Draft → Published only (EVT-R3) | `Event.Publish()` | — | — |
| What publishing needs (EVT-R9) | `Event.Publish()` checks name, description, image, Venue, ≥ 1 Performance, ≥ 1 Ticket type | — | — |
| Only an Owner publishes (EVT-A10 *Who can*) | `TeamRole.CanPublish` | — | — · `TeamAccess` → 403 |
| Price: euros, > 0, two decimals (EVT-F13) | `Price` | same constants | cents |

**Events raised:** `Event.Publish()` raises `EventPublishedEvent`. Every later change to a
Published Event raises `PublishedEventUpdatedEvent` with the same snapshot, whether it's details, a
Performance added, moved or removed, or a Ticket type added, changed, removed or taken off sale.
Changes to a Draft raise nothing, because no other context knows the Event exists yet.

---

## Sales

**Ownership.** Sales owns everything about buying, and above all the rule that a Performance never
sells more than it offers (SAL-R6). Concurrent checkouts are what break that rule, so the design
starts there.

**No overselling, on SQLite.** The Availability check lives in Sales' own `Performance` aggregate.
That aggregate is Sales' copy of a published Performance and its Ticket types: Quantity, Price, On
sale. Starting checkout (SAL-A1) runs in this order:

1. Load the `Performance`, tracked.
2. Read the tickets on its Confirmed Orders and on its Pending payment Orders whose hold hasn't
   ended (`IOrderQueryRepository.ActiveTicketCounts`).
3. If the Attendee already has a Pending payment Order for this Performance, expire it (SAL-R15) and
   leave its tickets out of the count.
4. Call `Performance.Reserve(lines, counts, now)`. It enforces SAL-R3, R4 and R6 and moves the
   Performance's `Version` on.
5. Build the `Order` and save it.

That one SaveChanges writes the new Order, the expired one and the Performance's new `Version`
together. `Version` is a concurrency token, so if two checkouts for the same Performance race, one
of them fails with a concurrency conflict. The handler then retries from step 1 a bounded number of
times, and either succeeds or refuses with "Sold out". The same `Version` move happens when a late
payment takes tickets back (SAL-R13), and when Events lowers a Quantity. All three are serialised
per Performance, which is exactly the unit the invariant is about.

**Holds run out without a job having to run on time (SAL-R5).** Availability counts a Pending
payment Order only while `HoldEndsAt > now`, so a lapsed hold frees its tickets at that very moment.
`ExpireLapsedOrdersJob`, a `BackgroundService` in the host process (not new infrastructure), marks
lapsed Orders `Expired` every 30 seconds. That raises `OrderExpiredEvent` and makes the stored status
match. Until it runs, the Order queries show a lapsed Pending payment Order as Expired, and payment
refuses it.

**The payment provider.** The provider sits behind `IPaymentProvider` (Application, Services). It
has three operations: start a hosted payment for an amount and get back the page to redirect to;
take the provider's result notification; refund a payment. Card details only ever reach the
provider's page (SAL-R11). Which provider to use is engineering's call (an ADR). Until it is made,
`SimulatedPaymentProvider` stands in. Its "hosted page" is a small page in `web/` that pays with
approving or declining test cards and can be held open past the hold, which SC-SAL-008 and
SC-SAL-009 need. It posts the result to the same callback a real provider would.

**Who bought.** The Order keeps the buyer's Account id, Name and Email, taken from the token when
checkout starts. SAL-V3 lists them, and `OrderConfirmedEvent` passes the name on to Admission. Sales
never asks Accounts.

### Files

```
Sales/
  Sales.Domain/
    Models/
      Performances/
        Performance.cs                 NEW  root: PerformanceId, EventId, OrganizerId, event name, StartsAt (local),
                                            Version, OfferedTicketTypes — SAL-R3, R4, R6
        OfferedTicketType.cs           NEW  child: TicketTypeId, name, Price, Quantity, OnSale
        TicketCounts.cs                NEW  value object: tickets sold or held per Ticket type
        ModelConstants.cs              NEW  PerformanceModelConstants
      Orders/
        Order.cs                       NEW  root: Buyer, PerformanceId, PlacedAt, HoldEndsAt, Status, Lines, Payments,
                                            FaceValue, BookingFee, OrderTotal — SAL-R2, R5, R7–R10, R12, R13, R15
        OrderLine.cs                   NEW  child: TicketTypeId, ticket type name, NumberOfTickets, TicketPrice (SAL-R7)
        Payment.cs                     NEW  child: provider reference, PaymentResult
        Buyer.cs                       NEW  value object: AccountId, Name, Email
        Money.cs                       NEW  value object: euros, two decimals
        OrderStatus.cs                 NEW  Enumeration: PendingPayment, Confirmed, Expired (SAL-F4)
        PaymentResult.cs               NEW  Enumeration: Paid, Declined, Refunded (SAL-F12)
        ModelConstants.cs              NEW  OrderModelConstants: MaxTicketsPerOrder = 8, HoldPeriod = 10 min,
                                            BookingFeePerTicket = €1.50
    Factories/
      IPerformanceFactory.cs · PerformanceFactory.cs  NEW  from a published-Event snapshot
      IOrderFactory.cs · OrderFactory.cs              NEW  prices and fee fixed at placing (SAL-R7, R8, R9)
    Repositories/
      IPerformanceDomainRepository.cs  NEW  FindByPerformanceId (tracked), ForEvent(eventId)
      IOrderDomainRepository.cs        NEW  Find, PendingFor(attendee, performance), Lapsed(now)
    Exceptions/
      CheckoutRefusedException.cs      NEW  SAL-R3 refusals, with the message shown
      InvalidOrderException.cs         NEW
    DomainConfiguration.cs             NEW  AddSalesDomain()
  Sales.Application/
    Orders/
      Commands/
        StartCheckout/     StartCheckoutCommand.cs · StartCheckoutCommandValidator.cs · StartCheckoutResponse.cs   SAL-A1
        Pay/               PayOrderCommand.cs · PayOrderResponse.cs  (the provider page to redirect to)           SAL-A2
        RecordPaymentResult/ RecordPaymentResultCommand.cs  (provider callback: SAL-R12, SAL-R13 incl. auto-refund)
        ExpireLapsedOrders/  ExpireLapsedOrdersCommand.cs   (sent by the job: SAL-R5)
      Queries/
        Checkout/          OrderCheckoutQuery.cs · OrderCheckoutResponse.cs                                       SAL-V1
        PerformanceSales/  PerformanceSalesQuery.cs · PerformanceSalesResponse.cs · SoldOrderItem.cs              SAL-V3
      IOrderQueryRepository.cs         NEW  incl. ActiveTicketCounts(performanceId, now)
    Performances/
      Queries/
        Availability/      PerformanceAvailabilityQuery.cs · PerformanceAvailabilityResponse.cs  (Sold out flags only)  SAL-V2
        GetAll/            GetAllPerformancesQuery.cs · PerformanceListItem.cs  (Finance manager picks one for SAL-V3)
      IPerformanceQueryRepository.cs
    Handlers/
      EventPublishedEventHandler.cs · PublishedEventUpdatedEventHandler.cs   (create / update / drop Performance copies)
    Services/
      IPaymentProvider.cs              NEW  StartHostedPayment, Refund; HostedPayment and PaymentNotification records
    Settings/
      SalesSettings.cs                 NEW  time zone, provider settings, sweep interval, retry count
    SalesApplicationConfiguration.cs
  Sales.Infrastructure/
    Persistence/
      SalesDbContext.cs                NEW
      SalesDbInitializer.cs            NEW
    Configurations/
      PerformanceConfiguration.cs      NEW  Version as concurrency token; unique PerformanceId
      OfferedTicketTypeConfiguration.cs · OrderConfiguration.cs (index PerformanceId+Status+HoldEndsAt;
      index Buyer AccountId+PerformanceId) · OrderLineConfiguration.cs · PaymentConfiguration.cs  — money as cents
    Repositories/
      PerformanceRepository.cs · OrderRepository.cs
    Payments/
      SimulatedPaymentProvider.cs      NEW  development stand-in for the hosted page
    Services/
      ExpireLapsedOrdersJob.cs         NEW  BackgroundService → ExpireLapsedOrdersCommand
    Migrations/
    InfrastructureConfiguration.cs     NEW  AddSalesInfrastructure() — also registers the job and the provider
  Sales.Web/
    Features/
      OrdersController.cs              NEW  StartCheckout, Checkout/{id}, Pay/{id}
      PaymentsController.cs            NEW  [AllowAnonymous] Callback — verified as the provider's before anything changes
      PerformancesController.cs        NEW  Availability/{id} [AllowAnonymous]; All and Sales/{id} [FinanceManager]
    WebConfiguration.cs                NEW  AddSalesWebComponents()
```

### Invariants

| Rule | Domain | App validator | EF |
|---|---|---|---|
| Scope: Attendee sees own Orders; Finance manager sees all (SAL-R1) | — | — | — · Order queries filter by caller; SAL-V3 by role |
| One status at a time, following the Order's life (SAL-R2) | `Order` methods: `Confirm`, `Expire`, `RecordLatePayment` | — | — |
| What refuses checkout (SAL-R3) | `Performance.Reserve` (not on sale, past start, over Availability) | — | — |
| 1–8 tickets, one Performance (SAL-R4) | `OrderFactory` | each line 1–8, sum ≤ 8 | — |
| Ten-minute Hold (SAL-R5) | `OrderFactory` sets HoldEndsAt; counts use `HoldEndsAt > now` | — | index on HoldEndsAt |
| Availability never oversold (SAL-R6) | `Performance.Reserve` against `TicketCounts` | — | `Version` concurrency token |
| Price at the time (SAL-R7) | `OrderLine.TicketPrice` copied at placing | — | — |
| Booking fee, Face value, total (SAL-R8, R9) | computed once in `OrderFactory` and stored | — | cents |
| Fee shown before paying (SAL-R10) | — | — | — · `OrderCheckoutResponse` returns the three amounts separately |
| Card details stay with the provider (SAL-R11) | — | — | — · no card field anywhere in the model or the API |
| Paying while the Hold lasts; Declined stays Pending (SAL-R12) | `Order.StartPayment(now)`, `Order.RecordPayment(result, now)` | — | — |
| Late payment: confirm if still free, else refund (SAL-R13) | `Performance.Reserve` for the Order's own lines → `Order.Confirm`, otherwise `Order.MarkRefunded` after `IPaymentProvider.Refund` | — | — |
| Sold out, never a count (SAL-R14) | — | — | — · `PerformanceAvailabilityResponse` holds booleans only |
| One checkout per Performance (SAL-R15) | StartCheckout expires the earlier Pending Order in the same save | — | index Buyer+Performance |

**Events raised:** `Order` raises `OrderPlacedEvent`, `OrderConfirmedEvent` (on time or late) and
`OrderExpiredEvent` (hold ran out, or replaced). A late payment that is refunded raises nothing more:
the Order was already Expired.

---

## Admission

**Ownership.** Admission owns Tickets: one per person getting in, each with its own code. Everything
else it shows is copied in: show details from Events, the buyer's name and the Ticket types from
Sales, Team roles from Accounts. It never reads another context's tables.

### Files

```
Admission/
  Admission.Domain/
    Models/
      Tickets/
        Ticket.cs                      NEW  root: TicketCode, OrderId, PerformanceId, TicketTypeId, ticket type name,
                                            Buyer AccountId, Buyer's name — ADM-R2
        TicketCode.cs                  NEW  value object: new random code from the BCL crypto RNG (ADM-R3)
        ModelConstants.cs              NEW  TicketModelConstants (code length)
      Performances/
        Performance.cs                 NEW  root: PerformanceId, OrganizerId, event name, venue name, Date, DoorsTime, StartTime
      Teams/
        TeamMember.cs · TeamRole.cs    NEW  copy of Accounts' memberships
    Factories/
      ITicketFactory.cs · TicketFactory.cs            NEW  one Ticket per ticket on a Confirmed Order
      IPerformanceFactory.cs · PerformanceFactory.cs  NEW
    Repositories/
      ITicketDomainRepository.cs       NEW  ExistsForOrder(orderId) — so a repeated event issues nothing twice
      IPerformanceDomainRepository.cs · ITeamMemberDomainRepository.cs
    DomainConfiguration.cs             NEW  AddAdmissionDomain()
  Admission.Application/
    Tickets/
      Queries/
        Mine/              MyTicketsQuery.cs · MyTicketItem.cs                                ADM-V1
        DoorList/          DoorListQuery.cs · DoorListResponse.cs · DoorListItem.cs           ADM-V2
      ITicketQueryRepository.cs
    Common/
      TeamAccess.cs                    NEW
    Handlers/
      OrderConfirmedEventHandler.cs    NEW  issues the Tickets (ADM-R2)
      EventPublishedEventHandler.cs · PublishedEventUpdatedEventHandler.cs
      TeamMemberAddedEventHandler.cs · TeamMemberRemovedEventHandler.cs · TeamRoleChangedEventHandler.cs
    AdmissionApplicationConfiguration.cs
  Admission.Infrastructure/
    Persistence/
      AdmissionDbContext.cs · AdmissionDbInitializer.cs
    Configurations/
      TicketConfiguration.cs           NEW  unique index on TicketCode (ADM-R3); index OrderId, PerformanceId, Buyer
      PerformanceConfiguration.cs · TeamMemberConfiguration.cs
    Repositories/
      TicketRepository.cs · PerformanceRepository.cs · TeamMemberRepository.cs
    Migrations/
    InfrastructureConfiguration.cs     NEW  AddAdmissionInfrastructure()
  Admission.Web/
    Features/
      TicketsController.cs             NEW  Mine, DoorList/{performanceId}
    WebConfiguration.cs                NEW  AddAdmissionWebComponents()
```

Printing the Door list is a print stylesheet on the frontend page. There is no PDF generation and no
new library.

### Invariants

| Rule | Domain | App validator | EF |
|---|---|---|---|
| Scope: Attendee sees own Tickets; Team sees own Organizer's (ADM-R1) | — | — | — · `MyTicketsQuery` filters by caller; `DoorListQuery` → `TeamAccess` (404 out of scope) |
| One Ticket per person, only from a Confirmed Order (ADM-R2) | `TicketFactory`; issued only from `OrderConfirmedEvent`; `ExistsForOrder` makes it idempotent | — | — |
| Every Ticket has its own code (ADM-R3) | `TicketCode` is random and long enough that a clash is negligible | — | unique index — on a clash, generate a new code and save again |

---

## Seams — what crosses between contexts

No context references another. Everything below is a domain event, with its contract in
`Common.Domain/Events/<Aggregate>/`, carrying primitives only (nested records of primitives for
lists). It is written to the publisher's outbox in the same save as the aggregate, and a background
dispatcher delivers it at least once ([ADR-005](adr/005-event-delivery-outbox.md)). Each consumer
turns the event into its own model in `<Ctx>.Application/Handlers/`, idempotently.

| Event (contract) | Raised by | Payload | Consumed by → what it does |
|---|---|---|---|
| `Organizers/TeamMemberAddedEvent` | Accounts `Organizer` (ACC-A3, A4) | OrganizerId, AccountId, TeamRole (string) | Events, Admission → add `TeamMember` |
| `Organizers/TeamMemberRemovedEvent` | Accounts `Organizer` (ACC-A5) | OrganizerId, AccountId | Events, Admission → remove `TeamMember` |
| `Organizers/TeamRoleChangedEvent` | Accounts `Organizer` (ACC-A6) | OrganizerId, AccountId, TeamRole | Events, Admission → update `TeamMember` |
| `Events/EventPublishedEvent` | Events `Event` (EVT-A10) | EventId, OrganizerId, EventName, VenueName, VenueAddress, Performances[PerformanceId, Date, DoorsTime, StartTime], TicketTypes[TicketTypeId, Name, Price, Quantity, OnSale] | Sales → create `Performance` copies · Admission → create `Performance` copies |
| `Events/PublishedEventUpdatedEvent` | Events `Event` (any change after publishing, EVT-R8) | same snapshot | Sales → upsert / drop copies, move `Version` on · Admission → upsert copies |
| `Orders/OrderPlacedEvent` | Sales `Order` (SAL-A1) | OrderId, PerformanceId, Lines[TicketTypeId, Number] | Events → `Sale` with sales |
| `Orders/OrderConfirmedEvent` | Sales `Order` (SAL-R12, R13) | OrderId, PerformanceId, OrganizerId, BuyerAccountId, BuyerName, Lines[TicketTypeId, TicketTypeName, Number] | Admission → issue Tickets · Events → `Sale` with sales (again, for a late payment) |
| `Orders/OrderExpiredEvent` | Sales `Order` (SAL-R5, R15) | OrderId, PerformanceId | Events → `Sale` no longer has sales |

**Snapshots rather than one event per change.** `PublishedEventUpdatedEvent` carries the whole
published Event each time. Consumers replace their copy, which makes every handler idempotent and
the order of events harmless. It also means Events doesn't need a separate event for each row of
the EVT-R8 table.

**What isn't an event.**
- The public event page shows details from Events (EVT-V5) and Sold out flags from Sales (SAL-V2).
  **The frontend calls both.** Sold out changes whenever a hold runs out, so Events must not keep a
  copy of it.
- Who is calling, and their platform role: the token.

---

## Two flows end to end

**Buying (SC-SAL-006, SC-ADM-002).**

1. The browser opens the Event through Events' `WhatsOnController.Details`, and its Sold out flags
   through Sales' `PerformancesController.Availability`.
2. The browser calls Sales' `OrdersController.StartCheckout`. The Order is Pending payment, with
   `HoldEndsAt` set, and `OrderPlacedEvent` updates Events' `Sale`.
3. The browser calls `OrdersController.Pay`, which returns the provider's page. The browser goes
   there, and the provider calls `PaymentsController.Callback`, which sends
   `RecordPaymentResultCommand`.
4. The Order is Confirmed. `OrderConfirmedEvent` → Admission issues one Ticket per ticket with a
   new code. Events' `Sale` still has sales.
5. The browser shows Admission's `TicketsController.Mine`. The Team's `DoorList` reads Admission's
   own Tickets and Performance copy.

**Changing a Published Event (SC-EVT-002, SC-EVT-003).**

1. Events' `ChangeTicketTypeCommand` loads the `Event` and the `SalesFigures` built from its local
   `Sale` records, and calls `Event.ChangeTicketType(…)`, which applies EVT-R6 and EVT-R8.
2. The Event is saved, raising `PublishedEventUpdatedEvent`.
3. Sales updates its `Performance` copy and moves its `Version` on. A checkout running at that
   moment conflicts on `Version`, retries, and sees the new Quantity.

---

## Frontend

`source-code/web/` stays the single frontend: one feature folder per context, each calling only its
own context's endpoints. A page that needs two contexts, like the event page, calls both.

```
web/src/
  lib/api.ts                 NEW  fetch wrapper: base /api, bearer token, ProblemDetails → message
  lib/auth.tsx               NEW  current Account (from Accounts' Me), token, platform role, Teams
  features/accounts/         NEW  register, sign in, choose password, organizers, team
  features/events/           NEW  venues, events list, event editor (Performances / Ticket types tabs), what's on, event page
  features/sales/            NEW  checkout and order page, simulated payment page, performance sales
  features/admission/        NEW  my tickets, door list (print stylesheet)
  App.tsx                    MOD  routes
  components/app-sidebar.tsx MOD  navigation by role
```

The shell has no router yet, so adding `react-router` is the one new frontend dependency. It is a
library, not infrastructure. Screens follow the `shadcn-craft` and `tecknoworks-brand` skills. Labels
match the ones the scenarios in `scenarios/` assume.

## Risks and the tests that guard them

| Risk | What would tempt it | Guard |
|---|---|---|
| A context references another | "Events just needs one Sales query for *has sales*" | `ProjectReferenceTests` (already there) |
| A domain object inside an event contract re-couples contexts through `Common` | putting `Ticket type` or `Order` in a payload "to save mapping" | NEW `EventContractTests`: every type in `Common.Domain/Events/**` has only primitive, `Guid`, `decimal`, date/time, string or array-of-such-record properties — reflection only, no new package |
| `TeamMember` is moved into `Common` because three contexts have one | duplication looks like waste | review rule: each is its own copy; no shared business type in `Common` |
| **Lost event on the money path:** the process stops after an Order is saved Confirmed but before `OrderConfirmedEvent` reaches Admission, so someone paid and has no Tickets. The same gap on `TeamMemberRemovedEvent` leaves a removed member with access | the in-process dispatcher looks like a bus | the outbox ([ADR-005](adr/005-event-delivery-outbox.md)); idempotent handlers, so delivering twice is safe |
| Quantity lowered in Events while a checkout lands in Sales: the two run in different contexts, so Events can allow 10 while Sales has just held the 11th | — | Sales clamps Availability at zero (SAL-R6), and the window is milliseconds; accepted, named in the ADR on overselling |
| Duplicate type names in the global namespace break the host or Swagger | copying `Common`'s namespace-less style into contexts | namespaces per context project; Swagger `CustomSchemaIds` |
| A refusal enforced only in the UI | hiding a button feels finished | refusals live in handlers and the domain; the scenarios' "try it another way" steps check it |

## Decisions to record as ADRs

The choices the specs leave to engineering are recorded in [`adr/`](adr/README.md), each
naming the spec entries that raised it:

1. [ADR-001](adr/001-identity-tokens-and-team-roles.md): identity. Self-issued JWT with platform
   roles only, Team roles copied into each context, password rules, session length.
2. [ADR-002](adr/002-no-overselling-on-sqlite.md): no overselling on SQLite. Concurrency token on
   Sales' `Performance`, with bounded retry.
3. [ADR-003](adr/003-hold-expiry.md): holds run out by time, plus a background sweep.
4. [ADR-004](adr/004-payment-provider-port.md): payment behind a hosted-page port; simulated
   provider until the real one is chosen.
5. [ADR-005](adr/005-event-delivery-outbox.md): cross-context events through an outbox.
6. [ADR-006](adr/006-money-and-time-on-sqlite.md): money as cents, local `DateOnly` / `TimeOnly`,
   UTC instants, one time zone.
7. [ADR-007](adr/007-invitation-delivery.md): invitation links behind a sender port, logged until
   mail is chosen.
8. [ADR-008](adr/008-event-images.md): Event images in SQLite: JPEG/PNG/WebP, 2 MB.
9. [ADR-009](adr/009-ticket-code-format.md): Ticket code format.

## Build order

Each step is usable on its own and unlocks the next.

1. **Foundation changes**: `ICurrentUser`, `ForbiddenException`, authentication in the host,
   `TimeProvider`, Swagger, the event contracts, the outbox.
2. **Accounts**: registration, sign-in, onboarding, the Team. Everything else needs someone to sign
   in.
3. **Events**: Venues, Events, publishing, what's on. Membership handlers first; `Sale` handlers can
   follow once Sales exists.
4. **Sales**: published-Event handlers, checkout, the simulated provider, payment, expiry, sales
   list.
5. **Admission**: ticket issuing, My tickets, the Door list.

After each context: run `ProjectReferenceTests` and `EventContractTests`, then the context's
scenarios in `scenarios/<CODE>/`.

## As built — where the code differs from this plan

The four contexts are implemented as planned. These are the places the build departed from the text above;
the plan was not changed, so read this as the current truth.

- **Table names are prefixed per context** (`Sales_Orders`, `Admission_Performances`). All contexts share one
  SQLite file and Sales and Admission both have a `Performances` table, so `BaseDbContext` prefixes every
  table with its context's name. Each context still owns its own tables and migrations.
- **Event pictures travel as base64 in JSON**, not as a multipart form, so the Application layer never sees
  an ASP.NET type (`ImageUpload.Content`). The 2 MB limit and the first-bytes check are in
  `Events.Application`, not in `EventsSettings`.
- **`TeamAccess` in Events is `EventAccess`**, and also loads the Event and the sales figures (EVT-R8).
- **Events are deferred.** `Entity.RaiseEvent(Func<IDomainEvent>)` builds a payload at save time, once EF has
  given new children their Ids, so a snapshot can mention a Performance added in the same save.
- **SAL-V2 is asked per Event**, not per Performance: `GET Performances/Availability?eventId=` answers for all
  the Event's Performances at once.
- **The simulated payment page** lives behind `ISimulatedPaymentPages` in Application and is reachable only in
  Development (`SimulatedPaymentsController`).
- **`DataRepository.Save` is `virtual`**, so the Order repository can turn a concurrency conflict into
  `ConcurrencyConflictException` (ADR-002).
- **Queries read aggregates and map in memory** rather than `ProjectTo`: the Price and Money value objects are
  stored as cents through converters, which EF cannot project through.
- **Platform staff are seeded from configuration** (`Accounts:SeededStaff`); no use case in the specs creates
  a Platform admin or the Finance manager.
- **Eventual consistency window.** Events learns of a new Order about a second after it is placed, so for that
  moment a Performance can still be removed (EVT-R8). ADR-005 documents the delay.
