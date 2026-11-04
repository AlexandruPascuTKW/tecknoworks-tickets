# Transcript — Increment 1 Follow-up: Product Owner Answers Open Questions

**Project:** TecknoworksTickets
**Channel:** #tecknoworkstickets-planning (thread)
**Participants:**
- PO
- Development team (Dev 1 and 2)
- Finance Manager

*Follow-up to the increment 1 kickoff, after the team drafted specs and sent the PO the points they couldn't settle.*

---

**Dev 1** [2:30 PM]
Thanks both. Specs for increment 1 are drafted. Below are the open points and the assumptions we wrote in. A yes or a correction on each is all we need.

**Dev 1** [2:31 PM]
First, venues. Shared between organizers, or each keeps its own?

**PO** [2:33 PM]
Each organizer keeps its own. If Northgate and the folk club both create "Old Tram Depot", that's two records, fine. A shared venue directory is a nice idea for later, not now.

**Dev 1** [2:34 PM]
Second, editing after publishing. You said you'd send a list.

**PO** [2:37 PM]
Here it is:
- Name, description, image: any time.
- Ticket type price: can change, but only affects new purchases. People who bought keep what they paid.
- Ticket type quantity: can go up as long as the total stays within capacity, and down but not below what's already sold or held.
- A ticket type that has sales can't be deleted. It can be taken off sale.
- New performances can be added to a published event.
- The date or time of a performance that has sold tickets can't be edited. That's a postponement and it's a whole process. Not now.

**Dev 2** [2:38 PM]
Can a performance with no sales be deleted?

**PO** [2:38 PM]
Yes.

**Dev 2** [2:39 PM]
Third, payment arriving after the hold expired. We assumed: reject it and refund automatically, the attendee starts again.

**PO** [2:41 PM]
No, that's harsh. If the tickets are still there, honour it — confirm the order. Only if they've gone to someone else in the meantime do we refund automatically and tell them.

**Finance Manager** [2:42 PM]
Agreed. And I don't want to be the one doing those refunds by hand, so automatic is right for that case even if other refunds are manual for now.

**Dev 2** [2:43 PM]
"Still there" meaning the same ticket types and quantity are available?

**PO** [2:43 PM]
Same types, same quantity, yes. Don't swap anything.

**Dev 1** [2:44 PM]
Fourth, two holds at once. Our assumption: one live hold per attendee per performance. Starting a new checkout for the same performance replaces the old hold. A hold for a different performance is separate.

**PO** [2:45 PM]
Yes, exactly that.

**Dev 1** [2:45 PM]
Fifth, the 8-ticket limit. We kept it per order, as you said.

**PO** [2:46 PM]
Keep it per order. Northgate will want a per-person cap for the big shows eventually, but per order is what we launch with.

**Dev 1** [2:47 PM]
Sixth, remaining counts. Our assumption: attendees see no numbers, just "Sold out" when nothing's left.

**PO** [2:48 PM]
Yes. No "only 12 left". I asked two organizers and both said no.

**Dev 1** [2:49 PM]
Now the assumptions. Both Owner and Manager can publish an event.

**PO** [2:50 PM]
No — only the Owner publishes. Managers build the drafts, the Owner presses the button. Publishing is the moment money starts moving and the Lantern was very clear they want one person accountable for that.

**Dev 2** [2:51 PM]
And editing a published event, per your list — Managers can do that?

**PO** [2:51 PM]
Yes, Managers can edit. Just not publish.

**Dev 1** [2:52 PM]
Ticket type names are unique within an event.

**PO** [2:52 PM]
Yes.

**Dev 1** [2:53 PM]
An Owner can add Owners and Managers, and remove them. An organisation always keeps at least one Owner.

**PO** [2:53 PM]
Yes. We've had partnerships where two people co-own, so more than one Owner is fine.

**Dev 2** [2:54 PM]
Expired orders: they stay visible to the attendee in an order history, marked Expired, but don't appear on "My tickets".

**PO** [2:55 PM]
Fine.

**Dev 1** [2:55 PM]
The door list shows confirmed tickets only: code, ticket type, buyer name. No email, no phone.

**Finance Manager** [2:56 PM]
That's what the front-of-house lead wants. Sorted by surname, please, they search by surname at the door.

**Dev 1** [2:57 PM]
Last: VAT on the booking fee?

**Finance Manager** [2:58 PM]
Still waiting. Our accountant is away until the 3rd. Show the fee as a single €1.50 per ticket for now and I'll come back on whether it needs splitting out. Please don't hardcode anything that can't change.

**Dev 1** [2:59 PM]
Recap of what changed from our drafts:
- Venues are per organizer, not shared.
- Edit rules after publishing, per the PO's list.
- Payment after hold expiry: confirm if the same tickets are still available, otherwise automatic refund.
- Only the Owner publishes; Managers draft and edit.
- Door list sorted by surname.
- Everything else we assumed is confirmed.

**PO** [3:00 PM]
Matches. Go.

---

## Still open after this conversation

- VAT treatment of the booking fee — the finance manager waiting on the accountant.
- Postponing a performance with sales — deliberately not in this slice.
- Per-person ticket caps and a shared venue directory — later.
