# AGENTS - HITSHCM

Master contract for every Cursor agent and human collaborator on this repo.
Inspired by Trellis-style project contracts: short, mandatory, living.

## 1. Read first (every session)

Before planning or coding, read in order:

1. `AGENTS.md` (this file)
2. `PRD.md`
3. `ARCHITECTURE.md`
4. `TASKS.md`
5. `MEMORY.md`

Also skim when relevant: `DESIGN.md`, `RULES.md`, `DECISIONS.md`, `GOTCHAS.md`, `SECURITY.md`.

## 2. Work loop

- One task at a time from `TASKS.md`.
- Sequence: **plan -> implement -> verify**.
- Prefer vertical slices (thin end-to-end) over horizontal layers.
- Do not claim done without evidence: tests run, commands shown, or manual checks listed.

## 3. Hard stops

- Never commit secrets. Never edit `.env` (use `.env.example` names only).
- Never invent features outside the current TASK / MVP In list.
- If docs conflict, stop and ask; do not silently expand scope.

## 4. After bugfixes and architecture choices

- Append a row to `GOTCHAS.md` after a real bugfix (symptom / cause / fix).
- Append one line to `DECISIONS.md` for architecture or stack choices (date | decision | why).

## 5. Where agents and skills live

| Kind | Path |
|---|---|
| Agents | `.cursor/agents/` - planner, implementer, reviewer, verifier |
| Skills | `.cursor/skills/` - vertical-slice, docs-sync, ship-check, vibe-* |
| Rules | `.cursor/rules/` - general, frontend, backend, testing, guardrails |
| Prompts | `docs/prompts/` - research through debug |
| Workflow | `docs/workflow/OVERVIEW.md` |

## 6. Daily default

planner -> implementer -> verifier (or `/vibe-verify`) -> docs-sync -> `/ship-check` before preview/prod.

Owner: Mina | Scaffolded: 2026-09-21
