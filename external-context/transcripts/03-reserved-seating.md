# Transcript — Increment 3 Kickoff: Reserved Seating

**Project:** TecknoworksTickets
**Channel:** Planning call (recorded), #tecknoworkstickets-planning
**Participants:**
- PO
- Development team (Dev 1–4)

---

**PO** [2:00 PM]
Hi all. Scanning is live and the front-of-house lead has stopped sending me photos of wet paper. Big thanks. Today is the one we've been dodging: seats.

**Dev 1** [2:01 PM]
Here we go.

**PO** [2:02 PM]
The Lantern's autumn season moves to the Riverside Playhouse, which is 620 numbered seats. And Northgate has a new room, Harbour Hall, which is a standing floor plus a seated balcony. So we need both, in the same venue.

**Dev 2** [2:03 PM]
So a venue stops being one capacity number. It's made of sections, and each section is either seated or standing.

**PO** [2:04 PM]
Exactly. Riverside: Stalls, Circle, Balcony, all seated. Harbour Hall: Floor, standing, 900; Balcony, seated, 240. The Tram Depot stays one standing section of 450.

**Dev 4** [2:05 PM]
And a seated section is rows and seats. Row letters, seat numbers?

**PO** [2:06 PM]
Usually. Riverside's Circle has rows AA to HH because someone in 1890 thought that was funny. Don't assume letters are single.

**Dev 3** [2:06 PM]
Is there a seat 13?

**PO** [2:07 PM]
In Riverside, no. Theatres are superstitious. Whatever the plan says is what exists.

**Dev 2** [2:08 PM]
Who draws the map? I don't see a theatre administrator drawing 620 seats in a browser.

**PO** [2:09 PM]
For the pilot, we do it. A platform admin builds the seating map from the venue's plan and attaches it to the organizer's venue. Organizers can't edit maps. A self-service map builder is a someday thing.

**Dev 1** [2:10 PM]
Once a performance is on sale, can the map change?

**PO** [2:11 PM]
The map, no. But organizers need to take seats off sale for a performance. The finance manager calls them "kills" — a sound desk in row P, a camera position, a seat that's broken. Per performance, not on the map, because the camera is only there on press night.

**Dev 2** [2:11 PM]
So the map is the building and it doesn't change. What's free on Friday — held, sold, killed — lives with the performance, not the venue. Which raises one: can they kill a seat that's already sold?

**PO** [2:13 PM]
…I mean, the broken seat case, yes, you'd have to move the person. I don't know what that looks like. Let me get back to you on that.

**Dev 1** [2:14 PM]
What about prices? In GA it's ticket types with a quantity.

**PO** [2:16 PM]
Prices by section. Stalls €38, Circle €30, Balcony €22. And then concessions on top: concession Stalls is €32 and so on. You had a picture?

**Dev 2** [2:17 PM]
A grid. Ticket types down the side — Standard, Concession — sections across the top, a price in each cell. A blank cell means that type isn't sold in that section. For a standing section the cell also has a quantity, like today. For a seated section there's no quantity — the seats are the quantity.

**PO** [2:18 PM]
Yes, that. That's what the Lantern's old system looked like.

**Dev 4** [2:19 PM]
So a seated section has 200 seats and two ticket types. How many concessions can be sold? All 200?

**PO** [2:20 PM]
Today, yes, any seat can be bought at either price. The Lantern would like to limit concessions to, say, 30 per performance. Not now. Backlog it.

**Dev 3** [2:21 PM]
Buying. The attendee picks their seats on the map?

**PO** [2:22 PM]
Yes. They click seats, pick a ticket type for each — some Standard, some Concession — and go to checkout. Same hold, same ten minutes, same limit of eight. "Best available" where we pick for you is nice, but later.

**Dev 2** [2:23 PM]
The hold now holds those exact seats.

**PO** [2:23 PM]
Those exact seats.

**Dev 4** [2:24 PM]
Two people clicking the same seat at the same moment — whoever gets the hold first wins, second sees "just taken". How we make that safe is ours.

**Dev 1** [2:25 PM]
Ours. Same for how the map is drawn — SVG, canvas, whatever performs on a phone with 620 seats.

**Dev 3** [2:26 PM]
Seat colours: available, held by someone else, sold, killed. Do attendees see the difference between held and sold?

**PO** [2:27 PM]
No, both are just "unavailable". Killed seats too. Don't tell people a seat is in someone's basket.

**Dev 2** [2:28 PM]
Gaps. If someone picks seats 4 and 5 and leaves seat 3 alone between seat 2 sold and seat 4, that's an orphan seat nobody will buy. A lot of theatre systems block that.

**PO** [2:29 PM]
The Lantern hates single-seat gaps, I know that much. Whether we block it or warn or let it go — I'll ask them. Question.

**Dev 4** [2:30 PM]
Wheelchair spaces. Riverside has eight in the Stalls.

**PO** [2:31 PM]
They're on the map as wheelchair spaces, flagged. Anyone can book them; we don't ask for proof. The front-of-house lead has views on this, it's in his memo.

**Dev 3** [2:32 PM]
Restricted view? The Balcony side seats have a pillar.

**PO** [2:33 PM]
Flag them on the map with a note, "restricted view". Whether they're cheaper — usually yes in real life. Would that be another ticket type, or a different price on the seat? I don't know. Put it on the list.

**Dev 2** [2:34 PM]
GA sections in a mixed venue — Harbour Hall Floor — work exactly like now? Quantity per ticket type, no seat?

**PO** [2:34 PM]
Exactly like now. An attendee can buy Floor and Balcony in the same order if they want, it's one performance.

**Dev 1** [2:35 PM]
And the capacity rule from increment 1 — ticket quantities can't exceed capacity — becomes per section.

**PO** [2:36 PM]
Per standing section. Seated sections can't oversell by construction.

**Dev 4** [2:37 PM]
Scanner: does anything change?

**PO** [2:38 PM]
Show the seat on the green screen. "Stalls, Row F, Seat 12." Ushers will love it. And on "My tickets" and the door list, obviously.

**Dev 3** [2:39 PM]
On the seat picker, one thing: with guests checking out without an account now, I can't use the account to remember which seats they were looking at if they refresh. Is local storage okay for that?

**PO** [2:40 PM]
Sure, whatever works. And yes — since last week you can buy with just an email, like the founder wanted. The tickets show up in My tickets if you register later with that email.

**Dev 3** [2:40 PM]
Cool.

**Dev 2** [2:41 PM]
Can I bring up the 8-ticket limit? With seats, groups of ten for a birthday want to sit together. Two orders can't guarantee adjacent seats.

**PO** [2:42 PM]
They can pick ten seats across two orders in two tabs — no wait, one hold per performance. Okay, that's annoying. But I'm not raising the limit for seats only. Groups ring the box office. Leave it.

**Dev 1** [2:43 PM]
Changing a performance from GA to seated, or attaching a map to an event that's already on sale?

**PO** [2:44 PM]
No. A performance's layout is fixed when it goes on sale. If you want seats, set it up seated from the start.

**Dev 4** [2:45 PM]
Existing GA performances at the Tram Depot — they become "one standing section" with no visible change?

**PO** [2:45 PM]
Right. Nobody should notice.

**Dev 2** [2:46 PM]
Sorry, I have to drop at 3:00 for the dentist, can we wrap the read-back?

**Dev 1** [2:48 PM]
Reading back:

**Dev 1** [2:50 PM]
- **Venue layout:** a venue is made of sections; each is seated (rows, seats, names as on the venue plan) or standing (capacity). Seating maps are built by a platform admin from the venue plan; organizers can't edit them. Existing GA venues become one standing section.
- **Per-performance kills:** organizers take seats off sale per performance. Killing an already sold seat: open.
- **Pricing:** a grid of ticket types × sections, a price per cell; a blank cell means not sold there. Standing cells carry a quantity; seated cells don't. Capacity check now per standing section. Concession caps per section: backlog.
- **Buying:** pick seats on the map, a ticket type per seat; mixed standing + seated in one order is fine. Same 10-minute hold on the exact seats, same 8-ticket limit. Held, sold and killed all show as "unavailable". Best available: later. Concurrency and map rendering: ours.
- **Seat flags:** wheelchair space (anyone can book, no proof), restricted view (with a note). Restricted-view pricing: open.
- **Single-seat gaps:** open — the PO to ask the Lantern.
- **Layout fixed at on-sale:** no switching GA ↔ seated once a performance is on sale.
- **Seat shown** on the scanner's green screen, "My tickets" and the door list.

**PO** [2:51 PM]
Yes. Go get your teeth done.

---

## Open questions carried forward (not resolved in this conversation)

- What happens when a seat that's already sold has to be killed.
- Single-seat gaps: block, warn, or allow — the PO to ask the Lantern.
- Restricted-view seats: a separate ticket type or a different price.
- Concession limits per performance — backlog.
- Seat-hold concurrency and seat-map rendering — engineering's call.
