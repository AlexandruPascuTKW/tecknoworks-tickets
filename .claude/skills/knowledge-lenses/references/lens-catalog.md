# Lens catalog

A lens is a way of looking at the same model. Pick lenses by what the user is trying to do
and by what the material actually contains. A lens without the right shape of material behind
it is decoration.

## Built in (data in model.json, no code)

| Lens | Learning job | Fits when the source has… | Cost |
|---|---|---|---|
| `map` | Orient: see the whole shape | Parts that connect: flow, entities, dependencies | Default |
| `reference` | Look up, read carefully | Always | Default |
| `map` + `scenarios` | Reason about a what-if | An explicit open decision, proposal or alternative | Low |
| `states` | Understand moving parts | Entities that move between named statuses | Low |
| `walkthrough` | Learn by doing | A process with steps and decisions a newcomer could get wrong | Default: one typical, everyday case told from the business side |
| `questions` | Interrogate: what is not solid | Unresolved items, owners, contradictions, inferred claims | Low |
| `quiz` | Retain | Always (default): essentials, misconceptions, terms | Default |

## Custom (a JS file in `<topic>/lenses/`, see custom-lenses.md)

| Lens | Learning job | Fits when the source has… |
|---|---|---|
| Timeline | See how it got here | Dated decisions, versions, history, a workshop sequence |
| Per-role view | Communicate | Several roles or audiences who care about different parts |
| Version diff | Track change | Two versions of the same document or design |
| Decision matrix | Compare options | Several options rated against the same criteria |
| Glossary graph | Pin the vocabulary | Dense jargon, or near-synonyms that might or might not mean the same thing |
| Blueprint sheet | One-page overview to print or glance at | Mature material that will be referred to often |
| Spec vs reality | Check what exists | A spec plus an implementation or prototype to compare against |
| **Journey mockup** | See the product and follow a case with the spec beside it | A spec with use cases (actions/views) and a lifecycle with gates, often a prototype to borrow the look from. A separate page embedded as a tab. See mockup-lens.md |

## Outside the app

| Lens | How |
|---|---|
| Plain language | Already applied: model text is written about 80% of the way to ASD-STE100. If the user wants it stricter, rewrite the model text and rebuild. |
| Teach-back | In chat: the user explains a part, you compare it against the model and name the gaps. |
| Video explainer | Only on explicit request. See video.md. |

## Choosing what to recommend

Recommend two or three, never the whole list. Put first the lens that answers the question
the user is likely stuck on. A spec reader usually needs `states` and `questions`. Say in
one line why each fits *this* material, e.g. "Order, Payment and Shipment each have
their own status list — a state machine lens shows all three", not "states show states".
