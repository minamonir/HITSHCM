---
name: structured-debug
description: Use when something breaks (test fail, runtime error, wrong UI). Force ERROR/EXPECTED/ACTUAL/REPRO/CONSTRAINT, find root cause first, then smallest fix - no drive-by refactors.
disable-model-invocation: false
---

# Structured debug

Diagnose before fixing.

## Fill first
```text
ERROR: [message / symptom]
EXPECTED: [correct behavior]
ACTUAL: [what happened]
REPRO: [exact steps / command]
CONSTRAINT: [relevant RULES, env, or design limits]
```

## Then
1. Hypothesize **root cause** (one primary). Cite file/evidence.
2. Propose the **smallest fix** that addresses that cause.
3. Apply only that fix unless the user expands scope.
4. Re-run the repro / relevant tests; report pass or new facts.

## Avoid
- Refactors, renames, or "while we're here" cleanups
- Guessing without reading the failing path
- Changing unrelated features
