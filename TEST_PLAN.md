# Test plan - HITSHCM

Updated: 2026-09-21

## Smoke (every slice)

- [x] AUTH-1: anonymous `/` redirects to `/Account/Login`
- [x] AUTH-1: seed login reaches landing; logout clears cookie
- [x] UX-0: login chrome (skip link, culture switch, labeled auth card)
- [x] UX-0: landing identity strip + what’s-next cards; Arabic `dir="rtl"`
- [x] Login CURRENT cues: BG field, Office 365/OKTA stubs, teal/cyan tokens, split hero; selected BG sets claims; cookie still HttpOnly
- [x] AUTH-1b: policy 002/003/004, BG claims on success, must-change redirect, login page uses orchestrator
- [ ] Empty / loading / error states (beyond login validation) — UX-1

## Automated (when stack exists)

| Suite | Command | When |
|---|---|---|
| Auth + shell chrome | `dotnet test Hitshcm.sln` | AUTH-1 / UX-0 host changes |
| Build | `dotnet build Hitshcm.sln` | each slice |
| E2E critical | | before preview (browser) |

## AUTH-1 manual

1. `dotnet run --project src/Hitshcm.Web`
2. Open http://localhost:5080/ → login
3. Sign in `admin@hitshcm.local` / `ChangeMe!123` with Business group **Demo HITS**
4. Confirm welcome + identity chips (user / org / bg) and what’s-next cards; DevTools: `Hitshcm.Auth` HttpOnly, no JWT in localStorage
5. Repeat with **East Region** — chips show `demo-org-east` / `demo-bg-east` (never a connection string)
6. Log out via user menu → back to login; `/` redirects again
7. Language: العربية → `dir="rtl"`, culture control shows العربية active, nav/chips mirror; login split still usable
8. Office 365 / OKTA buttons are visible stubs (coming soon; password login still works)
9. AUTH-1b: `mustchange@hitshcm.local` / `ChangeMe!123` → `/Account/ChangePassword` (landing gated). `inactive@` and `hrinactive@` stay on login with no cookie.

## Responsive smoke

- [x] ~375px — header compact (avatar + culture), login card full width, cards stack (screenshot smoke 2026-09-21)
- [x] ~768px — two-column what’s-next (screenshot smoke 2026-09-21)
- [x] ~1280px+ — three-column what’s-next, identity strip in a row (screenshot smoke 2026-09-21)

## Evidence rule

Verifier and `/vibe-verify` must cite commands + results (or explicit manual steps) before "done".
