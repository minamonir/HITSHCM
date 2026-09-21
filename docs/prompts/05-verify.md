# Prompt 05 - Verify after a slice

Project: HITSHCM | Owner: Mina

Use after implementer claims a TASK done (or `/vibe-verify` / verifier agent).

```text
Verify the latest slice for HITSHCM. Be skeptical.

1) Identify claimed TASK id and acceptance criteria (TASKS.md / chat).
2) Run the smallest relevant checks available (test, lint, typecheck, smoke).
3) Compare results to TEST_PLAN.md and the TASK acceptance list.
4) Report:
   - Passed (criteria + evidence)
   - Failed / incomplete (repro)
   - Not verified (why)
   - Verdict: ship-ready for this slice | fix required | blocked
5) Suggest docs-sync edits (do not expand scope).

Never mark done on vibes alone. Evidence or explicit manual steps required.
```
