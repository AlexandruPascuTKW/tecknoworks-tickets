# ADR-008: Event images are stored in the Events context's tables, with fixed formats and size

**Status:** Proposed

**Date:** 2026-11-04

**Deciders:** Alexandru Pascu (proposed); to be accepted by the development team

**Technical Story:** [`architecture.md` — Events](../architecture.md#events)

**Raised by:** Events — EVT-F6 (Image: "a picture, in the formats and up to the size engineering
sets"), EVT-A2, EVT-A3, EVT-R9 (an Event needs an Image to be published), EVT-V4 and EVT-V5 (the
public sees it).

---

## Context

Every Event has an Image before it can be published (EVT-R9), shown on "What's on" and on the event
page. The spec leaves formats and size to engineering. The image has to be stored somewhere and
served to anyone, signed in or not.

**Constraints.** No file store or CDN, by the foundation's rules. One SQLite file. `ApiController`
already serves an `IRequest<Stream>` result with 30-day cache headers.

---

## Decision

**We have decided to store each Event's image as bytes in an `EventImage` aggregate in the Events
context, accept JPEG, PNG and WebP up to 2 MB, and serve it from a public endpoint with long-lived
caching.**

- **Formats:** JPEG, PNG, WebP, recognised from the file's first bytes, not from its name or the
  browser's stated type. **Maximum 2 MB.** Both are in `EventsSettings`, read by the validator.
- **Storage:** an `EventImage` aggregate (content type + bytes) in its own table, so loading an Event
  never loads its picture. The Event holds the image's id. Changing the picture (EVT-A3) stores a
  new `EventImage` and points the Event at it, and the old one is deleted.
- **Serving:** `WhatsOnController.Image/{id}` (anonymous) sends `EventImageQuery` and returns the
  stream through `ApiController`'s cached `Send(IRequest<Stream>)`. A new picture gets a new id, so
  a cached old one is never shown for it.
- **Who may see it:** an image whose Event is Draft is served only to that Organizer's Team (EVT-R1).

---

## Rationale

1. **No infrastructure added,** and the image is backed up and moved with the rest of the data.
2. **The image belongs with its Event.** It sits inside the Events context and leaves with it if
   Events is ever split out.
3. **2 MB is plenty for a poster-sized web image** and keeps the database small: four pilots with a
   few dozen Events each is well under 1 GB.
4. **Checking the first bytes** stops a renamed file or a script being stored and served as a
   picture.

---

## Alternatives Considered

### Alternative 1: Files on the server's disk

**Pros:** keeps binaries out of the database; simple to serve.
**Cons:** a second thing to back up and keep in step with the database; breaks if the app runs on
more than one machine.
**Why Rejected:** splits one piece of data across two stores for little gain at pilot scale.

### Alternative 2: A blob store or CDN

**Pros:** the right answer at scale; offloads serving.
**Cons:** new infrastructure, which the foundation rules out.
**Why Rejected:** out of scope; worth revisiting with reporting or real traffic.

---

## Consequences

### Positive Consequences

- ✅ Images work locally with nothing extra to run.
- ✅ Browsers cache them; changing a picture is never served stale.

### Negative Consequences

- ⚠️ **The database grows with images,** and SQLite's single writer handles uploads too. Fine for the
  pilots; move to a blob store if image traffic grows.
- ⚠️ **No resizing.** The browser scales a 2 MB image down for the list. Add thumbnails if "What's
  on" feels slow.

### Neutral/Unknown Consequences

- 🤷 Organizers may want more than one image per Event later; the separate aggregate makes that
  straightforward.

---

## Notes

- Related: ADR-006 (storage choices on SQLite).

---

*Last Updated: 2026-11-04 by Alexandru Pascu*
