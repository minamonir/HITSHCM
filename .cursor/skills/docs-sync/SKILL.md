---
name: docs-sync
description: Use after a completed vertical slice or verified task. Update TASKS.md checkboxes, MEMORY.md status, DECISIONS.md if architecture changed, GOTCHAS.md after fixes, and README if needed. Keep docs truthful.
disable-model-invocation: false
---

# Docs sync

Keep project docs aligned with what actually shipped.

## After a completed slice

1. **TASKS.md** - check off the finished item; note blockers on anything deferred.
2. **MEMORY.md** - short status: what works, known gaps, next focus.
3. **DECISIONS.md** - only if architecture/stack/API shape changed; one line (date | decision | why).
4. **GOTCHAS.md** - if a real bug was fixed; symptom | cause | fix.
5. **README.md** - only if setup, scripts, or user-facing capabilities changed.
6. Do **not** invent future features in docs. Reflect reality.

## Output

List files updated and a one-line summary of each change.
