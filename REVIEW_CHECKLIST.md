# Review checklist - HITSHCM

Use with the reviewer agent or before merge.

## Scope

- [ ] Matches current TASK acceptance criteria
- [ ] No drive-by features or refactors
- [ ] Docs touched if behavior/setup changed (docs-sync)

## Correctness

- [ ] Edge cases called out in plan handled or deferred explicitly
- [ ] Error paths do not fail silently

## Security

- [ ] No secrets committed
- [ ] Authz checked server-side for sensitive ops
- [ ] Inputs validated

## Quality

- [ ] Types / lint / tests for this slice (or justified N/A)
- [ ] UI states: loading / empty / error where relevant

## Evidence

- [ ] Commands or manual steps recorded in the PR / chat
- [ ] TASKS.md / MEMORY.md update planned or done

Verdict: approve | request changes | blocked
