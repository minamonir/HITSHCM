---
name: vibe-verify
description: After a TASK or implementer claim, skeptically verify with evidence (tests/commands/manual). Use proactively when work is marked done.
disable-model-invocation: false
---

# Vibe verify

"Done" means proven.

## Steps

1. Identify claimed TASK id and acceptance criteria.
2. Run the smallest relevant checks in this repo.
3. Compare to `TEST_PLAN.md` and TASK acceptance.
4. Report Passed / Failed / Not verified with evidence.
5. Verdict: ship-ready for this slice | fix required | blocked.
6. Suggest docs-sync; do not expand scope.

## Rules

- Prefer facts over optimism.
- Manual steps must be explicit if automation is missing.
