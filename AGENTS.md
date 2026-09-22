# AGENTS.md — HITSHCM

Master contract for every Cursor agent and human collaborator on this repo.
Inspired by Trellis-style project contracts: short, mandatory, living.

## 1. Read first (every session) — MANDATORY

Before planning or coding, read in this order:

1. `AGENTS.md` (this file)
2. **`ARCHITECTURE.md`** — CURRENT brownfield system (agent entry)
3. **`docs/inventory/00-index.md`** — Phase 0 inventory pack
4. **`DECISIONS.md`** — D-001… especially D-007 SoT, D-008 dacpac, D-009 arch pack
5. `docs/architecture/current-state.md` — depth behind ARCHITECTURE.md
6. `docs/architecture/migration-plan.md` — whole-system strangler waves (D-014)
7. `TASKS.md` / `MEMORY.md` for active work
8. Skim when relevant: `PRD.md`, `DESIGN.md`, `RULES.md`, `GOTCHAS.md`, `SECURITY.md`

**Do not invent a greenfield stack.** This is brownfield strangler + API-first.

## 2. Source of truth & write roots

| Role | Path | Access |
|---|---|---|
| **SoT application code** | `D:\Workspaces\HITSNasDnaFS\HITSNasDna` | **READ ONLY** |
| Solution | `...\HITSNasDna\HITSNasDna.sln` | read |
| Web project | `...\NasDna\NasDna.vbproj` (VB.NET Web Forms, **net48**, HITSDNA **2020.01**) | read |
| Sibling compare only | `D:\Workspaces\HITSNasDnaFS\HITSNasDnaV12.1` | read / diffs |
| **Canonical schema** | `C:\Users\mina\Projects\HITSHCM\docs\inventory\db\DNACloudDB.dacpac` | read |
| **Write root** | `C:\Users\mina\Projects\HITSHCM\` | write here only |
| **Never touch** | `D:\Workspace\Roadmap\HITSDo\HITSDoForC`, EasyDO | forbidden |

Some `.md` files may be UTF-16 — detect BOM; write new/updated docs as **UTF-8**.

## 3. Work loop

- One task at a time from `TASKS.md`.
- Sequence: **plan → implement → verify**.
- Prefer vertical slices (thin end-to-end) over horizontal layers.
- Prefer strangling existing seams (ERec / Time / NasAI / HITSAPIs) — do not start with NasSetup / WorkFlow / NasForms.
- Do not claim done without evidence: tests run, commands shown, or manual checks listed.

## 4. Hard stops

- Never commit secrets. Never edit `.env` (use `.env.example` names only). Never paste live Web.config passwords/IdP secrets into docs.
- Never invent features outside the current TASK / MVP In list.
- Never treat V12.1 or Integration embeds as SoT (D-007).
- If docs conflict, stop and ask; do not silently expand scope.

## 5. After bugfixes and architecture choices

- Append a row to `GOTCHAS.md` after a real bugfix (symptom / cause / fix).
- Append one line / section to `DECISIONS.md` for architecture or stack choices (date | decision | why).

## 6. Where agents and skills live

| Kind | Path |
|---|---|
| Agents | `.cursor/agents/` — planner, implementer, reviewer, verifier |
| Skills | `.cursor/skills/` — vertical-slice, docs-sync, ship-check, vibe-* |
| Rules | `.cursor/rules/` — **current-architecture**, general, frontend, backend, testing, guardrails |
| Prompts | `docs/prompts/` |
| Workflow | `docs/workflow/OVERVIEW.md` |

## 7. Daily default

planner → implementer → verifier (or `/vibe-verify`) → docs-sync → `/ship-check` before preview/prod.

Owner: Mina | Updated: 2026-09-21

## TARGET vs CURRENT

- **CURRENT (legacy SoT):** ARCHITECTURE.md + docs/architecture/current-state.md — 2-tier Web Forms + SQL (D-010)
- **TARGET (modern slices):** docs/architecture/target-state.md — 3-tier UI → ASP.NET Core API → SQL strangler on **.NET 10 LTS** (D-011 / D-011a)
- New code defaults to TARGET under HITSHCM write root; SoT remains read-only.

**UI (D-005):** Razor Pages default for new slices; Blazor allowed for interactive modules only.

## UI/UX track (D-012)

- Spec: docs/architecture/target-ux.md`r
- Rule: .cursor/rules/target-ux.mdc`r
- Ship UX DoD with every strangler slice; shell/tokens/RTL before one-off screen CSS.

