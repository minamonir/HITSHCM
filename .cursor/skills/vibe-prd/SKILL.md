---
name: vibe-prd
description: Draft a short MVP PRD into PRD.md from research notes. Slash-only - invoke with /vibe-prd.
disable-model-invocation: true
---

# Vibe PRD

Produce a living MVP PRD. Prefer ruthless scope.

## Steps

1. Read research notes, `MEMORY.md`, and current `PRD.md` if any.
2. Fill `PRD.md`: Vision, Problem, Users, Outcome, MVP In/Out, few user stories, Metrics, Risks, Open questions, Non-goals.
3. Every In item must map to a user-visible outcome.
4. Mark unknowns as Open questions; do not invent fake metrics.
5. End with "Ready for tech design" or "Blocked: ..." and the top decisions needed.

## Avoid

- Feature laundry lists
- Tech stack debates (defer to `/vibe-tech-design`)
