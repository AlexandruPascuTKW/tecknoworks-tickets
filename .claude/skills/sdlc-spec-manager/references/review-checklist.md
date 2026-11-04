# Review checklist

For "review / check / clean up / zoom out on" a spec. Check every item in `known-issues.md` first —
those are the defects that keep recurring; this checklist is the wider pass. Grade each file per
dimension as **strong · adequate · thin · broken**; a file's verdict is its worst dimension. Count
each defect once, in its main dimension. Report findings with the exact text and a concrete fix,
then ask before applying.

## Spec

| Dimension | Asks | Typical findings |
|-----------|------|------------------|
| **Completeness** | Are Context, Glossary, Use Cases and Domain Rules all there and carrying load? Is every needed rule stated, or marked undecided? | A rule plainly needed but silent; a rule that grants a permission no entry's *Who can* gives. *Not* a finding while the spec is being built: rules and glossary terms waiting for an action not yet written — list them as the queue for the next actions instead |
| **Altitude** | Is everything about what the domain *is*? | Database or column names, labels, screens, steps, routes, phases, open-questions sections |
| **Substance** | Does each line constrain or define something specific to this business? | Generic purpose statements; "rules" nobody could violate; definitions that restate the term |
| **Consistency** | Is each fact said once, and terms used verbatim? | **Rules or permissions inside glossary definitions** (the most common — but only phrases a developer would test; what makes a term what it is stays); the same fact in Context, Glossary and a rule; a permission repeated in a rule or definition; synonyms for a defined term; inconsistent ❓ marking |
| **Navigability** | Can a reader find things? | Ungrouped glossary; long flat rule list; Contents out of date; rule numbers out of order before first commit; ranges instead of explicit rule lists; a stage with actions but no views; a group whose name doesn't match its contents, or rule groups in a different order from the use-case groups |

## Always also

- **Action, view or rule:** every entry under Use Cases is something a person does
  (action) or looks up (view). Anything the system does by itself or derives is a rule — flag it.
  Flag an entry that blends kinds ("Handle a late return") and a read side with no views.
- **Who can against the authority matrix:** where one role can do everything another can (a Branch
  Manager does everything a Librarian does), check every entry lists both; check separation of
  duties (no one approves their own work — nobody waives their own Fine) is a rule; check no
  permission is inferred without being marked.
- **Buildable, not just faithful:** for every rule, row or field that lists checks, records,
  conditions or contents, ask whether a developer could build and test each item from the spec
  alone (KI-20). A fidelity review — "did we lose a clause?" — passes vague text unchanged.
- **Status words and dates:** search for each entity's status values and read every lowercase use;
  check each defined adjective keeps one meaning and each date field says what its passing does
  (KI-21).
- **Rules lines both ways:** an entry missing a rule that names it (KI-4), *and* a line
  padded with background or shared contract rules (KI-19). Never fix one by causing the
  other.
- **Closed lists that point at other rules** ("only LND-R7 and LND-R9 are recorded") go stale as
  rules are added — prefer "for example", or keep them complete.
- **Fields:** every field an action takes or a view returns is defined once under
  Fields, with its validation, in a group named after its concept; no *Required* column;
  Input and Returns lines use concept and field names (not IDs) spelled as the table spells
  them; validation is not restated in a rule or an action; no concept's fields are defined in
  a spec that doesn't own the concept.
- **Derived values** (like a Loan's status) need a rule saying which conditions produce which value,
  so no value is unreachable.
- **Blockers:** is each ⛔ really foundation-shaping — would a wrong answer force rework of the
  data model, access model or a whole workflow? And is any open point that would do so missing one?
- Run `scripts/check_refs.py` on the spec — undefined IDs or broken Contents links are findings.
- Check there are no links to other spec files, and — where the project has an audience rule — no
  citation of anything in `forbidden_paths` (see `spec-format.md`).
- Note, but don't fix unasked, anything outside the two files that the change makes stale — e.g.
  rule numbers cited elsewhere, or conventions in the project's spec index.
