# Scenarios: Lending

> Spec: [`specs/LND/lending.md`](../../specs/LND/lending.md) · Last reviewed: 2026-10-14

## Contents

- [Test data](#test-data)
- [LND-A2 — Return an Item](#lnd-a2--return-an-item)
  - [SC-LND-001 — Spine: an Item is lent, renewed and returned at another Branch](#sc-lnd-001--spine-an-item-is-lent-renewed-and-returned-at-another-branch) 🟢
  - [SC-LND-002 — A returned Item is set aside for the first Member waiting for it](#sc-lnd-002--a-returned-item-is-set-aside-for-the-first-member-waiting-for-it) 🟢
- [LND-A3 — Renew a Loan](#lnd-a3--renew-a-loan)
  - [SC-LND-003 — A Loan is not renewed while another Member is waiting for its Title](#sc-lnd-003--a-loan-is-not-renewed-while-another-member-is-waiting-for-its-title) 🟢
- [LND-A7 — Waive a Fine](#lnd-a7--waive-a-fine)
  - [SC-LND-004 — A Librarian cannot waive a Fine](#sc-lnd-004--a-librarian-cannot-waive-a-fine) 🔴

## Test data

Scenarios refer to data by the part it plays, not by name — any environment with this shape will
do. Where a step says "any", the choice does not affect the outcome.

| Placeholder | What it must be |
|-------------|-----------------|
| **Branch North**, **Branch South** | Two Branches |
| **Librarian 1** | A Librarian at Branch North |
| **Librarian 2** | A Librarian at Branch South |
| **Member 1** | A Member in good standing, with no Loans, Holds or Fines |
| **Item 1** | A book on the shelf at Branch North, whose Title has no Holds |
| **Item 2** | A book on an *Active* Loan from Branch North to any Member other than Member 2 and Member 3 |
| **Member 2**, **Member 3** | Members each with a *Waiting* Hold on Item 2's Title, both collecting at Branch North; Member 2's placed before Member 3's, and no other Holds on that Title |
| **Item 3**, **Member 4** | Item 3 is a book on an *Active* Loan to Member 4 from Branch North, renewed fewer than twice, whose Title has a *Waiting* Hold by any other Member |
| **Member 5**, **Fine 1** | Member 5 owes one *Outstanding* Fine, Fine 1, raised on a Loan from Branch North |

---

## LND-A2 — Return an Item

### SC-LND-001 — Spine: an Item is lent, renewed and returned at another Branch

- **Covers:** LND-R6, LND-R7
- **Use cases:** LND-A2, LND-A1, LND-A3, LND-V1
- **Status:** 🟢 Built
- **Setup:** sign in as Librarian 1, at Branch North
- **PO Feedback:** none yet

1. On the "Loan desk", enter Member 1's card number in "Member card" and click "Find member"
   - → Member 1 is shown, with "No outstanding fines"
2. Scan Item 1 into "Item barcode"
3. Click "Lend"
   - → "Item lent", with a Due date of today plus the Loan period for books
4. Click the "Loans" tab
   - → Item 1 is listed as "Active", lent today at Branch North
5. Click "Renew" on Item 1
   - → "Loan renewed"
   - → Item 1 shows "Renewals: 1 of 2"
6. Sign out, and sign in as Librarian 2, at Branch South
7. On the "Loan desk", scan Item 1 into "Return item" and click "Check in"
   - → "Item returned"
8. Sign out, and sign in as Member 1
9. Open "My loans"
   - → Item 1 is listed as "Returned", returned today

### SC-LND-002 — A returned Item is set aside for the first Member waiting for it

- **Covers:** LND-R9, LND-R14
- **Use cases:** LND-A2
- **Status:** 🟢 Built
- **Setup:** sign in as Librarian 1, at Branch North
- **PO Feedback:** none yet

1. On the "Loan desk", scan Item 2 into "Return item" and click "Check in"
   - → "Item returned"
   - → "Set aside for hold", naming Member 2, with the Hold shown as "Ready"
   - → Member 3 is not named

---

## LND-A3 — Renew a Loan

### SC-LND-003 — A Loan is not renewed while another Member is waiting for its Title

- **Covers:** LND-R10
- **Use cases:** LND-A3, LND-V1
- **Status:** 🟢 Built
- **Setup:** sign in as Librarian 1, at Branch North
- **PO Feedback:** none yet

1. On the "Loan desk", enter Member 4's card number in "Member card" and click "Find member"
2. Click the "Loans" tab
   - → Item 3 is listed as "Active"; note its Due date and Renewals
3. Click "Renew" on Item 3
   - → "Renewal refused: another member is waiting for this title"
   - → Item 3's Due date and Renewals are unchanged

---

## LND-A7 — Waive a Fine

### SC-LND-004 — A Librarian cannot waive a Fine

- **Covers:** LND-A7
- **Use cases:** LND-A7, LND-V3
- **Status:** 🔴 Not built
- **Setup:** sign in as Librarian 1, at Branch North
- **PO Feedback:** none yet

1. On the "Loan desk", enter Member 5's card number in "Member card" and click "Find member"
2. Click the "Fines" tab
   - → Fine 1 is listed as "Outstanding"
   - → no "Waive fine" control is offered
3. Try to waive Fine 1 as Librarian 1 another way than the screen — a saved link, or a direct
   request to the system
   - → refused
   - → Fine 1 is still "Outstanding"
