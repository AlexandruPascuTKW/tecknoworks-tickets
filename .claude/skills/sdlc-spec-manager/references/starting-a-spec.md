# Starting a spec from existing material

How to turn a codebase, meeting transcripts, a PRD, a design document, scattered tickets or
scenario files written without a spec into a first spec in the format of `spec-format.md`. The sources differ in what they contain, not in what
you do with them: read the whole of it, ask what the domain *is* rather than how the product *does*
it, and route the rest.

## Scope: one input, one spec

Whatever you were handed — a document, a transcript, an area of a codebase — is one spec's scope.
Don't split it, even when it plainly covers more than one area: a reader given one document
expects one spec, splitting scatters rules that are read together, and the boundary you'd draw is a
guess about someone else's mental model. Organise a wide spec with `###` groups instead. If the
structure genuinely strains, say so and let the owner decide.

Several inputs, or several named areas, make several specs. Pointed at a whole codebase with no
guidance, propose a grouping and get it confirmed before writing.

## Product or technical?

For each statement in the source, ask in order:

1. **Could a competent implementer, holding the spec and the codebase, arrive at this without being
   told?** If yes, leave it out — it is noise.
2. **Is it what the domain *is*, or how the product *does* it?** Vocabulary, actions, views,
   fields and rules are the first. Screens, flows, wording, endpoints and schemas are the second.
3. **Would the product owner need to validate it?** If not, it belongs in a technical spec.

"The domain" is broader than what a user clicks: background jobs and integrations belong in a spec
when people depend on their results — a nightly overdue run raises Fines even though nobody presses
a button. Naming an external system in a rule is fine ("Fines are paid through the Payments spec's
service"); how the integration is built is not.

A rule **narrows** the solution space; it does not fill it. "Only authenticated staff lend Items"
is a rule; "use OAuth2" is a technical decision that would survive no change of provider.

## Abstract up

Every source hands you implementation. Abstracting it is the work, not a clean-up:

| Source says | The spec says |
|-------------|---------------|
| `POST /api/loans {memberId, barcode}` | Action: Lend an Item — *Input:* Member, Item |
| "the checkout screen greys out the button when the member owes €10" | Rule: what refuses a loan includes the Fine limit — the screen is not the spec |
| `if (loan.renewals >= 2) throw` | Rule: a Loan is renewed at most twice — marked ❓ until the product owner confirms it |
| "on validation failure show inline errors" | Nothing — that is a scenario |
| "make holds a separate page, not a modal" | Not the spec — the client overrode the default, so note it for the scenarios rather than dropping it |
| "no waivers in v1" | Not the spec — increment scope belongs in the backlog |

Never carry the source's wording across unexamined: a statement copied verbatim is one nobody
abstracted, and vague source words ("where required", "configured") stay vague (KI-20).

## By kind of source

- **A codebase.** Read it for rules — derived values, state changes, permission checks, invariants,
  validation — and for the actions and views its endpoints and handlers expose (a command or
  mutation is usually an action, a read or query a view). Ignore layout,
  field order, routes, messages and flows. A rule read out of code can't be told apart from an
  accident of that code, so mark each one `*[❓ Needs PO clarification — … Inferred from the code.]*`
  until someone with authority confirms it. Set each entry's status with `sdlc-build-status`, not by
  eye.
- **Transcripts.** Separate decisions from discussion: write what was agreed as the rule, with ❓
  where it was only proposed. When the client argues against the implementer's default ("I know a
  modal is less work, but…"), that is a decision — route it to the scenarios or a technical spec,
  never to the bin.
- **Documents and tickets.** Pull the vocabulary into the Glossary first; then the actions and
  views; then the rules each one needs. Acceptance criteria are scenario material.
- **Scenario files written without a spec.** Their Covers lines are the rules: each one in plain
  words becomes a domain rule with an ID, marked
  `*[❓ Needs PO clarification — … Taken from scenario SC-BRW-001.]*` until the product owner
  confirms it — a scenario shows one case, and the rule behind it may be wider or narrower than
  that case. Where Covers holds only a ticket, read the ticket for the rule. The actions and views
  are what the steps walk through; their labels and screens stay in the scenarios. Then update the
  scenarios with `sdlc-scenario-manager`: Covers cites the new IDs, each scenario gains a
  `Use cases:` line, and the file moves to mirror the spec's path. Their `SC-` IDs stay as they are: an ID is never renumbered, and tickets or tests may already cite it.
- **Modernisation.** Write the spec for the *target* state — the domain as it should be. The
  difference between the spec and the codebase is the modernisation backlog.

## Then

Write the Context, Glossary and the first stage's actions and views, stop for confirmation, and
carry on a stage at a time as in the main workflow. Route what doesn't belong rather than deleting
it: behaviour to the scenarios (`sdlc-scenario-manager`), technical content to a technical spec,
increment scope to the backlog. If the project has nowhere to put it, say so and keep it in a
holding note — cutting it would be deferral pretending to be concision.
