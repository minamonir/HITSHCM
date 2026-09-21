# SETUP - HITSHCM

Day-1 checklist. Check items off as you go. Owner: Mina | Started: 2026-09-21

## A. Open and orient

- [ ] Open this folder in Cursor
- [ ] Skim `AGENTS.md`, `docs/workflow/OVERVIEW.md`, and `docs/playbook/QUICKSTART.md`
- [ ] Confirm `.cursor/agents`, `.cursor/skills`, `.cursor/rules` are present

## B. Product clarity

- [ ] Run research (`docs/prompts/01-research.md` or `/vibe-research`)
- [ ] Draft MVP PRD (`docs/prompts/02-prd.md` or `/vibe-prd`) into `PRD.md`
- [ ] Capture open questions in `PRD.md` and `MEMORY.md`

## C. Tech shape

- [ ] Tech design (`docs/prompts/03-tech-design.md` or `/vibe-tech-design`)
- [ ] Fill `ARCHITECTURE.md`, `DESIGN.md`, `SECURITY.md`
- [ ] Fill `docs/agent/tech_stack.md` and `docs/agent/testing.md`
- [ ] Bootstrap agents (`docs/prompts/04-agent-bootstrap.md`) so MEMORY reflects reality

## D. Local env

- [ ] Copy `.env.example` to `.env` locally (never commit `.env`)
- [ ] Install **.NET 10 SDK** (`dotnet --list-sdks`)
- [ ] Optional: copy `.mcp.json.example` to `.mcp.json` only if you need MCP; fill locally, never commit secrets
- [ ] Confirm ignore rules in `.gitignore`
- [ ] `dotnet run --project src/Hitshcm.Web` (see root `README.md`)

## E. First slice

- [ ] Break MVP into vertical slices in `TASKS.md` (TASK-001 first)
- [ ] planner on TASK-001 -> implementer -> `/vibe-verify`
- [ ] docs-sync; append DECISIONS/GOTCHAS if needed

## Done when

You can describe the MVP in one sentence, name the next TASK, and run one smoke check.
