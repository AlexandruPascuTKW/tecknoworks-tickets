# Known issues

The defects reviews of scenario files keep finding, most frequent first. Review mode checks every
one. Add an issue, with the next free SK number, when a review finds the same kind of defect a
second time. The `SK-` prefix keeps these apart from project IDs and from the spec manager's `KI-`
issues.

## SK-1. Citing what the steps don't prove

**Spot it:** for each ID on *Covers* or *Use cases*, find the step or `→` result that proves it. An
action or view whose control is never used, or a rule whose outcome is never asserted, is false coverage —
for example a scenario citing LND-R10's renewal limit on a Loan that has never been renewed.
**Fix:** add the step or assertion, or drop the ID.

## SK-2. Proving what it doesn't cite

**Spot it:** a step that uses an action's or view's control ("Renew", the "Loans" tab) or asserts a
rule's outcome ("no Fine is raised"), with that ID missing from the header.
**Fix:** add it to *Use cases* or *Covers* — or drop the assertion if it isn't what the scenario is
for.

## SK-3. Noise

**Spot it:** anything other than the header, Test data and steps: coverage or findings tables,
Priority or "pending because" lines, results that explain ("because", "never") instead of observing.
**Fix:** delete; contradictions between the product and the spec go in the report or pull request,
not the file.

## SK-4. A step with no control or no observable result

**Spot it:** an action with no quoted label; a result with nothing to see ("is told", "works"); a
scenario with intent but no steps.
**Fix:** name the control and the visible text or state. Assumed labels are allowed where no product
exists yet — say so in the report.

## SK-5. Test data that lets the outcome happen by chance

**Spot it:** "any …" where some values would change the result — any Loan, when an *Overdue* one is
refused a Renewal for a different reason; any Member, when one over the Fine limit can't borrow.
**Fix:** constrain it ("renewed fewer than twice", "with no Fines").

## SK-6. Missing or contradicting what the rule requires

**Spot it:** read each cited rule and check every observable part this scenario's situation reaches
is asserted (who, when, the resulting status), and that nothing contradicts a rule that has changed.
**Fix:** add the assertion or update the step.

## SK-7. Scenarios that don't earn their place

**Spot it:** two scenarios that would fail for the same reason; a short scenario that repeats part of
the spine; a scenario per rule; a scenario for detail no user or the business would notice (a format,
a range, a message's wording).
**Fix:** keep the spine and the behaviours someone would notice; merge or drop the rest. A rule
without a scenario is a decision, not a gap.

## SK-8. Real names or ID-like placeholders

**Spot it:** real people, branches, customers or records; placeholders like "Member R2" or
"Item LND-4" that read as IDs.
**Fix:** role placeholders defined in Test data (Member 1, Branch North); "any" where the choice
doesn't matter.

## SK-9. No scenario for the main journey or a behaviour someone would notice

**Spot it:** no spine; a separation of duties, a fair queue, something recorded, or a rule about
money that no scenario proves.
**Fix:** add the spine, or one scenario for the behaviour — with an "another way" step if it's a
refusal.

## SK-10. Several actions in one step

**Spot it:** a step with more than two actions ("enter …, enter …, then click …").
**Fix:** split it. "Enter a card number and click Find" is fine as one step.

## SK-11. Test data shared and changed by another scenario

**Spot it:** a record used by two scenarios where one of them changes it, so the other's expected
result depends on run order — an Item lent in one scenario and expected on the shelf in another.
**Fix:** give each scenario that changes a record its own record in Test data.

## SK-12. One rule stated two ways (without a spec)

**Spot it:** in a file written without a spec, two scenarios whose Covers lines or results state
the same rule differently — "a Member with an unpaid Fine can't borrow" in one, "a Member owing
over €5 can't borrow" in another. With no spec to settle it, nobody can tell which is right.
**Fix:** ask the product owner which wording holds, then use it in both — or mark it ❓ until they
answer.
