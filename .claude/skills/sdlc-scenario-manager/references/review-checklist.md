# Review checklist — scenarios

For "review / check / clean up" a scenario file. Grade it per dimension as **strong · adequate · thin
· broken**; its verdict is its worst dimension. Count each defect once, in its main dimension. Report
findings with the exact text and a concrete fix, then ask before applying.

## Dimensions

| Dimension | Asks | Typical findings |
|-----------|------|------------------|
| **Selection** | Is the main journey walked by one spine, and each behaviour a user or the business would notice proven once — and no more? | No spine; a refusal that protects money or data with no scenario; two scenarios that would fail for the same reason; scenarios for detail nobody would notice |
| **Altitude** | Steps and observable results only? | A step restating the rule; selectors or endpoints |
| **Substance** | Does `Covers` match what the steps actually prove? | Citing a rule the scenario never exercises; walking an action or view it doesn't list |
| **Executability** | Could a tester run it cold? | Vague verbs with no control; no actor in Setup; real names where a placeholder or "any" belongs; a refusal checked only by a hidden button |
| **Noise** | Anything that repeats or says nothing? | Coverage tables, findings tables, Priority / "Pending because" lines, verification paragraphs, empty README placeholders |

## Always also

- Run the spec manager's `check_refs.py` on the spec — every ID a scenario cites must exist.
  Without a spec it doesn't apply; instead read down the Covers lines and the acceptance criteria
  the file came from: every Covers line names a rule the source states (or is the ticket), each
  rule is worded the same way wherever it appears (SK-12), and every gap in the source is a ❓, not
  a guess.
- Run `scripts/check_labels.py` on the scenario file — every quoted label is found in the product,
  or is a known assumed label.
- Check rendering: header fields as a bullet list, results as `- →` sub-bullets.
