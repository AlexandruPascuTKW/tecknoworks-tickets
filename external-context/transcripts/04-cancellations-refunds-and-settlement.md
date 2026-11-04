# Transcript — Increment 4 Kickoff: Cancellations, Refunds & Settlement

**Project:** TecknoworksTickets
**Channel:** Planning call (recorded), #tecknoworkstickets-planning
**Participants:**
- PO
- Development team (Dev 1, 2 and 4)
- Finance Manager

---

**PO** [11:00 AM]
Right, money. The finance manager has been doing every refund and every organizer settlement by hand for three months, and she has a spreadsheet macro called DOOM.

**Finance Manager** [11:01 AM]
It stands for something. I've forgotten what.

**PO** [11:02 AM]
Three things today. One, organizers cancelling a performance. Two, attendees asking for their money back. Three, paying organizers what they're owed. You start, you've earned it.

**Finance Manager** [11:03 AM]
First, housekeeping. The accountant came back on the booking fee. It's VAT-inclusive, and we show it as one amount, "€1.50 booking fee per ticket, incl. VAT". No separate VAT line for the attendee. I need the VAT split on my reports, not on their screen.

**Dev 1** [11:04 AM]
So that closes the increment 1 question. Good.

**Finance Manager** [11:05 AM]
Second, how money flows, so everyone uses the same words. Attendee pays face value plus booking fee. The booking fee is ours. From the face value we take a commission, four percent, and the rest is the organizer's. Card processing costs are ours too, we don't pass them on. What we pay the organizer for a performance — I call it the settlement.

**Dev 2** [11:06 AM]
We've been calling it a payout in the code.

**Finance Manager** [11:06 AM]
Call it whatever you want in the code. To an organizer it's a settlement and there's a settlement statement.

**PO** [11:07 AM]
Let's start with cancellations, because they drive everything else.

**PO** [11:09 AM]
An organizer can cancel a single performance — the actor's ill, Tuesday's off — or a whole event, which cancels all of its performances that haven't happened yet. Only the Owner can do either. Same reasoning as publishing.

**Dev 4** [11:10 AM]
And everyone who bought for that performance?

**PO** [11:11 AM]
Full refund, automatically, to the card they paid with. Face value and booking fee, everything. Their tickets stop working. And they get an email saying the performance is cancelled and the refund is on its way.

**Finance Manager** [11:12 AM]
The booking fee comes back to the attendee, but it's not us who should eat it. The organizer cancelled, so the fees we refunded are recharged to the organizer. They come off their next settlement.

**Dev 2** [11:13 AM]
And if they have no next settlement — a one-off show that got cancelled?

**Finance Manager** [11:14 AM]
Then I invoice them. Outside the app. Don't build me invoicing.

**Dev 4** [11:15 AM]
A cancelled performance in the scanner — the code shows red "no longer valid"?

**PO** [11:15 AM]
Red, and say why: "Performance cancelled". Saves the front-of-house lead an argument.

**Dev 1** [11:16 AM]
Can a cancelled performance be un-cancelled?

**PO** [11:16 AM]
No. Final. If it's back on, they create a new performance and people buy again.

**Dev 2** [11:17 AM]
What about postponing? You said last time moving a date is a whole process.

**PO** [11:18 AM]
Still a whole process. Not this round. If they need to move a date now, they cancel and create a new one.

**PO** [11:20 AM]
Okay, attendee refunds. Each event gets a refund policy, picked by the organizer when they set it up. Two options for now: "No refunds", or "Refunds up to 7 days before the performance". Default is no refunds, that's what most of them do today.

**Finance Manager** [11:21 AM]
Seven's fine by me. The organizers said a two-week cut-off was turning away people with perfectly good reasons, and a week is still enough time to resell the seats.

**Dev 4** [11:22 AM]
If the policy allows it, does the organizer approve each request, or is it automatic?

**PO** [11:23 AM]
Automatic. If you're inside the window, you click "Request refund", it's done. If you're outside or the policy is no refunds, the button isn't there.

**Dev 2** [11:24 AM]
Per ticket or per order?

**PO** [11:25 AM]
Per ticket. A dad bought four, one kid's sick — he refunds one.

**Finance Manager** [11:25 AM]
And on an attendee refund, the booking fee is not refunded. Face value only. That's industry standard.

**Dev 4** [11:26 AM]
Can I push on that? Two of the bigger platforms started refunding the fee last year. It's a sore point in reviews.

**Finance Manager** [11:28 AM]
I know, I've read the reviews. But the fee is what pays for the card processing on the original sale, and that cost doesn't come back when we refund. If we refund the fee on attendee requests we're paying to process a sale that made us nothing. When the organizer cancels, different story, they pay for it. So no.

**PO** [11:28 AM]
Keep it. Fee non-refundable on attendee requests.

**Dev 2** [11:29 AM]
And a refunded ticket goes back on sale?

**PO** [11:30 AM]
Yes, straight back. For seats, the same seat becomes available again. That's the whole point of the seven days.

**Dev 4** [11:31 AM]
Transferred tickets. The friend holds it, the dad paid. Who presses "Request refund"? The money goes back to the dad's card regardless.

**PO** [11:32 AM]
Hm. If the friend can refund it, the money goes to someone else. If only the dad can, the friend's ticket disappears from under them. I don't know. Let me get back to you on that.

**Dev 1** [11:33 AM]
Can an organizer change the refund policy after the event's on sale?

**PO** [11:34 AM]
Making it more generous, sure. Making it stricter for people who already bought, that feels wrong. Let me think about whether it's per-purchase or per-event. Question.

**Dev 2** [11:35 AM]
Organizer-initiated refund of one order, not a cancellation? A Manager refunding someone as a goodwill gesture?

**PO** [11:36 AM]
Yes, Owners and Managers can refund any ticket of a performance that hasn't happened, regardless of policy. Face value only, same as an attendee request — right?

**Finance Manager** [11:36 AM]
Same as attendee request. Face value. Goodwill is on them.

**Dev 1** [11:37 AM]
Refunds failing at the provider, retrying, not refunding twice — that's ours to make safe.

**PO** [11:37 AM]
Yours. Just never refund twice. The finance manager will find you.

**PO** [11:39 AM]
Last part, settlement. Over to finance.

**Finance Manager** [11:41 AM]
We settle weekly, on Mondays. A performance gets settled once it's five days in the past — that's our window for complaints and late refunds. The statement, per performance, has: tickets sold, gross face value, refunds, our commission, booking fees recharged if there was a cancellation, and the net. One bank transfer per organizer per week covering all their settled performances.

**Dev 4** [11:42 AM]
Does the bank transfer itself happen through us, or do you still send it?

**Finance Manager** [11:43 AM]
For now I still press the button in our bank. What I need from the app is the statements and a weekly list: organizer, IBAN, amount. If that's right, DOOM can retire.

**Dev 1** [11:44 AM]
Bank details. Where do they come from?

**PO** [11:45 AM]
The Owner enters their IBAN and account name in the organizer settings. Only the Owner sees and changes it.

**Finance Manager** [11:46 AM]
Changing bank details is the number one fraud trick in this industry. Someone takes over an account, changes the IBAN on Friday, Monday's money goes to them.

**PO** [11:47 AM]
So what do we do — hold it, confirm by phone, platform admin approves?

**Finance Manager** [11:47 AM]
I'd want a check of some kind. I don't know which. Let me think about what the bank recommends.

**PO** [11:48 AM]
Question, then.

**Dev 2** [11:49 AM]
Do organizers see their statements in the app?

**PO** [11:50 AM]
Yes. Owners and Managers see statements for their organisation, download as PDF. Door staff no.

**Dev 4** [11:51 AM]
And platform admins see all statements, and mark a weekly run as paid when you've pressed the button?

**Finance Manager** [11:51 AM]
Yes, please. "Paid on" with a date.

**Dev 2** [11:52 AM]
A refund after a performance is settled? Goodwill a week later?

**PO** [11:53 AM]
Once settled, no refunds through the app. The finance manager handles it.

**Finance Manager** [11:53 AM]
I'll grumble but yes.

**Dev 1** [11:54 AM]
Resale? People will ask, once refunds close at seven days.

**PO** [11:55 AM]
The founder wants a resale marketplace. Not now. Transfers cover friends.

**Dev 1** [11:56 AM]
Reading back.

**Dev 1** [11:58 AM]
- **Booking fee:** €1.50 per ticket, VAT-inclusive, shown as one amount to the attendee. VAT split on internal reports.
- **Money flow:** attendee pays face value + booking fee. Booking fee is the platform's. Platform takes 4% commission on face value; the rest is the organizer's. Card processing costs absorbed by the platform. What the organizer gets is the settlement.
- **Organizer cancellation:** Owner only; a single performance, or an event (all its future performances). Final. Every order is refunded in full automatically, face value and booking fee, to the original card; tickets become invalid; attendees emailed. Booking fees refunded are recharged to the organizer on the next settlement. Postponement: not this increment.
- **Scanner:** a cancelled performance's ticket shows red "Performance cancelled".
- **Refund policy per event:** "No refunds" (default) or "Refunds up to 7 days before the performance". Refunds inside the window are automatic, per ticket, face value only; booking fee kept (re-examined, kept). The ticket goes back on sale, same seat for seated.
- **Goodwill refund:** Owner or Manager, any ticket, regardless of policy, face value only, before the performance.
- **Open:** who can request a refund on a transferred ticket; changing the refund policy after on-sale.
- **Settlement:** weekly on Mondays for performances at least five days past. Statement per performance: tickets sold, gross face value, refunds, commission, recharged fees, net. One transfer per organizer per week; the transfer itself is manual outside the app. Weekly list of organizer, IBAN, amount. Platform admin marks a run paid with a date. No in-app refunds once a performance is settled.
- **Bank details:** Owner only enters and sees IBAN and account name. Protection on changes: open, the finance manager to propose.
- **Statements:** Owners and Managers see and download; door staff don't.
- **Refund retries / no double refunds:** ours.
- **Out of scope:** postponement, resale, invoicing organizers, automatic bank transfers.

**Finance Manager** [12:00 PM]
That's my whole job on one screen. Lovely.

**PO** [12:00 PM]
Thanks all. Specs first, then questions to me.

---

## Open questions carried forward (not resolved in this conversation)

- Who may request a refund on a transferred ticket — purchaser, holder, or both.
- Whether an organizer may make an event's refund policy stricter after tickets are sold.
- What check is applied when an Owner changes bank details — the finance manager to propose.
- Retry and idempotency of refunds at the payment provider — engineering's call.
- Postponement, resale marketplace, invoicing — out of scope.
