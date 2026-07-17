# Decision Framework — Choosing the Right Level of Rigor

> *This document helps you decide which requirements-gathering practices to apply. The underlying principles are documented in [`docs/project-management.md`](../../../docs/project-management.md). Use the framework below to match rigor to context.*

---

## 1. Scenario Classification Matrix

First, classify your feature or initiative across these dimensions. Score each row honestly — the totals guide your approach.

| Dimension | Low (1) | Medium (2) | High (3) |
|-----------|---------|------------|----------|
| **Stakeholder count** | 1–2 (solo or pair) | 3–8 (single team) | 9+ (cross-team, external) |
| **Domain complexity** | Well-understood, routine | Some unknowns, moderate domain | Novel domain, deep expertise required |
| **Risk of getting it wrong** | Inconvenience (internal tool, low traffic) | Revenue or reputation impact | Safety, legal, or existential risk |
| **Regulatory exposure** | None | Industry standards (e.g., WCAG) | Statutory (HIPAA, GDPR, PCI-DSS, SOX) |
| **Integration surface** | Standalone or 1–2 integrations | Several internal services | Many external systems, legacy migration |
| **Delivery model** | Continuous delivery, easy rollback | Scheduled releases, moderate rollback cost | Fixed-scope contract, hard deadline, expensive rollback |

**Interpret your score:**

| Total | Tier | Approach |
|-------|------|----------|
| 6–9 | **Lightweight** | Bias toward action. Minimal documentation, conversation-driven requirements. |
| 10–14 | **Standard** | Structured but lean. Document key decisions, quantify critical NFRs, formalize acceptance criteria. |
| 15–18 | **Rigorous** | Full-form requirements engineering. Formal traceability, sign-offs, change control, comprehensive NFR specification. |

---

## 2. Principle Selection by Tier

Each principle is documented in full in the [Requirements Gathering Principles](../../../docs/project-management.md) reference. This grid maps each principle to its application at each tier.

| Principle | Lightweight | Standard | Rigorous |
|-----------|-------------|----------|----------|
| Stakeholder Identification | Who's the user? Ask them directly. | Identify all groups; lightweight RACI on key decisions. | Full RACI matrix; persona documentation; stakeholder map. |
| Elicitation Diversity | One conversation or a quick mockup. | 2–3 methods (interview + prototype + doc review). | All applicable methods; facilitated workshops; formal observation. |
| SMART Requirements | Use as a mental checklist. | Write SMART criteria for critical requirements. | Every requirement is explicitly SMART. |
| Clarity & Unambiguity | Always. This costs nothing. | Always. Peer review requirement statements. | Always. Formal inspections against IEEE 830 checklist. |
| Separation of Concerns | Mentally separate functional from NFR. | Document NFRs explicitly; quantify the top 3–5. | Full NFR specification with measurable thresholds for every category. |
| Prioritization | Implicit — build the most valuable thing first. | MoSCoW on the release backlog. | MoSCoW + stack ranking + Kano analysis + cost-of-delay weighting. |
| Traceability | Backlog item links to a user need (even a Slack thread). | User story → acceptance criteria → test case trace. | Full bidirectional traceability matrix (requirement → design → code → test). |
| Validation vs. Verification | Ship it, watch it, iterate. | Demo to stakeholders; automated acceptance tests. | Formal validation sessions; independent verification; audit trail. |
| Iteration & Change Mgmt | Backlog is the living spec. | Baseline per release; triage changes against the baseline. | Formal change control board; impact analysis for every change. |
| Shared Understanding | A shared doc or whiteboard photo. | Glossary + process diagrams + single repo. | Ubiquitous language enforced; formal models (BPMN, UML); signed baselines. |
| NFR Checklist | Skim it — pick the 1–2 that matter. | Walk through all categories; document the top 5 with thresholds. | Every category assessed; quantified thresholds for all applicable NFRs; compliance evidence documented. |
| Anti-Patterns | Awareness is enough — catch yourself. | Periodic self-check; mention risks in refinement. | Formal peer review for anti-pattern detection; retrospective review of past escapes. |

---

## 3. Decision Flowchart (Quick-Start)

Use this sequence when starting a new feature to decide what to apply *right now*. Principle names reference the [Requirements Gathering Principles](../../../docs/project-management.md) document.

```
START
  │
  ├─ Is this a bug fix or trivial change?
  │    YES → SMART mental check + clarity. Done in < 5 min. Ship it.
  │
  ├─ Does the wrong outcome risk revenue, safety, or compliance?
  │    YES → Jump to Rigorous tier. Do not shortcut.
  │
  ├─ Are there 3+ distinct stakeholder groups?
  │    YES → Apply Stakeholder Identification seriously.
  │    NO  → Identify the primary user; move on.
  │
  ├─ Is the domain novel or poorly understood by the team?
  │    YES → Invest in Elicitation Diversity. At least 2 methods.
  │    NO  → A structured conversation or user story mapping is enough.
  │
  ├─ Is there a hard deadline with consequences?
  │    YES → Apply Prioritization (MoSCoW) and MVP thinking aggressively.
  │    NO  → Prioritize naturally; ship when ready.
  │
  ├─ Will this feature live for 6+ months with multiple maintainers?
  │    YES → Invest in Shared Understanding — glossary, diagrams, single source of truth.
  │    NO  → A well-written README or PR description suffices.
  │
  └─ DEFAULT for everything else:
       SMART + Clarity & Unambiguity + Separation of Concerns + Anti-Pattern awareness.
       These are always-on, zero-cost habits — not ceremony.
```

---

## 4. T-shirt Sizing Examples

| Scenario | Tier | What to Actually Do |
|----------|------|---------------------|
| **Fix a misspelled label on an internal admin page** | Lightweight | SMART mental check. Clarify what the label should say. Done. |
| **Add export-to-CSV to an existing report** | Lightweight | Ask the user what columns they need. Confirm format. One acceptance criterion. Ship. |
| **New onboarding flow for a B2B SaaS product** | Standard | Persona + user journey map. SMART acceptance criteria. MoSCoW the features. Quantify performance (page load < 2s). Prototype before build. |
| **Payment processor migration (Stripe → Adyen)** | Standard–Rigorous | RACI (finance, engineering, ops, support). Full NFR checklist (security, compliance, availability). Traceability on PCI-relevant requirements. Formal rollback plan. |
| **Patient health data API for hospital integration** | Rigorous | Full RACI. HIPAA compliance evidence per requirement. Bidirectional traceability. Formal change control. Independent security review. Signed-off baseline. |
| **Greenfield marketplace platform (0→1, 6-month timeline)** | Rigorous | All elicitation methods. Personas + journey maps for buyers and sellers. MoSCoW + Kano for MVP scope. Full NFR specification. Glossary of domain terms. Change control board for post-baseline changes. |

---

## 5. Signals to Escalate

If you encounter any of these, increase your tier by one level (or at minimum, pause and reassess):

- A stakeholder says "I thought we already agreed on this" — you have a shared understanding gap.
- Two stakeholders describe the same feature in contradictory ways — you have a consistency problem.
- Someone asks "Why are we building this?" — you have a traceability gap.
- A requirement contains "and" or "or" — it's not atomic; split it.
- A non-functional requirement is stated without numbers — NFRs must be quantified.
- The team estimates vary by more than 2× — the requirement is ambiguous.
- A stakeholder has been silent for two sprints — stakeholder engagement is slipping.
- "We'll figure that out later" applies to security, compliance, or data — escalate to Rigorous immediately.

---

## References

- **IEEE 830-1998** — Recommended Practice for Software Requirements Specifications
- **IIBA BABOK Guide v3** — Business Analysis Body of Knowledge
- **ISO/IEC/IEEE 29148:2018** — Systems and Software Engineering — Requirements Engineering
- **CMMI for Development v2.0** — Requirements Development and Management (RDM) Practice Area
- **Agile Alliance** — Agile Requirements Best Practices