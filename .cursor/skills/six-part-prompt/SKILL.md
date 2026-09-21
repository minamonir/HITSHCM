---
name: six-part-prompt
description: Build a six-part implementation prompt (CONTEXT/TASK/FILES/CONSTRAINTS/ACCEPTANCE/TESTING) from project docs and a task id. Slash-only - invoke with /six-part-prompt.
disable-model-invocation: true
---

# Six-part prompt

Fill a ready-to-run prompt from project docs. Do not invent product features.

## Steps
1. Read PRD.md, RULES.md, DESIGN.md (if present), ARCHITECTURE.md (if present), and the named item in TASKS.md (ask for task id if missing).
2. Produce **only** this filled template (edit placeholders from docs):

```text
CONTEXT: [product + relevant constraints from PRD/RULES; current slice]
TASK: [exact TASKS.md item - one sentence]
FILES: [paths likely to create/edit; match repo layout]
CONSTRAINTS: [RULES.md + DESIGN.md must-follow; no scope creep]
ACCEPTANCE CRITERIA:
- [checkable bullet]
- [...]
TESTING:
- [commands and/or manual steps]
```

3. If the user asked to **run** it, proceed to implement that prompt. Otherwise present it ready to copy/paste or hand to implementer.
4. Keep it short. One task only.
