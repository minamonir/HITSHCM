# Test plan - HITSHCM

Updated: 2026-09-21

## Smoke (every slice)

- [x] AUTH-1: anonymous `/` redirects to `/Account/Login`
- [x] AUTH-1: seed login reaches landing; logout clears cookie
- [ ] Empty / loading / error states (beyond login validation)

## Automated (when stack exists)

| Suite | Command | When |
|---|---|---|
| Auth flow | `dotnet test Hitshcm.sln` | AUTH-1 and later host changes |
| Build | `dotnet build Hitshcm.sln` | each slice |
| E2E critical | | before preview (browser) |

## AUTH-1 manual

1. `dotnet run --project src/Hitshcm.Web`
2. Open http://localhost:5080/ → login
3. Sign in `admin@hitshcm.local` / `ChangeMe!123`
4. Confirm welcome card + org/bg ids; DevTools: `Hitshcm.Auth` HttpOnly, no JWT in localStorage
5. Log out → back to login; `/` redirects again
6. Language stub: العربية → `dir="rtl"`

## Responsive smoke

- [ ] ~375px
- [ ] ~768px
- [ ] ~1280px+

## Evidence rule

Verifier and `/vibe-verify` must cite commands + results (or explicit manual steps) before "done".
