# Test plan - HITSHCM

Updated: 2026-09-21

## Smoke (every slice)

- [ ] Happy path for the current TASK / MVP slice
- [ ] Auth gates (if any)
- [ ] Empty / loading / error states

## Automated (when stack exists)

| Suite | Command | When |
|---|---|---|
| Unit | | each slice |
| Lint / typecheck | | each slice |
| E2E critical | | before preview |

## Responsive smoke

- [ ] ~375px
- [ ] ~768px
- [ ] ~1280px+

## Evidence rule

Verifier and `/vibe-verify` must cite commands + results (or explicit manual steps) before "done".
