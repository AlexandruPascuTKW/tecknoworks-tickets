# Transcript — Increment 2 Kickoff: Door Scanning & Ticket Transfers

**Project:** TecknoworksTickets
**Channel:** Planning call (recorded), #tecknoworkstickets-planning
**Participants:**
- PO
- Development team (Dev 1–4; Dev 4 joins this increment)
- Finance Manager (joined at 9:20)

---

**PO** [9:00 AM]
Morning. Welcome to our new dev, first planning call, don't worry, nobody understands the box office on day one. First on-sale went well: Hamlet sold 71% across the run, Northgate's Ana Lux sold out in 40 minutes. The printed door list worked, but barely. The front-of-house lead's team spent the first Ana Lux night flipping pages in the rain.

**Dev 1** [9:01 AM]
So: scanning.

**PO** [9:02 AM]
Scanning. And the second-biggest complaint: people can't give a ticket to a friend. Right now they forward the screenshot of the code, and then two people turn up with the same code.

**Dev 4** [9:03 AM]
Which is exactly what scanning would catch.

**PO** [9:03 AM]
Which is why they're in the same increment. Let's do scanning first.

**Dev 3** [9:04 AM]
What do door staff scan with? Their own phones?

**PO** [9:05 AM]
Their own phones, in the browser. No app to install, the Lantern has volunteers who change every week. The attendee shows the code from "My tickets" as a QR, the phone reads it.

**Dev 1** [9:06 AM]
Who's allowed to scan? Right now the team is Owner and Manager.

**PO** [9:07 AM]
New role: **Door staff**. An Owner adds them like anyone else. Door staff can scan for their organizer's performances and see the check-in count, and that's it. No events, no sales numbers, no attendee details beyond what's on the screen when they scan. Owners and Managers can scan too.

**Dev 2** [9:08 AM]
Volunteers changing every week means a lot of accounts. Fine?

**PO** [9:09 AM]
Fine. The Owner removes them afterwards. Or doesn't, and that's their problem.

**Dev 3** [9:10 AM]
What does the scanner show?

**PO** [9:12 AM]
Big green or big red. Green: ticket type and holder name, and it counts as checked in. Red, with the reason:
- already checked in — and when, and by whom,
- wrong performance — this is the big one for the Lantern, people turn up on Thursday with Friday's ticket,
- not a ticket we know,
- ticket no longer valid.

**Dev 4** [9:13 AM]
"No longer valid" being what — transferred away?

**PO** [9:13 AM]
Transferred away, for now. We'll get to that.

**Dev 3** [9:14 AM]
Sound on green? The old handheld scanners beep.

**PO** [9:14 AM]
A beep on green, a different one on red. And it should vibrate. Door staff don't look at the screen after the first hour.

**Dev 2** [9:15 AM]
Duplicates. Second scan of the same ticket at the same performance — always red?

**PO** [9:16 AM]
Always red. That's the whole point. No re-entry through the app: once you're in, you're in. If someone steps out to smoke, that's the venue's problem.

**Dev 4** [9:17 AM]
What if the door person scans the wrong QR — the person behind in the queue holds up their phone too early. Can they undo a check-in?

**PO** [9:18 AM]
Oh, that happens. I don't know who should be allowed to undo. Let me think.

**Dev 4** [9:18 AM]
Related: a red "already checked in" where the attendee swears it's their ticket. Can staff admit them anyway?

**PO** [9:19 AM]
Also a "let me think". In real life the front-of-house lead lets them in and sorts it out after. Whether the app should have an "admit anyway" button, I'm not sure. If yes, probably not for volunteers.

**Finance Manager** [9:20 AM]
Sorry, sorry, settlement run overran. Where are we?

**Dev 1** [9:20 AM]
Scanning. Duplicates are always red, no re-entry.

**Finance Manager** [9:21 AM]
The front-of-house lead's memo allows pass-outs at the Lantern.

**PO** [9:22 AM]
I know, I read it. Not in the app. They can stamp hands if they want to.

**Finance Manager** [9:22 AM]
Fair. What I need from scanning is the count. During the show and after: sold, checked in, no-shows, per performance. That's my end-of-night reconciliation, I do it on paper today.

**Dev 1** [9:23 AM]
Live count per performance, visible to Owner and Manager, and door staff see the checked-in number.

**Finance Manager** [9:23 AM]
And which door. The Tram Depot has two doors. If one door is checking in at half the speed I want to know.

**Dev 3** [9:24 AM]
So the door person picks their door when they open the scanner — "Door 1", "Door 2" — and every scan records it?

**Finance Manager** [9:24 AM]
Yes.

**Dev 2** [9:25 AM]
Are door names set up on the venue, or typed free by the scanner person?

**PO** [9:26 AM]
On the venue, I suppose. Otherwise we'll get "door 2", "Door2", "side door". I'll leave the details to you, but it should be a list.

**Dev 1** [9:27 AM]
When can scanning start? Doors time, start time?

**Finance Manager** [9:28 AM]
Doors time, normally. But for Ana Lux they started checking the queue against the list early so the doors flowed. The front-of-house lead's memo has times in it.

**PO** [9:29 AM]
I don't want to decide that on a call. Question.

**Dev 1** [9:30 AM]
The hard one: signal. The Tram Depot's front door is basically a concrete tunnel. If the phone can't reach us, what happens?

**PO** [9:31 AM]
The one thing I can't have is a queue of 400 people standing still because the wifi dropped. I'd rather risk letting a duplicate in than stop the door.

**Dev 1** [9:32 AM]
That's a real design choice — caching the valid codes on the device, syncing later, accepting duplicates across doors while offline. I'd like to take it away and write it up properly with the options and the risk for each.

**PO** [9:33 AM]
Do. Tell me the risk in plain words and I'll sign it.

**Dev 4** [9:34 AM]
And the code format itself — whatever goes in the QR — I assume is ours. Not guessable, obviously.

**Dev 1** [9:34 AM]
Ours.

**PO** [9:35 AM]
Okay, transfers.

**PO** [9:37 AM]
An attendee opens a ticket in "My tickets", taps "Transfer", enters the friend's name and email. The friend gets the ticket in their own "My tickets". The old code stops working, the friend gets a new one. So the screenshot trick dies.

**Dev 2** [9:38 AM]
The friend needs an account?

**PO** [9:38 AM]
Yes. Tickets live in an account. If they don't have one, they register with that email and the ticket is waiting.

**Dev 4** [9:39 AM]
Does the friend have to accept, or does it just land?

**PO** [9:40 AM]
Just lands. Accepting is another step for people to not do.

**Dev 2** [9:41 AM]
Who's the "customer" after a transfer? The order is still the original buyer's — they paid.

**Finance Manager** [9:42 AM]
The buyer is the buyer. The money relationship doesn't move. The friend is the ticket holder, that's all. In box office we say "purchaser" and "holder" and they're often different people — a dad buys four for his kids.

**PO** [9:42 AM]
Right. And the door list and the scanner show the holder's name, not the buyer's.

**Dev 3** [9:43 AM]
Can you transfer one ticket out of an order of four?

**PO** [9:43 AM]
Yes, ticket by ticket.

**Dev 4** [9:44 AM]
Until when?

**PO** [9:45 AM]
Until doors open for that performance. After that, no, otherwise people transfer in the queue and the door staff go crazy.

**Dev 2** [9:46 AM]
Can the friend transfer it on again?

**PO** [9:47 AM]
Hm. Touts would love that. But a friend who can't go either… I don't know. Question.

**Dev 4** [9:48 AM]
And can the original buyer take back a transfer — sent it to the wrong email?

**PO** [9:48 AM]
Also question. Put them together.

**Finance Manager** [9:49 AM]
Transfers are free, by the way. No fee. We've never charged for name changes.

**Dev 2** [9:50 AM]
And a ticket that's already checked in can't be transferred, I assume.

**PO** [9:50 AM]
Correct. Doors are open by then anyway, but yes.

**Dev 4** [9:52 AM]
Can I raise something from the hold numbers? I looked at the first on-sale. About 9% of expired holds expired while the person was on the payment page, mostly on mobile. Should the hold be 15 minutes instead of 10?

**PO** [9:53 AM]
What does finance say?

**Finance Manager** [9:54 AM]
I'd keep ten. On Ana Lux, the problem wasn't people timing out, it was tickets stuck in holds while 2,000 people refreshed. Longer holds mean more "sold out" that isn't sold out. And since we confirm late payments when the tickets are still there, most of those 9% got their tickets anyway, didn't they?

**Dev 4** [9:55 AM]
About two-thirds did, yes.

**PO** [9:55 AM]
Then ten stays. Make the countdown more visible on mobile instead.

**Dev 3** [9:56 AM]
It's already red. I'll make it bigger and red.

**Dev 1** [9:57 AM]
Scanner: full-screen page or inside the normal organizer dashboard?

**PO** [9:57 AM]
Full screen, nothing else on it. Dark background, it's used in dark doorways.

**Dev 1** [9:59 AM]
Reading back:

**Dev 1** [10:01 AM]
- **New team role, Door staff:** added by an Owner; can scan for the organizer's performances and see the checked-in count; nothing else. Owners and Managers can also scan.
- **Scanning:** in the phone browser, reading the QR of a ticket code. Door chosen from a list set up on the venue; every scan records performance, door, who, when. Green = valid, shows ticket type and holder name, ticket becomes checked in. Red reasons: already checked in (when, by whom), wrong performance, unknown code, ticket no longer valid.
- **Duplicates:** a second scan is always rejected. No re-entry in the app.
- **Counts:** per performance — sold, checked in, not yet arrived; per door. Owner and Manager see all; door staff see checked-in.
- **Open:** undo a mistaken check-in; an "admit anyway" override and who may use it; when scanning opens and closes.
- **Offline/poor signal:** The PO's priority is the queue keeps moving even at the risk of a duplicate; design is ours, with the options and risk written up for her sign-off. Code format: ours.
- **Transfers:** per ticket, from "My tickets", to a name and email; recipient needs an account (registers if they don't have one); lands immediately; old code void, new code issued. Allowed until doors open; not for a checked-in ticket. No fee. The buyer stays the purchaser on the order; the recipient is the holder. Door list and scanner show the holder.
- **Open:** onward transfer by the recipient; recalling a transfer.
- **Hold:** re-examined, stays at 10 minutes.

**PO** [10:02 AM]
Yes. Ship it before Hamlet's last week if you can, the front-of-house lead will kiss you.

**Dev 3** [10:02 AM]
Please tell him not to.

---

## Open questions carried forward (not resolved in this conversation)

- Undoing a mistaken check-in, and who may do it.
- Whether there is an "admit anyway" override on a red scan, and for which roles.
- When scanning opens and closes relative to doors and start time.
- Onward transfer by a recipient; recalling a transfer sent to the wrong person.
- Offline / poor-signal scanning — engineering to write up the options and risks for sign-off.
- Ticket code format — engineering's call.
