---
name: ship-check
description: Pre-preview or production checklist - functionality, UI breakpoints, secrets, typecheck/lint/test/build, env vars per environment. Slash-only - invoke with /ship-check.
disable-model-invocation: true
---

# Ship check

Run before preview or production. Read SECURITY.md and TEST_PLAN.md if present. Report pass / fail / N/A per item - do not invent green checks.

## Checklist

### Functionality
- [ ] Happy path for current MVP / slice works end-to-end
- [ ] Auth (if any): sign-up, sign-in, sign-out, protected routes
- [ ] Empty / loading / error states visible and sane

### UI
- [ ] Smoke at mobile and desktop widths used in DESIGN.md (or ~375 and ~1280)
- [ ] Primary actions reachable by keyboard where interactive

### Security
- [ ] No secrets in git, client bundles, or chat
- [ ] No privileged keys in `NEXT_PUBLIC_*` (or equivalent)
- [ ] Env examples have names only; real values only in local/hosting secrets

### Quality gates (run what the repo has)
- [ ] Typecheck
- [ ] Lint
- [ ] Unit/integration tests
- [ ] Build
- [ ] E2E or manual TEST_PLAN smoke

### Environment
- [ ] Required env vars set for **this** target (local / preview / production)
- [ ] Preview URL (or prod) verified after deploy - not only `localhost`

## Output
Verdict: **ready for preview** | **ready for production** | **not ready** - with failed items and next actions.
