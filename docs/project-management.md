# Requirements Gathering Principles

> *A distillation of industry-recognized standards from BABOK (IIBA), IEEE 830, CMMI, and modern agile practices.*

---

## 1. Stakeholder Identification & Inclusive Elicitation

Identify **all** stakeholders — direct users, sponsors, subject-matter experts, operations, compliance, and downstream consumers. Cast a wide net early; missed stakeholders are the #1 source of late-breaking scope changes.

- **RACI matrix** — define who is Responsible, Accountable, Consulted, and Informed for each requirement area.
- **Personas & user journeys** — ground requirements in real workflows, not abstract feature lists.
- **Continuous engagement** — requirements are not gathered once; they are refined iteratively with stakeholder feedback loops.

---

## 2. Elicitation Diversity

No single technique captures every need. Employ multiple complementary methods:

| Technique | Best For |
|-----------|----------|
| Structured interviews | Deep-dive domain knowledge |
| Workshops | Cross-functional alignment & conflict resolution |
| Observation (job shadowing) | Tacit/unspoken workflows |
| Prototyping & wireframes | Validating UI/UX before build |
| Surveys & questionnaires | Broad quantitative data |
| Document analysis | Existing business rules & constraints |
| Brainstorming | Greenfield innovation |

---

## 3. SMART Requirements

Every requirement must satisfy the **SMART** criteria:

- **Specific** — one requirement per statement; avoid conjunctions ("and"/"or").
- **Measurable** — define objective acceptance criteria (e.g., "Page loads in < 2 seconds at p95").
- **Achievable** — technically feasible within constraints.
- **Relevant** — traceable to a business objective or user need.
- **Time-bound** — when must it be delivered? What is the priority window?

---

## 4. Clarity & Unambiguity (IEEE 830)

- **Atomic** — each requirement addresses exactly one concern.
- **Unambiguous** — only one interpretation is possible. Avoid vague adjectives ("fast," "intuitive," "robust").
- **Complete** — the requirement stands alone; no missing information the implementer must guess.
- **Consistent** — no contradiction with any other requirement.
- **Verifiable** — there exists a cost-effective way to prove the requirement has been met.
- **Feasible** — achievable with known technology and within budget/schedule.
- **Necessary** — if removed, a deficiency exists that cannot be fulfilled by another requirement.
- **Implementation-free** — describes *what*, not *how*.

---

## 5. Separation of Concerns

- **Functional requirements** — what the system *does* (features, behaviors, data transformations).
- **Non-functional requirements (NFRs)** — how the system *is* (performance, security, availability, scalability, maintainability, compliance).
- **Constraints** — immovable boundaries (technology stack, regulatory, budget, timeline).
- **Business rules** — policies that govern decisions (not system features).

NFRs must be quantified. "The system shall be fast" is not a requirement; "The system shall respond to 95% of search queries in under 200 ms" is.

---

## 6. Prioritization (MoSCoW Method)

| Category | Meaning |
|----------|---------|
| **Must have** | Non-negotiable; the release fails without it. |
| **Should have** | High importance but a workaround exists; painful to omit. |
| **Could have** | Nice-to-have; included if time/resources permit. |
| **Won't have (this time)** | Explicitly deferred to a future release. |

Complement MoSCoW with **stack ranking** within each category and **Kano analysis** for features where delight vs. dissatisfaction tradeoffs matter.

---

## 7. Traceability

Establish bidirectional traceability:

- **Forward trace** — business objective → requirement → design → code → test case.
- **Backward trace** — test case → code → design → requirement → business objective.

Every requirement must justify its existence by tracing to a business goal or user need. If you cannot trace it, question whether it belongs.

---

## 8. Validation vs. Verification

- **Validation** — "Are we building the right thing?" (stakeholder review, prototyping, acceptance criteria walkthroughs).
- **Verification** — "Are we building the thing right?" (peer reviews, inspections, test alignment, formal analysis).

Both are continuous activities, not phase gates.

---

## 9. Iteration & Change Management

Requirements evolve. Anticipate change with:

- **Baseline** — snapshot approved requirements at key milestones.
- **Change control process** — every proposed change is assessed for impact, cost, and priority before acceptance.
- **Backlog refinement** — regularly groom the backlog; stale requirements rot.
- **MVP thinking** — deliver the smallest coherent slice that validates the hypothesis, then iterate.

---

## 10. Shared Understanding & Communication

- **Ubiquitous language** — use the domain's terminology consistently; a glossary of terms prevents ambiguity.
- **Visual models** — process flows, state diagrams, decision tables, and entity-relationship diagrams convey structure words cannot.
- **Single source of truth** — requirements live in one accessible, version-controlled location.
- **Sign-off** — formal stakeholder approval on baselines, acknowledging that sign-off represents understanding, not a frozen-scope promise.

---

## 11. Non-Functional Requirements Checklist

Use this as a prompt during elicitation. Not every category applies, but every category should be *considered*:

- [ ] **Performance** — response time, throughput, concurrency
- [ ] **Scalability** — horizontal/vertical, data volume growth
- [ ] **Availability** — uptime %, disaster recovery RPO/RTO
- [ ] **Security** — authentication, authorization, data encryption, threat model
- [ ] **Compliance** — GDPR, HIPAA, SOC 2, PCI-DSS, etc.
- [ ] **Usability** — learnability, efficiency, error tolerance, accessibility (WCAG)
- [ ] **Maintainability** — logging, monitoring, observability, configuration management
- [ ] **Portability** — platform, browser, device support
- [ ] **Data** — retention, archival, migration, integrity constraints

---

## 12. Anti-Patterns to Avoid

| Anti-Pattern | Why It Hurts |
|--------------|--------------|
| Gold-plating | Building what wasn't asked for wastes budget and adds complexity. |
| Analysis paralysis | Infinite requirements gathering delays delivery; bias toward action. |
| Solutioning too early | Jumping to *how* before understanding *what* and *why* constrains innovation. |
| Silent stakeholders | Unengaged stakeholders become late-stage critics. |
| Copy-paste requirements | Reusing requirements without revalidation imports stale assumptions. |
| Ambiguous adjectives | "Fast," "easy," "flexible," "robust" — meaningless without quantification. |

---

## References

- **IEEE 830-1998** — Recommended Practice for Software Requirements Specifications
- **IIBA BABOK Guide v3** — Business Analysis Body of Knowledge
- **ISO/IEC/IEEE 29148:2018** — Systems and Software Engineering — Requirements Engineering
- **CMMI for Development v2.0** — Requirements Development and Management (RDM) Practice Area
- **Agile Alliance** — Agile Requirements Best Practices