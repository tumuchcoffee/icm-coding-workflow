# ICM Coding Workflow

An ICM style software development template.

---

## What is ICM?

**Interpretable Context Methodology (ICM)** is a method for orchestrating AI agent workflows using folder structure, markdown files, and local scripts — instead of a multi-agent framework. It was introduced in the paper [*Interpretable Context Methodology: Folder Structure as Agentic Architecture*](https://arxiv.org/abs/2603.16021) (Van Clief & McDermott, 2026).

The central insight: if the prompts and context for each stage of a workflow already exist as files in a well-organized folder hierarchy, you don't need a coordination framework to manage multiple specialized agents. You need **one orchestrating agent** that reads the right files at the right moment. The folder structure tells it what to do at each step.

### The Five Design Principles

ICM is built on five principles borrowed from decades of established software engineering practice:

| Principle | Description | Roots |
|---|---|---|
| **One stage, one job** | Each stage handles a single step and writes output to its own folder | Unix philosophy (McIlroy, 1978); Parnas's information hiding (1972) |
| **Plain text as the interface** | Stages communicate through markdown and JSON files — no binary formats, no databases | Kernighan & Pike's "text is the universal interface" (1984) |
| **Layered context loading** | Agents load only the context needed for the current stage, avoiding the "lost in the middle" problem | Liu et al. (2024); prevention over compression |
| **Every output is an edit surface** | Intermediate output of each stage is a file a human can open, read, edit, and save before the next stage runs | Horvitz's mixed-initiative principles (1999); Shneiderman's direct manipulation (1983) |
| **Configure the factory, not the product** | A workspace is set up once with preferences, style, and structure. Each run produces a new deliverable using the same configuration | Humble & Farley's continuous delivery (2010) |

### The Five-Layer Context Hierarchy

An ICM workspace delivers context to the agent through five layers:

```
Layer 0:  AGENT.md        — "Where am I?"       (~800 tokens)   Structural routing
Layer 1:  CONTEXT.md       — "Where do I go?"    (~300 tokens)   Structural routing
Layer 2:  Stage CONTEXT.md — "What do I do?"     (200–500 tok)   Structural routing
Layer 3:  Reference material — "What rules apply?" (500–2k tok)  Content (the factory)
Layer 4:  Working artifacts — "What am I working with?" (varies)  Content (the product)
```

- **Layers 0–2** provide structural routing and stage instructions.
- **Layer 3** holds reference material (design systems, voice rules, coding standards, style guides) — stable across runs.
- **Layer 4** holds working artifacts (the output of the previous stage, user-provided source material) — changes every run.

This layered loading means each stage typically receives only **2,000–8,000 tokens** of focused context. A monolithic approach loading everything at once could exceed 40,000 tokens, most of it irrelevant to the current task.

### How Stages Work

Each stage defines a **contract** with three parts:

1. **Inputs** — Which Layer 3 (reference) and Layer 4 (working) files to load
2. **Process** — What transformation to perform
3. **Outputs** — What files to write to the stage's `output/` folder

The output of stage N becomes the input to stage N+1. At each boundary, a **review gate** lets the human inspect and edit the output before the next stage runs. This is prompt chaining at the filesystem level.

### Where ICM Works (and Doesn't)

**Ideal for:**
- Sequential multi-step workflows with human review at each stage
- Content production pipelines, training material development, research analysis
- Workflows where the same pipeline runs repeatedly with different input
- Teams where non-developers need to modify agent behavior

**Not suited for:**
- Real-time multi-agent collaboration with dynamic communication
- High-concurrency systems with many simultaneous users
- Workflows requiring complex automated branching based on AI decisions mid-pipeline

### How This Repo Applies ICM

This repository uses ICM's folder structure to organize a **software development workflow**:

- **`source/01-ui/`** — Stage 1: UI/frontend implementation
- **`source/02-backend/`** — Stage 2: Backend/business logic
- **`source/03-sql/`** — Stage 3: Database/schema work
- **`docs/`** — Layer 3 reference material (coding standards, system architecture)
- **`AGENT.md`** — Layer 0: global agent identity and instructions
- **`feature-workspace/template/`** — Template for spinning up new feature workspaces

Each numbered stage folder is a self-contained pass in the pipeline. The agent reads the relevant reference material, processes the current stage, and writes output that feeds into the next stage — all inspectable, editable, and version-controlled as plain files.

---

### Further Reading

- [Full paper on arXiv](https://arxiv.org/abs/2603.16021) — Van Clief & McDermott, 2026
- [ICM is MIT-licensed](https://arxiv.org/abs/2603.16021) — open source, model-agnostic, and Git-compatible by design
