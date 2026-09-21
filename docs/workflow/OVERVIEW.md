# Workflow overview - HITSHCM

Idea -> Research -> PRD -> Tech Design -> Build -> Verify

Keep each stage thin. Do not skip Research/PRD for greenfield work.

## 1. Idea

Capture a one-sentence vision and who it is for. Park shiny extras in Out.

## 2. Research

Use `docs/prompts/01-research.md` or `/vibe-research`.

Interview the human. Clarify users, pain, alternatives, constraints, success signal.
Write findings into MEMORY and open questions into PRD.

## 3. PRD

Use `docs/prompts/02-prd.md` or `/vibe-prd`.

Produce a short MVP PRD in `PRD.md`: In / Out, stories, metrics, risks.

## 4. Tech design

Use `docs/prompts/03-tech-design.md` or `/vibe-tech-design`.

Fill `ARCHITECTURE.md`, `DESIGN.md`, `SECURITY.md`, and `docs/agent/tech_stack.md`.
Log stack choices in `DECISIONS.md`.

## 5. Build

Bootstrap agent context (`docs/prompts/04-agent-bootstrap.md`).
Break MVP into TASKS. For each TASK:

planner -> implementer (vertical slice) -> tests

## 6. Verify

Use `docs/prompts/05-verify.md`, verifier agent, or `/vibe-verify`.
Evidence required. Then docs-sync. `/ship-check` before preview/prod.

## Recovery

If broken: `docs/prompts/06-debug.md` or `/vibe-debug` / structured-debug.
Append GOTCHAS after the fix.

## Stage gates (simple)

| From | Ready when |
|---|---|
| Research -> PRD | Users, problem, constraints named |
| PRD -> Tech Design | MVP In/Out stable |
| Tech Design -> Build | Stack + boundaries written |
| Build -> Verify | Acceptance criteria listed for the TASK |
| Verify -> Next TASK | Evidence recorded; TASK checked |
