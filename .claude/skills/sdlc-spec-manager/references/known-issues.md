# Known issues

Each issue has an id, `KI-<n>`, that never changes and is never reused. Refer to it by that id —
in reviews, commits and PRs — never as `#<n>`, which GitHub reads as an issue or PR number. The
ids match the leads `scripts/check_refs.py` prints.

The defects spec reviews keep finding, most frequent first. Review mode checks every one of them.
**[script]** means `check_refs.py` flags candidates; a reviewer still judges each. **[review]** means
only a reader can find it. Examples use the library lending domain of
`assets/example/specs/LND/lending.md`.

Add an issue here when a review finds the same kind of defect a second time. One-offs stay out.

## KI-1 — A rule hidden in a glossary definition  [script + review]

**Spot it:** a definition that says what must, never, always or only happens, who may do it, or
what gets recorded — not just what the term means. "Never renewed while overdue"; "with a reason";
"Librarian or Branch Manager".
**Fix:** cut the definition back to the meaning; move the constraint to a rule (or to *Who can*).
What makes the term what it is ("until the Item comes back or is declared lost") stays.
The most common defect by far.

## KI-2 — A term undefined, thin, unused or drifting  [script + review]

**Spot it:** a capitalised term in rules or use cases that the Glossary doesn't define; a glossary
term nothing uses; a defined term written in lowercase or with a synonym; one word with two
meanings ("hold" as a Hold on a Title and as an account hold on a Member); a definition a newcomer
couldn't act on.
**Fix:** define it, use the defined term verbatim, or split the two meanings into two terms.

## KI-3 — Two rules contradict each other  [review]

**Spot it:** read rules that share a term together — especially an *only*/*never* rule against a
*can*/*may* one, and any new rule against the rules it touches. Also glossary vs rule.
**Fix:** reconcile in the rule that owns the constraint; add the exception explicitly ("any Branch
takes back any Item (LND-R7)" inside the scope rule LND-R1).

## KI-4 — A Rules line misses a rule that governs the entry  [script + review]

**Spot it:** a rule that names an action or view, what it does ("renewing", "one at a time") or its
object, while that entry's *Rules* line doesn't list it. Orphan rules (no entry points to them) are
listed by the script — check each is intended.
**Fix:** add the rule to every entry it constrains. Keep background rules and shared contract
rules off the line (see KI-19): fixing this issue must not cause that one.
For example, LND-R15 says lending a *Ready* Hold's Item makes the Hold *Collected*, so LND-A1 cites
it although it sits in the Holds group.

## KI-5 — A source rule dropped or only partly carried  [review]

**Spot it:** compare the change against its source (old spec, PO answers) clause by clause; after
any glossary or rule clean-up, check each removed sentence's meaning still lives somewhere.
**Fix:** restore it as a rule, or record it as deliberately dropped where the team tracks that.
Limits, separation of duties and "recorded explicitly, never inferred" clauses are the usual losses.

## KI-6 — A permission outside *Who can*  [script]

**Spot it:** a role name, "authorized user" or an authority next to *can / may / only / by a* in a
rule, a definition or an entry's description.
**Fix:** put it on the action's or view's *Who can*; keep the rule to what happens.
"An authorized user can reverse a Fine" in a rule means nobody can, since no entry lists them.

## KI-7 — An agreed position still marked ❓  [review]

**Spot it:** for every ❓ marker, look in the sources you were given for an answer, an agreed
simplification or a working position.
**Fix:** write the position as the rule; drop the marker if the PO clarified it, otherwise keep it
with "pending …".

## KI-8 — Stale references after a rename or renumber  [script]

**Spot it:** IDs in the spec folder's other files (open questions, READMEs) or the project's index
files that the spec no longer defines; unprefixed IDs (`A3`, `R12`) left from an older format;
READMEs describing old conventions.
**Fix:** update them in the same change, or list them in the report if they're outside its scope.

## KI-9 — The same fact said twice  [review]

**Spot it:** the same constraint in Context, a definition and a rule; a rule restating another
("(LND-R20)" spelled out again inside LND-R4's text); a scenario step restating a rule.
**Fix:** keep it in the rule that owns it; elsewhere, cite or cut.

## KI-10 — An inference stated as fact  [review]

**Spot it:** a rule, *Who can* or value list with no source you were given — including values taken
from a prototype or the current system.
**Fix:** add `*[❓ Needs PO clarification — …]*` saying what it was inferred from. A business call
of ours keeps its ❓; a mechanics call (list values, status moves, validation) the team may decide
itself needs none.

## KI-11 — A bundled or missing entry  [review]

**Spot it:** an entry that is really two decisions (request *and* decide a waiver); something a
person does or looks up that has no entry ("handle a late return" — action, view or rule?);
a stage with actions but no views.
**Fix:** split by decision, actor or rules; add the missing action or view; move what the system
does itself to a rule.
**Also:** a view that doesn't return what its name promises — "Find a Member's Loans" without each
Loan's status. Two views may return the same fact; a rule keeps them agreeing.

## KI-12 — *Who can* missing or naming the wrong role  [script + review]

**Spot it:** a Librarian listed without the Branch Manager (a Branch Manager does everything a
Librarian does); a role added that the sources don't give; an action with no entry, so nobody can
do it.
**Fix:** list every role allowed; mark an inferred one. The script checks the `role_implies` pairs
in the project config.

## KI-13 — UI or editorial wording  [script]

**Spot it:** screens, buttons, tabs, queues, "shown", "displayed"; naming instructions ("Not called
…", "reserved for …").
**Fix:** say what the domain does; leave the presentation to the code and scenarios.

## KI-14 — Vague or broader-than-the-source wording  [review]

**Spot it:** a rule nobody could test ("a reasonable time", "a suitable Item"); a matching rule
joined by *or* that catches far more than intended; wording wider than the source's.
**Fix:** use defined terms and say exactly when it applies ("seven days after it becomes *Ready*").
**Also:** a rule written as the mechanism or in the source's jargon rather than as an outcome someone
can observe — "all Branches read one shared policy store" instead of "a Member gets the same answer
at every Branch". Copying the source's wording is faithful, not precise.

## KI-15 — A closed list that drifts  [script + review]

**Spot it:** "only LND-R7 and LND-R9 …", or a rule enumerating conditions or actions that other
rules also define (what refuses a loan).
**Fix:** "for example …", or point to the owning rule instead of copying its list, and check every
listed action cites it.

## KI-16 — A derived value with no complete rule  [review]

**Spot it:** a value the system works out (a Loan's status, an outstanding total) — check every
value it can take is produced by some condition, and that the conditions together cover every case.
**Fix:** one rule mapping conditions to values, as LND-R3 does for Loan status.

## KI-17 — Fields out of step  [script]

**Spot it:** an *Input* or *Returns* name that matches no field or concept; two fields with the same
name; a field nothing takes or returns; requiredness or validation restated outside its place.
**Fix:** use the name exactly as the Fields table writes it; keep names unique within the spec.

## KI-18 — A heading used twice  [script]

**Spot it:** the same `###` heading in two sections (a use-case group and a rule group both called
*Holds*). The Contents link for the second reaches the first.
**Fix:** rename one — use-case groups after the stage (*Holding a Title*), rule groups after the
topic (*Holds*).

## KI-19 — A Rules line padded with general rules  [script + review]

**Spot it:** the same rules repeat on most entries, or one line lists more than 12 ids. Often
these are background rules that no entry enforces ("time alone never closes a Loan") or parts of
the shared action contract (checks, records, all-or-nothing, no silent overwrite), added one review
at a time while fixing KI-4.
**Fix:** drop background rules from the line, since they may stand alone. Name the contract's
parts in one umbrella rule and cite only that rule. Keep what is specific to the entry.

## KI-20 — A list item, or a "where required" clause, with nothing behind it  [review]

**Spot it:** a rule, table row or field that lists what is checked, recorded, assessed or included,
where an item isn't defined there and doesn't point to the rule, field or named specification that
defines it ("checks the Member's standing", "applicable charges"). Also any "where required",
"where material", "configured", "specified", "according to policy" or "a fixed list" that never
leads to a written list — or leads back to itself (a waiver needed "where a waiver applies"). Two
items that mean the same thing, and an item a sibling rule implies but the list misses (refusal
reasons vs the checks that cause them), are the same defect.
**Fix:** make the list a table — item, defined in, outcome — as LND-R4 does, or point each item to
its definition. Where nothing defines it: ❓ if it's the product owner's call, otherwise the
precision backlog — a list the team keeps (for example at the end of the spec folder's README) and
works through as each build slice comes up. Fix contradictions at once; don't rewrite the whole spec
for the rest.

## KI-21 — A state word or date used loosely  [review]

**Spot it:** prose that borrows a status or a defined term for a different set of states, or none:
lowercase "open", "closed", "settled", "final", "active", "approved" next to an entity whose status
list has an *Active* or *Paid* with a narrower meaning ("a Member's active Loans" — only *Active*, or
also *Overdue*?). The same adjective with two senses ("late" for a late return and for a late
collection). A date field whose name doesn't say what its passing does (a Due date that only makes a
Loan *Overdue*, next to a Collect by that ends a Hold).
**Fix:** in prose, either use the exact status value in italics or name the set it covers ("Loans
that are *Active* or *Overdue*") — define the set once and point to it. Give each defined adjective
one meaning across the spec. Every date field has a rule saying what happens when it passes, and a
name that says so. A reviewer finds these by searching the spec for each entity's status values and
reading every lowercase use.

## KI-22 — A group that doesn't match its contents  [script + review]

**Spot it:** a `###` heading that promises more than its group holds while other groups hold the rest
("Ending a Loan" holding returns, with declaring an Item lost elsewhere); a group mixing a specific
action with tools every action uses; a rule group mixing topics (scope rules, the action contract
and Hold rules); a group of one; rule groups in a different order from the use-case groups.
**Fix:** name each group for everything in it ("Lending, renewing and ending a Loan"); move shared
tools to their own group; fold a group of one into its neighbour; order the rule groups like the
use-case groups, shared rules first. Moving entries changes no ID.

## KI-23 — A circular definition  [script + review]

**Spot it:** a glossary term defined by the entry that uses it, while the entry describes itself
with the term — *Renewal* "a renewal made with LND-A3", and LND-A3 "make a renewal". A milder
form: two terms defined through each other (*Member*: "a holder of a Membership"; *Membership*: "what
makes someone a Member").
**Fix:** define the term by what it is ("extending a Loan's Due date while the Item stays with the
Member"), and let the entry name what it does.

## KI-24 — A rule cited where it doesn't apply  [review]

**Spot it:** a rule on a Rules line that someone building only that entry wouldn't need — a Fine
rule on a view that returns nothing about Fines; a rule whose point another cited rule already
covers. Usually left behind when a rule moved or an entry's scope changed.
**Fix:** ask of each id: would someone building only this entry get something wrong without it? If
not, drop it — and if that leaves the rule cited nowhere, find the entry it does shape.
