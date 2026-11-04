# Bounded contexts — TecknoworksTickets

**Status:** Proposed · **Date:** 2026-11-04
**Covers:** Increment 1 (`transcripts/01-first-on-sale-events-accounts-and-checkout.md`)
**Not yet checked against:** the client documents in `documents/`, increment 1b (PO answers), increments 2–4

This note splits the increment-1 requirements into bounded contexts for the .NET modular monolith in
`source-code/`. Each context gets its own Domain / Application / Infrastructure / Web projects, wired
together only in `ProjectStartup`. It is meant as the outline for the domain specs in `specs/`: one
spec per context.

## The split

| Context | Owns | Main invariant it protects |
|---|---|---|
| **Accounts** | User logins (email + password), organisations, team memberships (Owner / Manager), the platform-admin role | One login per email. A user can be an attendee and a team member at the same time. Only a platform admin creates an organisation and its first Owner, and only an Owner manages the team. |
| **Events** | Venues, events, performances, ticket types, draft → published | The ticket-type quantities for an event add up to no more than the venue's capacity. Prices are in euros, above zero, two decimals. Drafts are visible only to the organisation's team. |
| **Sales** | Each performance's stock of each ticket type, holds, orders, the booking fee, the card payment step, sold-out | **Sold + held ≤ quantity for each (performance, ticket type).** Also: at most 8 tickets per order, a 10-minute hold, one performance per order, Pending payment → Confirmed or Expired, and the price is saved on the order when it's placed. |
| **Admission** | Individual tickets and their codes, "My tickets", the printable door list | One unique code per person getting in. Tickets exist only for confirmed orders. |

## Why it's cut this way

**Accounts holds both logins and organisations.** It's tempting to split Identity from Organizers.
But onboarding creates an organisation *and* its first Owner's login in one step, and an Owner adding
someone by email has to find or create a login. A step that writes to two contexts at once means the
line between them is in the wrong place. "Owner" and "Manager" also only mean something attached to an
organisation. Every other context gets `UserId`, `OrganisationId` and the user's role as claims in
their login session, so no other context calls Accounts while handling a request.

**Venue stays inside Events.** The capacity rule compares a venue's capacity with the event's ticket
quantities. If Venue were its own context, that rule would cross a boundary. This may change: see
open question 1.

**Sales is separate from Events.** They describe the same show with different words. Events talks
about what is on: show, dates, prices. Sales talks about what can still be bought: available, held,
sold, order. Organizers edit events at an office pace. Sales takes the load of everyone buying at
once, such as the Ana Lux on-sale. The no-oversell rule has to be checked in one transaction inside
Sales, so stock, holds and orders sit together there.

**Admission is separate from Sales.** In Sales a ticket is a line on an order (quantity × type). In
Admission a ticket is one person's way in, with its own code. Increment 2 adds scanning, check-in,
door staff and transfers, all of which belong to Admission. Scanning on show night and buying during
an on-sale are also different peak loads.

## How the contexts talk to each other

| Need | How | What's passed |
|---|---|---|
| Who is calling, and in which role | Claims in the login session, issued by Accounts | `userId`, `isPlatformAdmin`, `memberships[{organisationId, role}]` |
| Sales needs the performances, ticket types, prices and quantities | Domain event, Events → Sales | `EventPublishedEvent`: event id, organisation id, performances (id, start), ticket types (id, name, price, quantity). Sales builds its own stock and price list from it. |
| Admission needs show details for "My tickets" and the door list | The same `EventPublishedEvent`, kept as Admission's own copy | Event name, venue name, each performance's date, doors time and start time |
| Turning a confirmed order into tickets | Domain event, Sales → Admission | `OrderConfirmedEvent`: order id, performance id, organisation id, buyer id, buyer name, lines (ticket type id, ticket type name, quantity) |
| The event page shows details plus sold-out | The frontend calls both | Event details come from Events and availability from Sales. Sold-out changes whenever a hold expires, so Events shouldn't keep a copy. |

Cross-context event contracts live in `Common.Domain/Events/<Aggregate>/` and carry primitives only.
No context references another context's projects, and no table has a foreign key into another
context's tables.

**Risk:** `OrderConfirmedEvent` follows a card payment. With in-process events, a crash between saving
and dispatching leaves a customer who paid but has no tickets. That one link needs an outbox (save the
event with the order, send it from a background reader) or a regular check that fixes the gap. To be
recorded as an ADR.

## Deliberately not contexts yet

- **Payments.** For now, the payment provider is just a connector inside Sales. It opens the hosted
  page and gets back paid or declined, and has no business rules of its own. Making it a context now
  would put "payment succeeded" on the same unreliable event path described above. It becomes a real
  context in increment 4, when refunds and organizer payouts give money its own vocabulary.
- **Booking-fee policy.** A flat €1.50 per ticket is calculated in Sales and saved on the order. When
  the tiered fees from the finance policy arrive, they probably belong to that later finance context.
- **Platform admin.** This is a role in Accounts, not a context.
- **Reporting.** Planned for increment 5. The finance manager's "what was sold per performance" list
  is a view in Sales for now.

## Open questions that would move the boundaries

Most of the PO's open questions stay inside one context: a limit of 8 per order versus per person,
holds in two tabs, showing "only 12 left". Only these four change the structure:

1. **Shared venues (Dev 2, 10:12).** If two organizers share one Tram Depot record, venues become
   platform-wide data and will probably move into their own context. Events would then keep a copy of
   the capacity to check against. Increment 3's seated sections push the same way.
2. **Edits after publishing (10:26).** This decides which further events Events sends to Sales and
   Admission (price changed, performance moved, quantity changed). It also decides what happens when a
   quantity goes below what's already sold.
3. **Card approved after the hold expired (10:41).** If that payment confirms the order, the
   no-oversell rule can break. Otherwise someone has to refund the money, which pulls refunds forward
   from increment 4.
4. **Who the finance manager is in the system.** Platform staff with read access across all
   organizers is a role the transcript doesn't define, and Accounts would need it.

## Looking ahead

| Increment | Expected effect on the split |
|---|---|
| 2 — scanning, door staff, transfers | Door staff become a new team role in Accounts. Scanning, check-in counts and transfers go in Admission. |
| 3 — reserved seating | Venues with sections may become their own context (see question 1). Stock in Sales moves from counts per ticket type to individual seats but stays in Sales. |
| 4 — cancellations, refunds, payouts | A Finance context appears. It listens for Sales' events, owns refunds and organizer payouts, and may take over the payment connector and the fee policy. |
