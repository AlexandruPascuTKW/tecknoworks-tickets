# Transcript — Increment 1 Kickoff: First On-Sale

**Project:** TecknoworksTickets
**Channel:** Planning call (recorded), #tecknoworkstickets-planning
**Participants:**
- PO
- Development team (Dev 1–3)
- Finance Manager

---

**PO** [10:00 AM]
Okay, I think everyone's in. Thanks for joining, I know you're buried in pilot contracts this week. Quick context for the devs: TecknoworksTickets lets an organizer — a theatre company, a promoter, a conference team — put an event on sale at a venue, and lets people buy tickets and get in at the door. We have four pilot organizers signed: the Lantern Theatre Company, Northgate Live, DevHarbour Conference and the Saltmarsh Folk Club.

**PO** [10:01 AM]
For this first slice I want the smallest thing that is actually a ticket platform: an organizer sets up a show, puts tickets on sale, an attendee buys them and pays. No seat maps. Every pilot venue we start with is standing or unreserved, so general admission only.

**Dev 1** [10:02 AM]
Good, seating is a monster, happy to defer it. Let's start with what the organizer sets up. What's an "event" for you?

**PO** [10:04 AM]
The show. "Hamlet", or "Northgate presents Ana Lux". It has a name, a description, an image, a venue. And then it has dates. The Lantern runs a play for three weeks, so "Hamlet" is one event with, I don't know, eighteen performances.

**Finance Manager** [10:05 AM]
Can I jump in on words? In the box office a "performance" is one specific date and time. "Hamlet, Friday 14th, 19:30" is a performance. When I reconcile I reconcile per performance, never per event. The conference people say "day one, day two" but it's the same thing to me.

**Dev 2** [10:06 AM]
So event is the show, performance is a dated occurrence of it. Does the first slice need several performances per event, or can we do one?

**PO** [10:07 AM]
Several. If the Lantern can't put a run on sale, they won't use it. Northgate will mostly have one performance per event, that's fine, it's the same model.

**Dev 3** [10:08 AM]
What does a performance hold? Date, start time…

**PO** [10:09 AM]
Date, start time, and a doors time. Doors is when people can start coming in. Everything is local time at the venue, all pilots are in the same country, don't overthink time zones.

**Dev 1** [10:10 AM]
And the venue. Who creates venues?

**PO** [10:11 AM]
The organizer, for now. A venue is a name, an address and a capacity. General admission only, so capacity is one number: the Old Tram Depot is 450 standing.

**Dev 2** [10:12 AM]
Two organizers could use the same venue though. Northgate and the folk club both put shows on at the Tram Depot. Is that one venue record they share, or each keeps their own?

**PO** [10:13 AM]
Hm. I genuinely don't know. Let me think about it.

**Dev 1** [10:14 AM]
Noted as open. Now tickets. What does the organizer sell?

**PO** [10:15 AM]
Ticket types. "Standard €28", "Concession €22", "Early bird €20". Each with a name, a price and how many there are.

**Finance Manager** [10:16 AM]
We call them price levels, but "ticket type" is fine, I'll get used to it. The thing I care about: the allocations can't add up to more than the capacity. If the Tram Depot is 450, I cannot sell 300 standard and 200 concession. That's how you end up with a fire officer at the door.

**Dev 2** [10:17 AM]
Is that per performance? If Hamlet has eighteen performances, does each one have its own 450?

**Finance Manager** [10:18 AM]
Each performance is its own house, yes. Selling out Friday has nothing to do with Saturday.

**Dev 2** [10:19 AM]
Then do organizers set the ticket types up once for the event, or for each performance?

**PO** [10:20 AM]
Once for the event, please. Nobody's typing "Standard €28" eighteen times. The quantities apply per performance. The matinees might be cheaper one day, but not now.

**Dev 1** [10:21 AM]
Price rules? Minimum, decimals?

**PO** [10:22 AM]
Euros, two decimals, more than zero. Free tickets — the folk club does free nights sometimes — park that, not now.

**Dev 3** [10:23 AM]
And when does it go on sale? Created means visible?

**PO** [10:24 AM]
No. An event is a draft until someone publishes it. Published means attendees can see it and buy. Before that, only the organizer's team sees it.

**Dev 1** [10:25 AM]
Can they edit after publishing? Change a price, move a date?

**PO** [10:26 AM]
Ugh. Probably yes for some things, no for others. Changing the date of a performance people already bought for is a big deal. Let me come back with a list.

**Dev 1** [10:27 AM]
Okay. Accounts. Who are the people?

**PO** [10:29 AM]
On the organizer side: an organizer is an organisation — "Lantern Theatre Company" — with a team. Two roles for now. **Owner**, who runs the account and adds people, and **Manager**, who sets up events and ticket types. We onboard organizers ourselves: a platform admin creates the organisation and its first Owner after we've signed the contract. No self-service sign-up for organizers.

**Dev 2** [10:30 AM]
Platform admin is us, the TecknoworksTickets team?

**PO** [10:30 AM]
Us. Two or three people. They can see every organizer, but they shouldn't be selling tickets or editing events on someone's behalf.

**Dev 3** [10:31 AM]
And attendees?

**PO** [10:32 AM]
Attendees register themselves: name, email, password. Email is the login. You need an account to buy, because that's where your tickets live. Anyone can browse published events without one.

**Dev 2** [10:33 AM]
Can a person be on an organizer team and an attendee? The Lantern's manager probably goes to Northgate gigs.

**PO** [10:34 AM]
Sure, why not. Same person, same email. I don't want to force two accounts.

**Dev 3** [10:35 AM]
Password rules, session length?

**PO** [10:35 AM]
Yours. Something sensible.

**Dev 1** [10:36 AM]
Right, the buying part. Walk us through it.

**PO** [10:38 AM]
Attendee opens a performance, picks how many of each ticket type, goes to checkout, pays by card, gets the tickets. The important bit: while they're paying, those tickets are held for them. Nobody else can grab them. If they wander off, the hold runs out and the tickets go back on sale.

**Finance Manager** [10:39 AM]
Ten minutes. That's what the old box office system did and nobody complained. For a big on-sale, Northgate's Ana Lux show, you want them moving through fast.

**Dev 2** [10:40 AM]
Ten-minute hold, then automatic release. If they're still on the payment page at minute eleven and the card goes through — what happens?

**PO** [10:41 AM]
Oh. Good question. I don't know. Write it down.

**Dev 1** [10:42 AM]
How we release expired holds — a sweeper, or checking expiry at read time — that's ours, we'll decide. The rule is just "after ten minutes they're available again".

**Dev 3** [10:43 AM]
Can someone hold tickets in two tabs? Start checkout for Friday, then again for Friday in another tab?

**PO** [10:44 AM]
I'd say no but I haven't thought about it. Flag it.

**Dev 1** [10:45 AM]
Limits on how many someone can buy?

**PO** [10:46 AM]
Eight per order, across all ticket types. Stops the obvious touting.

**Finance Manager** [10:47 AM]
Eight is low for school groups but they ring me anyway.

**Dev 2** [10:48 AM]
Per order — so somebody could place two orders of eight?

**PO** [10:48 AM]
Mm. I meant per order. I think. Put it on the list.

**Dev 1** [10:49 AM]
What's on an order?

**PO** [10:51 AM]
Who bought it, which performance, the ticket lines — type, quantity, price at the time — the booking fee, the total, a status, when it was placed. One performance per order. If you want Friday and Saturday, that's two orders.

**Finance Manager** [10:52 AM]
The booking fee is €1.50 per ticket, paid by the attendee on top of the face value. Face value goes to the organizer, the fee is ours. I've got a tiered fee in my policy doc but forget the tiers for launch — flat €1.50.

**Dev 3** [10:53 AM]
So the page shows face value, fee and total separately?

**Finance Manager** [10:54 AM]
Must. It's a legal thing, the fee has to be visible before they pay, not sprung on them at the end. Whether it shows VAT separately I'd have to ask our accountant.

**Dev 2** [10:55 AM]
Order statuses. I'm thinking: Pending payment while the hold is live, Confirmed when payment succeeds, and Expired if the hold runs out without payment.

**PO** [10:56 AM]
Fine. And a declined card doesn't kill it — they can retry while the hold is still running.

**Dev 1** [10:57 AM]
Card payments — we'd use a hosted payment page from a provider, so we never see card numbers. Which provider is engineering's call, I'll write it up.

**PO** [10:58 AM]
Please. And refunds — not in this slice. If something goes wrong, the finance manager does it by hand in the provider's dashboard.

**Finance Manager** [10:58 AM]
As long as I have a list of what was sold per performance, I can live with that for a month.

**Dev 3** [11:00 AM]
What does the attendee get after paying? A PDF?

**PO** [11:01 AM]
A "My tickets" page. Each ticket has its own code — one code per person getting in, so an order of four is four codes. No scanning yet, that's the next increment. For the pilot, the doors will work off a printed list.

**Finance Manager** [11:02 AM]
Which means I need that list: per performance, every confirmed ticket with its code, ticket type and the buyer's name. Printable. The front-of-house lead's team ticks them off at the door.

**Dev 2** [11:03 AM]
Which front-of-house lead?

**Finance Manager** [11:03 AM]
Front of house lead at the Lantern. He's written a memo on how they run doors, I'll share it, it'll matter next time.

**Dev 3** [11:04 AM]
Should the event page show how many are left? "Only 12 left!"

**PO** [11:05 AM]
Marketing would love it, organizers might hate it. I don't know. Show "Sold out" when it is, obviously. The rest — question.

**Dev 3** [11:06 AM]
Checkout — separate page or a drawer over the event page?

**PO** [11:07 AM]
Separate page, with the countdown timer visible at the top. People panic if they can't see the time.

**Dev 3** [11:07 AM]
Big red countdown, got it. I'll do it tasteful.

**Finance Manager** [11:08 AM]
Red is fine. Red sells.

**Dev 1** [11:09 AM]
One more on sold out. Tickets in someone's live hold — sold out or not?

**PO** [11:10 AM]
If everything's either sold or held, it's sold out. If holds expire it comes back. Right?

**Finance Manager** [11:10 AM]
That's how it works. People refresh like mad at a big on-sale, they'll see it come back.

**Dev 1** [11:12 AM]
Okay, reading back.

**Dev 1** [11:14 AM]
- **Organizers:** an organisation with a team. Roles: Owner (manages the team), Manager (sets up events and ticket types). Organisations and their first Owner are created by a platform admin after contract; no self-service sign-up. Platform admins can see all organizers but don't sell or edit events.
- **Attendees:** self-register with name, email, password; email is the login. Account needed to buy, not to browse. One person can be on an organizer team and also be an attendee with the same email. Password rules and session length: ours.
- **Venue:** created by the organizer; name, address, capacity (general admission only).
- **Event:** name, description, image, venue; one or more **performances** (date, start time, doors time, venue local time). Draft until published; published events visible to everyone.
- **Ticket types:** name, price (euros, above zero, two decimals), quantity. Defined once per event; quantities apply to each performance separately. Sum of quantities must not exceed venue capacity. Free tickets: not now.
- **Holds:** starting checkout holds the chosen tickets for 10 minutes; expired holds release automatically (mechanism ours).
- **Order:** one performance; lines (type, quantity, price at the time), booking fee €1.50 per ticket, total, status, placed-at. Max 8 tickets per order across types. Statuses: Pending payment → Confirmed, or Expired. A declined card can be retried while the hold lasts.
- **Payment:** card via a hosted payment page (provider: ours). Fee shown separately before payment. No refunds in-app; manual.
- **Tickets:** one code per ticket, shown on "My tickets". Printable door list per performance for the organizer. No scanning yet.
- **Sold out:** when everything is sold or held.

**PO** [11:16 AM]
That's it. I owe you a few answers, I'll come back.

**Finance Manager** [11:16 AM]
And I'll ask the accountant about VAT on the fee. Can I go reconcile Hamlet's previews now?

**Dev 1** [11:17 AM]
Go. We'll draft specs and send the questions they raise.
