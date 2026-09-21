---
name: vibe-tech-design
description: Draft architecture, design, security, and stack docs from a stable MVP PRD. Slash-only - invoke with /vibe-tech-design.
disable-model-invocation: true
---

# Vibe tech design

Shape the system for a fast first vertical slice.

## Steps

1. Read `PRD.md` (MVP In/Out must be stable enough).
2. Draft updates for `ARCHITECTURE.md`, `DESIGN.md`, `SECURITY.md`.
3. Fill `docs/agent/tech_stack.md` and `docs/agent/testing.md`.
4. Append one-line entries to `DECISIONS.md` for material choices.
5. Propose ordered slices that can become `TASKS.md` items.

## Rules

- Prefer boring tech that fits stated constraints.
- Authz model early; env var NAMES only; no secrets.
- List rejected alternatives briefly.
