# Rules - HITSHCM

Human-readable twin of `.cursor/rules`. Agents must obey these.

1. One TASK at a time from `TASKS.md`.
2. Read `AGENTS.md`, `PRD.md`, `ARCHITECTURE.md`, `TASKS.md`, `MEMORY.md` before coding.
3. No scope creep beyond MVP In / current TASK.
4. Never commit secrets; never edit `.env`.
5. Prefer vertical slices; evidence before "done".
6. Loop: planner -> implementer -> verifier -> docs-sync.
7. After fixes: append `GOTCHAS.md`. After architecture choices: append `DECISIONS.md`.
8. Ask before adding dependencies or changing public APIs.
