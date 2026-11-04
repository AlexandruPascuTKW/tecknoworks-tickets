---
name: sdlc-adr-writer
description: >
  Generate Architecture Decision Records (ADRs) to document significant technical
  decisions with context, rationale, and consequences. Use when: making major
  architectural choices, evaluating technology alternatives, or explaining
  design decisions to stakeholders.
---

# Architecture Decision Record Writer

> **Purpose:** Document technical decisions with context and rationale for future reference.

> **Career Alignment:** L3+ (Technical Depth category)

---

## When to Use This Skill

Write an ADR when making decisions about:
- **Technology selection:** Frameworks, databases, cloud providers
- **Architecture patterns:** Monolith vs. microservices, CQRS, event-driven
- **Infrastructure:** Deployment strategy, scaling approach
- **Security:** Authentication mechanism, encryption approach
- **Integration:** API design, third-party service selection

**Rule of Thumb:** If the decision is hard to reverse or affects multiple teams, write an ADR.

**Trigger phrases:**
- "Document this architectural decision"
- "Write ADR for [decision]"
- "Why did we choose [technology]?"

---

## ADR Template (Standard Format)

```markdown
# ADR-[Number]: [Short Title of Decision]

**Status:** Proposed / Accepted / Deprecated / Superseded by [ADR-XXX]

**Date:** YYYY-MM-DD

**Deciders:** [Names of people involved in decision]

**Technical Story:** [Link to work item, PR, or issue if applicable]

---

## Context

[Describe the situation and problem that requires a decision]

**What prompted this decision?**
[The triggering event or requirement]

**What constraints exist?**
[Technical, business, timeline, or resource constraints]

**What are the requirements?**
[Functional and non-functional requirements influencing the decision]

**Example:**
"We need to choose a database for the Patient Portal project. The system
must store patient records, test results, appointments, and messages for
50,000 active patients with expected growth to 100,000 in 2 years.

Requirements:
- ACID transactions (financial and medical data integrity)
- Complex relational queries (patients → test results → providers)
- JSON support (flexible provider notes, EHR data)
- HIPAA compliance (encryption at rest/transit, audit logging)
- Cost-conscious (fixed budget, cloud hosting)

Constraints:
- Must be Azure-hosted (client requirement)
- Team has SQL expertise but limited NoSQL experience
- Timeline: Must decide by end of Week 1 to start development"

---

## Decision

[The decision that was made]

**We have decided to [chosen option].**

[1-2 paragraphs explaining the decision clearly]

**Example:**
"We have decided to use Azure Database for PostgreSQL as the primary data store."

---

## Rationale

[Why this decision makes sense]

**Factors favoring this decision:**
1. [Reason 1 with explanation]
2. [Reason 2 with explanation]
3. [Reason 3 with explanation]

**Example:**
1. **ACID compliance:** PostgreSQL provides strong transactional guarantees
   critical for medical and financial data integrity.

2. **JSON support:** Native JSONB type allows flexible storage of EHR data
   without schema migrations for every Epic field.

3. **Cost:** Azure Database for PostgreSQL is 40-50% cheaper than Azure SQL
   Database for equivalent performance ($800/month vs. $1,400/month).

4. **Team expertise:** Team has 5 years combined PostgreSQL experience vs.
   1 year with NoSQL alternatives.

5. **Query complexity:** Relational model fits our multi-table joins better
   than document stores (patient → test results → providers).

---

## Alternatives Considered

### Alternative 1: [Option Name]

**Pros:**
- [Advantage 1]
- [Advantage 2]

**Cons:**
- [Disadvantage 1]
- [Disadvantage 2]

**Why Rejected:**
[Specific reason this wasn't chosen]

**Example:**

### Alternative 1: Azure SQL Database (MSSQL)

**Pros:**
- Team very familiar (used in 80% of TKW projects)
- Excellent tooling (SSMS, Azure Data Studio)
- Native Azure integration

**Cons:**
- 40-50% more expensive than PostgreSQL
- Less efficient JSON handling (JSON functions added late)
- Licensing complexity for client handoff

**Why Rejected:**
Cost difference ($600/month over 3 years = $21,600) outweighs familiarity benefit.
PostgreSQL's JSONB is superior for EHR data storage.

---

### Alternative 2: Azure Cosmos DB (NoSQL)

**Pros:**
- Highly scalable (global distribution)
- Flexible schema (no migrations)
- Fast for document-based queries

**Cons:**
- Team has limited Cosmos DB experience (1.3x dev time multiplier)
- Poor fit for relational data (patient-testresult-provider relationships)
- 2-3x more expensive at our scale
- Overkill for 50K-100K users (Cosmos DB shines at millions of users)

**Why Rejected:**
Relational model is natural fit for our data. Cosmos DB's strengths (global distribution,
massive scale) don't match our requirements. Cost and learning curve not justified.

---

### Alternative 3: MongoDB (Self-Hosted)

**Pros:**
- Flexible schema
- Horizontal scaling built-in
- Free (self-hosted)

**Cons:**
- Self-hosting burden (backups, monitoring, patching)
- Poor fit for relational data
- HIPAA compliance complexity (self-managed)
- No native Azure-managed service

**Why Rejected:**
Self-hosting MongoDB for HIPAA compliance requires significant DevOps effort
(backup encryption, audit logging, access controls). Azure-managed PostgreSQL
provides this out-of-the-box. Relational model is better fit for our data.

---

## Consequences

[Positive and negative outcomes of this decision]

### Positive Consequences

- ✅ [Benefit 1]
- ✅ [Benefit 2]

### Negative Consequences

- ⚠️ [Tradeoff 1]
- ⚠️ [Tradeoff 2]

### Neutral/Unknown Consequences

- 🤷 [Uncertain outcome that we'll monitor]

**Example:**

### Positive Consequences
- ✅ **Cost savings:** $600/month saved vs. Azure SQL (ROI: 36% budget savings)
- ✅ **JSON flexibility:** Can adapt to Epic schema changes without migrations
- ✅ **Performance:** PostgreSQL handles complex joins efficiently (tested in POC)
- ✅ **Open source:** No vendor lock-in, can migrate to self-hosted if needed

### Negative Consequences
- ⚠️ **Learning curve:** 2 engineers need to learn PostgreSQL-specific features (JSON operators, full-text search)
  - Mitigation: 40 hours training budget, pair programming
- ⚠️ **Tooling gap:** No SSMS equivalent (Azure Data Studio works but less familiar)
  - Mitigation: Team comfortable with pgAdmin and Azure portal query editor

### Neutral/Unknown
- 🤷 **Future scaling:** Unknown if PostgreSQL will scale to 500K users in 5 years
  - Monitoring: Track query performance, revisit if p95 latency >500ms

---

## Notes

[Additional information, links, or context]

- [Related ADRs]
- [Implementation notes]
- [Review date]

**Example:**
- POC results: [Link to performance testing report]
- Cost analysis spreadsheet: [Link]
- Review this decision after 6 months of production usage
- Related: ADR-012 (Caching Strategy) depends on this choice

---

*Last Updated: YYYY-MM-DD by [Name]*
```

---

## ADR Writing Workflow

### Step 1: Recognize ADR-Worthy Decision (5 minutes)

**Ask:** "Is this decision:"
- [ ] Hard to reverse? (Changing later would require significant rework)
- [ ] Affects multiple teams/components? (Ripple effects across codebase)
- [ ] Costs money? (Licensing, infrastructure, training)
- [ ] Controversial? (Team has different opinions)
- [ ] Impacts future work? (Sets precedent for similar decisions)

**If YES to 2+:** Write an ADR.

---

### Step 2: Research Alternatives (1-2 hours)

**For each viable alternative:**
1. **List pros and cons** (be honest, include negatives)
2. **Estimate cost** (licensing, hosting, training)
3. **Assess team expertise** (1.0x familiar, 1.3x learning curve)
4. **Test if possible** (POC, spike, prototype)
5. **Gather data** (benchmarks, case studies, vendor documentation)

**Minimum:** Consider 2-3 alternatives (shows due diligence)

---

### Step 3: Make Decision (30 minutes)

**Decision-Making Framework:**

```
1. Does it meet requirements? (Eliminate non-viable options)
   ↓
2. What's the cost difference? (TCO over 3 years)
   ↓
3. What's the team expertise gap? (Learning curve)
   ↓
4. What are the long-term consequences? (Lock-in, scalability)
   ↓
5. What's the risk if we're wrong? (Can we change later?)
   ↓
Decision: [Chosen option]
```

**Get Consensus:**
- Present options to team
- Discuss pros/cons
- Make decision (Tech Lead/Architect has final say)
- Document dissenting opinions if significant

---

### Step 4: Write ADR (30-60 minutes)

**Follow template structure:**
1. **Context:** Set up the problem (why decide now?)
2. **Decision:** State clearly what was decided
3. **Rationale:** Explain why (3-5 key reasons)
4. **Alternatives:** Show what else was considered (2-3 options)
5. **Consequences:** Be honest about tradeoffs

**Writing Tips:**
- Be specific (not "better performance" but "p95 latency 200ms vs. 500ms")
- Include numbers (costs, metrics, timelines)
- Link to evidence (POC results, benchmarks, documentation)
- Future-proof (explain context so future readers understand)

---

### Step 5: Review & Approve (15-30 minutes)

**Review Checklist:**
- [ ] Context explains why decision was needed
- [ ] Decision is stated clearly (no ambiguity)
- [ ] Rationale includes 3+ solid reasons
- [ ] At least 2 alternatives documented
- [ ] Consequences include both positive and negative
- [ ] Specific, not vague ("cost-effective" → "$600/month savings")

**Approval Process:**
- Tech Lead reviews technical soundness
- Practice Lead reviews if cross-squad impact
- Document approval in ADR (Status: Proposed → Accepted)

---

### Step 6: Store & Communicate (10 minutes)

**Storage:**
```
project/
└── docs/
    └── adr/
        ├── 001-use-postgresql.md
        ├── 002-choose-react-framework.md
        ├── 003-caching-strategy.md
        └── README.md  # Index of all ADRs
```

**Communicate:**
- Share ADR in team Slack channel
- Link from relevant documentation (README, TDD)
- Reference in code comments where decision is implemented
- Present in tech team meeting if major decision

---

## Common ADR Mistakes

| Mistake | Why It's Bad | Better Approach |
|---------|-------------|-----------------|
| **No alternatives** | Looks like decision wasn't considered carefully | Document at least 2 alternatives |
| **Vague rationale** | Future readers can't understand why | Be specific with data and metrics |
| **Only positives** | Dishonest, every decision has tradeoffs | Include negative consequences |
| **Too late** | Written after implementation | Write when decision is made, not after |
| **No status** | Unclear if decision is final | Always include status (Proposed/Accepted/etc.) |
| **Too detailed** | 20-page ADR nobody reads | Keep to 1-2 pages, link to details |

---

## ADR Lifecycle

```
Status: Proposed
   ↓
Status: Accepted (Decision approved, implementation proceeds)
   ↓
[Time passes, requirements change]
   ↓
Status: Deprecated (No longer recommended)
   ↓
Status: Superseded by ADR-XXX (New decision replaces this)
```

**When to update status:**
- **Deprecated:** Technology no longer recommended (but still in use somewhere)
- **Superseded:** New decision explicitly replaces this one (link to new ADR)

**Don't delete old ADRs:** They provide historical context ("why did we do it that way back then?")

---

## Career Development Notes

**For L3 Engineers:**
- Write ADRs for feature-level architecture decisions
- Present ADRs to team for review and discussion
- Learn to justify decisions with data

**For L4 Engineers:**
- Write ADRs for system-level architecture
- Review ADRs from team members
- Use ADRs to build practice-wide standards

---

## Examples

See `assets/` folder for:
- `adr-example-database-choice.md` - Example: PostgreSQL vs. MSSQL
- `adr-example-frontend-framework.md` - Example: React vs. Angular
- `adr-example-caching.md` - Example: Redis caching strategy

---

## Related Skills

- **tdd-generator:** ADRs document decisions made in TDD
- **architecture-review:** Review ADRs as part of architecture assessment

---

## External Resources

- **ADR GitHub Org:** https://adr.github.io/
- **Michael Nygard's ADR Template:** http://thinkrelevance.com/blog/2011/11/15/documenting-architecture-decisions

---

*Document decisions while context is fresh. Future you (and future engineers) will thank you.*
