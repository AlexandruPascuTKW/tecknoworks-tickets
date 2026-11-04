# ADR-004: Card payment behind a hosted-page port, with a simulated provider until a real one is chosen

**Status:** Proposed

**Date:** 2026-11-04

**Deciders:** Alexandru Pascu (proposed); to be accepted by the development team

**Technical Story:** [`architecture.md` — Sales](../architecture.md#sales)

**Raised by:** Sales — SAL-A2 (Pay for an Order), SAL-R11 (card details stay with the Payment
provider), SAL-R12 (paying while the Hold lasts), SAL-R13 (a late payment is refunded automatically
when its tickets are gone). The increment 1 transcript: "we'd use a hosted payment page from a
provider, so we never see card numbers. Which provider is engineering's call."

---

## Context

Attendees pay by card on the provider's own hosted page, so TecknoworksTickets never receives a card
number (SAL-R11). Sales needs three things from a provider:

1. Start a payment for an Order's exact total, and get back the page to send the Attendee to.
2. Learn the result (Paid or Declined), even when it arrives after the hold has run out (SAL-R13),
   and be sure the result really came from the provider.
3. Refund a whole payment automatically when a late payment's tickets are gone (SAL-R13).

**Constraints.** No provider has been chosen, so there are no contract, keys or sandbox account. The
workshop must run the full loop locally with no outside service. Scenarios SC-SAL-007 to SC-SAL-009
need a declining card, and a payment that completes after the hold has run out.

---

## Decision

**We have decided to put the provider behind `IPaymentProvider` in Sales.Application, shaped around
a hosted payment page, and to implement it with a `SimulatedPaymentProvider` for development and the
workshop. Choosing the real provider is a separate decision, made before go-live in its own ADR.**

- **Port:** `IPaymentProvider.StartHostedPayment(orderId, amount, returnUrl)` returns a provider
  reference and the page to redirect to; `Refund(providerReference)`. Results arrive through
  `PaymentsController.Callback` (anonymous), which must verify the result came from the provider
  before sending `RecordPaymentResultCommand`.
- **Idempotency:** a result for a provider reference already recorded changes nothing. Providers send
  results more than once.
- **Simulated provider:** its "hosted page" is a small page in `web/`, outside the app's normal
  screens. It shows the amount and offers *approve* and *decline* test cards. It can be left open
  past the hold, and it posts a signed result to the same callback a real provider would. It is
  registered in Development only. `Refund` records the refund and shows it on that page's history,
  which SC-SAL-009's "the Payment provider shows the whole payment refunded" step reads.
- **No card fields** in any request, response, table or log on our side.

---

## Rationale

1. **The workshop can build and test the whole flow today,** declined cards and late payments
   included, with nothing outside the repository.
2. **Choosing a provider is separate from building checkout.** The real provider's adapter
   replaces the simulated one in Sales.Infrastructure; handlers, the domain and the frontend flow
   don't change.
3. **The port fits every mainstream provider.** "Redirect to a hosted page, receive a signed
   notification, refund by reference" is what hosted-checkout products have in common.
4. **SAL-R11 holds structurally.** There is nowhere in our model to put a card number.

---

## Alternatives Considered

### Alternative 1: Pick a provider now and integrate its sandbox

**Pros:** real behaviour from day one; finds integration surprises early.
**Cons:** needs a merchant account and keys before anyone can run the app; ties workshop runs to
an outside service and network; commercial terms are the business's to agree.
**Why Rejected:** blocks local development on a business decision that hasn't been made.

### Alternative 2: Fake payments inside Sales (no port)

**Pros:** least code.
**Cons:** the real integration later means changing handlers; the fake can't model a hosted page, a
callback or a late completion, so SC-SAL-008 and SC-SAL-009 can't be run.
**Why Rejected:** saves little now and costs a rewrite later.

---

## Consequences

### Positive Consequences

- ✅ The full checkout loop runs locally (SC-SAL-005 to SC-SAL-009).
- ✅ Swapping in the real provider touches one Infrastructure class, its settings and the callback
  verification.

### Negative Consequences

- ⚠️ **The simulator can hide real-world behaviour:** duplicate or out-of-order notifications,
  provider downtime, partial failures. Mitigation: the simulator can send a result twice; the
  real-provider ADR lists what to test in the provider's sandbox.
- ⚠️ **A refund can fail** at the provider. Increment 1 records the failure for the Finance manager
  to resolve by hand. Retries and refunds that are never sent twice are increment 4's problem (the
  transcript: "ours to make safe").

### Neutral/Unknown Consequences

- 🤷 Increment 4 may move the payment connector into a Finance context. The port moves with it.

---

## Notes

- **Open, before go-live:** a follow-up ADR choosing the provider (candidates: any product with a
  hosted checkout page, signed notifications and a refund API), with its fees and terms agreed by the
  Finance manager.
- Related: ADR-002 (a late payment takes tickets back through the same guard), ADR-005 (Order events
  after payment).

---

*Last Updated: 2026-11-04 by Alexandru Pascu*
