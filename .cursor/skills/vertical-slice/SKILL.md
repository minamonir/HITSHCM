---
name: vertical-slice
description: Use when starting a feature or TASKS item. Define a thin user-visible end-to-end flow before layering polish, extra UI, or refactors.
disable-model-invocation: false
---

# Vertical slice

Ship the thinnest path a user can complete before adding polish.

## Steps
1. Name the **user-visible outcome** for this TASKS.md item (one sentence).
2. Sketch the thin path: UI entry -> API/data -> persistence -> UI confirmation (or equivalent for this stack).
3. List **must-have** for the slice vs **defer** (polish, edge cases that are not acceptance criteria).
4. Implement must-have only; match RULES.md / DESIGN.md / ARCHITECTURE.md.
5. Smoke-test the path once before expanding.

## Avoid
- Horizontal layers with no user proof ("just the schema", "just the API")
- Redesigns mid-slice
- Second features riding along
