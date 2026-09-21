# Prompt 06 - Structured debug

Project: HITSHCM | Owner: Mina

Use when something is broken (or `/vibe-debug` / structured-debug).

```text
Structured recovery for HITSHCM. Diagnose before fixing.

Fill first:
ERROR: 
EXPECTED: 
ACTUAL: 
REPRO: 
CONSTRAINT: (RULES / env / design limits)

Then:
1) Primary root cause hypothesis with file evidence
2) Smallest fix only
3) Re-run REPRO / tests
4) If fixed: propose a GOTCHAS.md row (symptom | cause | fix)
5) If not: new facts only - no drive-by refactors

Do not expand into unrelated cleanup.
```
