# Scenarios: Accounts

> Spec: [`specs/ACC/accounts.md`](../../specs/ACC/accounts.md) · Last reviewed: 2026-11-04

## Contents

- [Test data](#test-data)
- [ACC-A4 — Add a Team member](#acc-a4--add-a-team-member)
  - [SC-ACC-001 — Spine: an Organizer is onboarded with its first Owner, who signs in and adds a Manager](#sc-acc-001--spine-an-organizer-is-onboarded-with-its-first-owner-who-signs-in-and-adds-a-manager) 🔴
  - [SC-ACC-002 — Someone who registered as an Attendee joins a Team with the same Account](#sc-acc-002--someone-who-registered-as-an-attendee-joins-a-team-with-the-same-account) 🔴
  - [SC-ACC-003 — A Manager cannot add, remove or change the Team role of anyone on the Team](#sc-acc-003--a-manager-cannot-add-remove-or-change-the-team-role-of-anyone-on-the-team) 🔴
- [ACC-A5 — Remove a Team member](#acc-a5--remove-a-team-member)
  - [SC-ACC-004 — A removed Team member keeps their Account and Tickets but can no longer work for the Organizer](#sc-acc-004--a-removed-team-member-keeps-their-account-and-tickets-but-can-no-longer-work-for-the-organizer) 🔴
  - [SC-ACC-005 — The last Owner is never taken off the Team or made a Manager](#sc-acc-005--the-last-owner-is-never-taken-off-the-team-or-made-a-manager) 🔴

## Test data

Scenarios refer to data by the part it plays, not by name — any environment with this shape will
do. Where a step says "any", the choice does not affect the outcome.

| Placeholder | What it must be |
|-------------|-----------------|
| **Platform admin 1** | A Platform admin |
| **Person 1**, **Email address 1** | Someone with no Account; Email address 1 is theirs and no Account uses it |
| **Person 2**, **Email address 2** | Someone with no Account; Email address 2 is theirs and no Account uses it |
| **Visitor 1**, **Email address 3** | Someone with no Account; Email address 3 is theirs and no Account uses it |
| **Organizer 1**, **Owner 1** | An Organizer, with Owner 1 an Owner on its Team |
| **Manager 1**, **Manager 2** | Managers on Organizer 1's Team, and on no other Team |
| **Manager 3** | A Manager on Organizer 1's Team and on no other Team, who holds a Ticket they bought for any Performance |
| **Organizer 2**, **Owner 2**, **Manager 4** | Another Organizer, whose Team is Owner 2, its only Owner, and Manager 4 |

---

## ACC-A4 — Add a Team member

### SC-ACC-001 — Spine: an Organizer is onboarded with its first Owner, who signs in and adds a Manager

- **Covers:** ACC-R3, ACC-R4, ACC-R5
- **Use cases:** ACC-A4, ACC-A3, ACC-V1, ACC-A2, ACC-V2
- **Status:** 🔴 Not built
- **Setup:** sign in as Platform admin 1
- **PO Feedback:** none yet

1. Open "Organizers" and click "Onboard organizer"
2. Enter any name in "Organizer name"
3. Enter Person 1's name in "Name" and Email address 1 in "Email address"
4. Click "Onboard"
   - → "Organizer onboarded"
   - → "Organizers" lists the new Organizer, with Person 1 on its Team as "Owner"
5. Sign out
6. Open the invitation sent to Email address 1 and click "Choose your password"
7. Enter any valid password in "Password" and click "Set password"
   - → "Password set"
8. On "Sign in", enter Email address 1 and the password just chosen, and click "Sign in"
   - → signed in as Person 1, with the new Organizer's "Team" offered
9. Open "Team" and click "Add team member"
10. Enter Email address 2 in "Email address" and Person 2's name in "Name"
11. Choose "Manager" in "Team role" and click "Add"
    - → "Team member added"
    - → "Team" lists Person 1 as "Owner" and Person 2 as "Manager"
    - → an invitation to choose a password arrives at Email address 2

### SC-ACC-002 — Someone who registered as an Attendee joins a Team with the same Account

- **Covers:** ACC-R2, ACC-R5
- **Use cases:** ACC-A4, ACC-A1, ACC-A2, ACC-V2
- **Status:** 🔴 Not built
- **Setup:** not signed in
- **PO Feedback:** none yet

1. Click "Register"
2. Enter Visitor 1's name in "Name" and Email address 3 in "Email address"
3. Enter any valid password in "Password" and click "Create account"
   - → signed in as Visitor 1
4. Sign out, and sign in as Owner 1
5. Open "Team" and click "Add team member"
6. Enter Email address 3 in "Email address"
7. Choose "Manager" in "Team role" and click "Add"
   - → "Team member added", showing Visitor 1's name as they registered it
   - → no invitation to choose a password arrives at Email address 3
8. Sign out, and sign in with Email address 3 and the password Visitor 1 chose in step 3
   - → signed in as Visitor 1, with Organizer 1's "Team" offered
9. Open "Team"
   - → Visitor 1 is listed once, as "Manager"
10. Sign out and click "Register"
11. Fill in the form with any name, Email address 3 and any valid password, and click "Create account"
    - → refused: "An account already uses this email address"

### SC-ACC-003 — A Manager cannot add, remove or change the Team role of anyone on the Team

- **Covers:** ACC-A4, ACC-A5, ACC-A6
- **Use cases:** ACC-A4, ACC-V2, ACC-A5, ACC-A6
- **Status:** 🔴 Not built
- **Setup:** sign in as Manager 1
- **PO Feedback:** none yet

1. Open "Team"
   - → Owner 1 is listed as "Owner" and Manager 2 as "Manager"
   - → no "Add team member", "Remove" or "Change role" control is offered
2. Try, as Manager 1, to add any Email address to Organizer 1's Team, to remove Manager 2, and to
   make Manager 2 an Owner, another way than the screen — a saved link, or a direct request to the
   system
   - → each is refused
3. Reload "Team"
   - → Manager 2 is still listed as "Manager", and no one new is listed

---

## ACC-A5 — Remove a Team member

### SC-ACC-004 — A removed Team member keeps their Account and Tickets but can no longer work for the Organizer

- **Covers:** ACC-R1
- **Use cases:** ACC-A5, ACC-V2, ACC-A2
- **Status:** 🔴 Not built
- **Setup:** sign in as Owner 1
- **PO Feedback:** none yet

1. Open "Team" and click "Remove" on Manager 3
   - → "Team member removed"
   - → Manager 3 is no longer listed
2. Sign out, and sign in as Manager 3
   - → signed in, with none of Organizer 1's pages offered
3. Open "My tickets"
   - → the Ticket Manager 3 bought is still listed
4. Try to open Organizer 1's "Team" as Manager 3 another way than the screen — a saved link, or a
   direct request to the system
   - → refused

### SC-ACC-005 — The last Owner is never taken off the Team or made a Manager

- **Covers:** ACC-R6
- **Use cases:** ACC-A5, ACC-A6, ACC-V2
- **Status:** 🔴 Not built
- **Setup:** sign in as Owner 2
- **PO Feedback:** none yet

1. Open "Team" and click "Remove" on Owner 2
   - → refused: "An organizer must keep at least one owner"
2. Click "Change role" on Owner 2 and choose "Manager"
   - → refused: "An organizer must keep at least one owner"
3. Try, as Owner 2, to remove Owner 2 and to make Owner 2 a Manager another way than the screen — a
   saved link, or a direct request to the system
   - → both are refused
4. Reload "Team"
   - → Owner 2 is still listed as "Owner", and Manager 4 as "Manager"
