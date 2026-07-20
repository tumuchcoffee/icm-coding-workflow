# Decision Framework — Choosing the Right Level of Rigor

> *This document helps you decide which requirements-gathering practices to apply. The underlying principles are documented in [`docs/project-management.md`](../../../docs/project-management.md). Use the framework below to match rigor to context.*

---

## 0. Feature Intake — Ingest, Merge & Draft

> *This section is an instruction block for an AI agent. Read it as your step-by-step operating procedure before proceeding to the classification matrix.*

**Goal:** Produce a set of architecture-ready requirements artifacts at `.\output\*.md` from all source material in `.\references\`. These artifacts bridge the gap between raw requirements and the architecture phase — they provide the Architecture Stage (Stage 02) with classified, structured, and traceable user stories, acceptance criteria, NFR specifications, and a prioritized backlog.

**Step-by-step:**

1. **Scan the references folder.** List every file in `.\references\` (recursively). Accept any format — markdown, plain text, JSON, YAML, CSV, images (describe what you see), and PDFs or Office documents if readable. Note the filename, type, and a one-line summary of what each file appears to contain.
2. **Extract all requirements signals.** For every source file, pull out:

   - Explicit functional requirements — restate each one as a user story using the template: **"As a [role], I want [action], so that [outcome]."** Then derive GIVEN/WHEN/THEN acceptance criteria. Even if the source uses prose, translate it into this format.
   - Explicit non-functional requirements (performance, security, compliance, availability, scalability, usability thresholds — anything with a number or a measurable target). Flag any NFR that is stated without a number as "needs quantification."
   - Implicit signals: stakeholder names/roles mentioned, system integrations referenced, pain points described, constraints stated (budget, timeline, technology, regulatory).
   - Contradictions or gaps between sources — flag them prominently; do not silently resolve them.
3. **Merge and de-duplicate.** Combine findings into a unified set:

   - Group related requirements by functional area or user journey. Use a consistent structure (e.g., "Functional Requirements" → sub-grouped by feature area; "Non-Functional Requirements" → sub-grouped by NFR category).
   - Merge duplicate or near-duplicate statements into a single, clearest version. Note in a `<!-- comment -->` when you consolidated.
   - Preserve the source trail: after each requirement or group, add a `**Sources:** file-a.md, file-b.md` attribution so traceability is maintained back to the original references.
4. **Apply the tier-appropriate rigor.** Use the classification matrix in Section 1 to determine the tier for this feature, then apply the principle-selection grid from Section 2 to decide how thoroughly to structure the draft. At minimum, always apply Clarity & Unambiguity and Separation of Concerns (functional vs. NFR).
5. **Write the requirements artifacts.** Save the following files to `.\output\` using these skeletons:

   - **`output/feature-requirements.md`** — Functional requirements expressed as **user stories** in Gherkin-style GIVEN/WHEN/THEN format, each with acceptance criteria. Group by feature area or user journey. Every story must be atomic, unambiguous, and testable. Include a summary of the user personas relevant to this feature.

     ```markdown
     ## User Persona: Software Engineer
     **Role**: A developer contributing to the Synergistic codebase.
     **Goal**: Run and develop the full application stack locally without manual setup.

     ### FR-001: Local Development Environment
     **As a** software engineer,
     **I want** to run the entire application from my local system with a single command,
     **so that** I can develop and test features without depending on shared infrastructure.

     **Acceptance Criteria**:
     - [ ] GIVEN I have cloned the repository
           WHEN I run the startup script
           THEN the Angular SPA is available at http://localhost:4200
     - [ ] GIVEN I have cloned the repository
           WHEN I run the startup script
           THEN the .NET API is available at http://localhost:5001
     - [ ] GIVEN I have cloned the repository
           WHEN I run the startup script
           THEN the SQL Server database is created and accessible

     **Sources:** requirements.md
     ```

   - **`output/non-functional-requirements.md`** — Quantified NFRs grouped by category (Performance, Security, Availability, Scalability, Maintainability, Usability, Compliance). Every NFR must include a measurable threshold, measurement method, and priority. If the source material lacks numbers, flag this as a gap.

     ```markdown
     ## NFR-001: Page Load Performance
     **Category**: Performance
     **Threshold**: Initial page load < 2 seconds at p95 on a 10 Mbps connection
     **Measurement**: Lighthouse audit via CI pipeline
     **Priority**: Must Have
     **Sources:** requirements.md
     ```

   - **`output/backlog-and-prioritization.md`** — A MoSCoW-prioritized backlog that maps user stories to implementation order. Include a traceability matrix linking each story back to its source reference.

     ```markdown
     | Priority | ID      | Story Summary                          | Source            |
     |----------|---------|----------------------------------------|--------------------|
     | Must     | FR-001  | Run entire app from local system       | requirements.md    |
     | Must     | FR-002  | Angular shell with Header/Footer/Menu  | requirements.md    |
     | Should   | FR-003  | Right-hand detail pane                 | requirements.md    |
     ```

   - **`output/glossary.md`** — Domain terminology that emerges from the requirements. Define each term once so all artifacts use consistent language.

     ```markdown
     | Term              | Definition                                                      |
     |-------------------|-----------------------------------------------------------------|
     | Synergistic       | The multi-tenant SaaS administration panel application          |
     | Health Check      | A lightweight endpoint that returns application status          |
     | Slide-out Menu    | A navigation panel that expands from the left side of the screen|
     ```

6. **Stop and report.** After writing the files, summarize: tier chosen, number of user stories produced, NFRs quantified, glossary terms defined, any gaps or contradictions flagged, and any files that could not be read. Do not proceed to architecture — the output files are the sole deliverable for this phase.

---

## 1. Scenario Classification Matrix

First, classify your feature or initiative across these dimensions. Score each row honestly — the totals guide your approach.

| Dimension                          | Low (1)                                    | Medium (2)                                 | High (3)                                                |
| ---------------------------------- | ------------------------------------------ | ------------------------------------------ | ------------------------------------------------------- |
| **Stakeholder count**        | 1–2 (solo or pair)                        | 3–8 (single team)                         | 9+ (cross-team, external)                               |
| **Domain complexity**        | Well-understood, routine                   | Some unknowns, moderate domain             | Novel domain, deep expertise required                   |
| **Risk of getting it wrong** | Inconvenience (internal tool, low traffic) | Revenue or reputation impact               | Safety, legal, or existential risk                      |
| **Regulatory exposure**      | None                                       | Industry standards (e.g., WCAG)            | Statutory (HIPAA, GDPR, PCI-DSS, SOX)                   |
| **Integration surface**      | Standalone or 1–2 integrations            | Several internal services                  | Many external systems, legacy migration                 |
| **Delivery model**           | Continuous delivery, easy rollback         | Scheduled releases, moderate rollback cost | Fixed-scope contract, hard deadline, expensive rollback |

**Interpret your score:**

| Total  | Tier                  | Approach                                                                                                             |
| ------ | --------------------- | -------------------------------------------------------------------------------------------------------------------- |
| 6–9   | **Lightweight** | Bias toward action. Minimal documentation, conversation-driven requirements.                                         |
| 10–14 | **Standard**    | Structured but lean. Document key decisions, quantify critical NFRs, formalize acceptance criteria.                  |
| 15–18 | **Rigorous**    | Full-form requirements engineering. Formal traceability, sign-offs, change control, comprehensive NFR specification. |

---

## 2. Principle Selection by Tier

Each principle is documented in full in the [Requirements Gathering Principles](../../../docs/project-management.md) reference. This grid maps each principle to its application at each tier.

| Principle                   | Lightweight                                              | Standard                                                         | Rigorous                                                                                                |
| --------------------------- | -------------------------------------------------------- | ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| Stakeholder Identification  | Who's the user? Ask them directly.                       | Identify all groups; lightweight RACI on key decisions.          | Full RACI matrix; persona documentation; stakeholder map.                                               |
| Elicitation Diversity       | One conversation or a quick mockup.                      | 2–3 methods (interview + prototype + doc review).               | All applicable methods; facilitated workshops; formal observation.                                      |
| SMART Requirements          | Use as a mental checklist.                               | Write SMART criteria for critical requirements.                  | Every requirement is explicitly SMART.                                                                  |
| Clarity & Unambiguity       | Always. This costs nothing.                              | Always. Peer review requirement statements.                      | Always. Formal inspections against IEEE 830 checklist.                                                  |
| Separation of Concerns      | Mentally separate functional from NFR.                   | Document NFRs explicitly; quantify the top 3–5.                 | Full NFR specification with measurable thresholds for every category.                                   |
| Prioritization              | Implicit — build the most valuable thing first.         | MoSCoW on the release backlog.                                   | MoSCoW + stack ranking + Kano analysis + cost-of-delay weighting.                                       |
| Traceability                | Backlog item links to a user need (even a Slack thread). | User story → acceptance criteria → test case trace.            | Full bidirectional traceability matrix (requirement → design → code → test).                         |
| Validation vs. Verification | Ship it, watch it, iterate.                              | Demo to stakeholders; automated acceptance tests.                | Formal validation sessions; independent verification; audit trail.                                      |
| Iteration & Change Mgmt     | Backlog is the living spec.                              | Baseline per release; triage changes against the baseline.       | Formal change control board; impact analysis for every change.                                          |
| Shared Understanding        | A shared doc or whiteboard photo.                        | Glossary + process diagrams + single repo.                       | Ubiquitous language enforced; formal models (BPMN, UML); signed baselines.                              |
| NFR Checklist               | Skim it — pick the 1–2 that matter.                    | Walk through all categories; document the top 5 with thresholds. | Every category assessed; quantified thresholds for all applicable NFRs; compliance evidence documented. |
| Anti-Patterns               | Awareness is enough — catch yourself.                   | Periodic self-check; mention risks in refinement.                | Formal peer review for anti-pattern detection; retrospective review of past escapes.                    |

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

| Scenario                                                           | Tier               | What to Actually Do                                                                                                                                                                                     |
| ------------------------------------------------------------------ | ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Fix a misspelled label on an internal admin page**         | Lightweight        | SMART mental check. Clarify what the label should say. Done.                                                                                                                                            |
| **Add export-to-CSV to an existing report**                  | Lightweight        | Ask the user what columns they need. Confirm format. One acceptance criterion. Ship.                                                                                                                    |
| **New onboarding flow for a B2B SaaS product**               | Standard           | Persona + user journey map. SMART acceptance criteria. MoSCoW the features. Quantify performance (page load < 2s). Prototype before build.                                                              |
| **Payment processor migration (Stripe → Adyen)**            | Standard–Rigorous | RACI (finance, engineering, ops, support). Full NFR checklist (security, compliance, availability). Traceability on PCI-relevant requirements. Formal rollback plan.                                    |
| **Patient health data API for hospital integration**         | Rigorous           | Full RACI. HIPAA compliance evidence per requirement. Bidirectional traceability. Formal change control. Independent security review. Signed-off baseline.                                              |
| **Greenfield marketplace platform (0→1, 6-month timeline)** | Rigorous           | All elicitation methods. Personas + journey maps for buyers and sellers. MoSCoW + Kano for MVP scope. Full NFR specification. Glossary of domain terms. Change control board for post-baseline changes. |

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
