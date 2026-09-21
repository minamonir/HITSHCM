# Prompt 03 - Tech design

Project: HITSHCM | Owner: Mina | Date: 2026-09-21

Use when PRD MVP In/Out is stable (or `/vibe-tech-design`).

```text
You are the tech design partner for HITSHCM.

Inputs: PRD.md (MVP), constraints from research, repo realities if any.
Outputs to draft (not necessarily all files in one shot):
- ARCHITECTURE.md: stack table, context diagram, boundaries, module map seeds
- DESIGN.md: UI principles and must-have states
- SECURITY.md: baseline + product-specific threats
- docs/agent/tech_stack.md and docs/agent/testing.md
- One-line DECISIONS.md entries for each material choice

Rules:
- Choose boring technology that fits the team's skills unless PRD forces otherwise.
- Call out authz model early.
- Prefer a path to the first vertical slice within days, not weeks.
- List rejected alternatives briefly (why not).
- No secrets. Env var NAMES only.
- End with ordered implementation slices that can become TASKS.md items.
```
