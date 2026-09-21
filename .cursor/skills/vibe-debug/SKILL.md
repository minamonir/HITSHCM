---
name: vibe-debug
description: Structured recovery when something breaks. Force ERROR/EXPECTED/ACTUAL/REPRO/CONSTRAINT, then smallest fix. Auto-apply when relevant.
disable-model-invocation: false
---

# Vibe debug

Diagnose before fixing. Companion to `structured-debug`.

## Fill first

```text
ERROR:
EXPECTED:
ACTUAL:
REPRO:
CONSTRAINT:
```

## Then

1. Primary root cause with file evidence
2. Smallest fix only
3. Re-run REPRO / relevant tests
4. On success: propose a `GOTCHAS.md` row
5. On failure: new facts only

## Avoid

- Drive-by refactors
- Scope expansion
- Guessing without reading the failing path
