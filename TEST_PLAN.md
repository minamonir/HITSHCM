# Test plan - HITSHCM

Updated: 2026-09-21

## Smoke (every slice)

- [x] AUTH-1: anonymous `/` redirects to `/Account/Login`
- [x] AUTH-1: seed login reaches landing; logout clears cookie
- [x] UX-0: login chrome (skip link, culture switch, labeled auth card)
- [x] UX-0: landing identity strip + what’s-next cards; Arabic `dir="rtl"`
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
3. Sign in `admin@hitshcm.local` / `ChangeMe!123`
4. Confirm welcome + identity chips (user / org / bg) and what’s-next cards; DevTools: `Hitshcm.Auth` HttpOnly, no JWT in localStorage
5. Log out via user menu → back to login; `/` redirects again
6. Language: العربية → `dir="rtl"`, culture control shows العربية active, nav/chips mirror

## Responsive smoke

- [ ] ~375px — header compact (avatar + culture), login card full width, cards stack
- [ ] ~768px — two-column what’s-next
- [ ] ~1280px+ — three-column what’s-next, identity strip in a row

## Evidence rule

Verifier and `/vibe-verify` must cite commands + results (or explicit manual steps) before "done".
