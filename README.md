# HITSHCM

Scaffolded with the Vibe Coding starter (workflow upgrade). Owner: Mina | Created: 2026-09-21

Brownfield HCM modernization (strangler + API-first). Architecture docs on `main` stay authoritative for CURRENT vs TARGET. This repo now also hosts the first TARGET Razor slice: **OpenIddict local IdP + cookie BFF**.

## Run the Razor host (AUTH-1)

Requires **.NET 10 SDK** (LTS).

```bash
dotnet --list-sdks   # expect 10.x
dotnet restore Hitshcm.sln
dotnet build Hitshcm.sln
dotnet run --project src/Hitshcm.Web --launch-profile https
```

| URL | Who |
|-----|-----|
| https://localhost:3000/ | Authenticated landing (anonymous users redirect to login) |
| https://localhost:3000/Account/Login | Password login (orchestrator) |
| https://localhost:3000/Account/ChangePassword | Forced after CheckURules-shaped `003` |
| https://localhost:3000/Account/FirstLogon | First-logon ack placeholder |
| https://localhost:3000/Account/Logout | Clears the HttpOnly cookie |
| https://localhost:3000/.well-known/openid-configuration | OpenIddict discovery |

The default launch profile is **HTTPS on port 3000** (HTTP fallback on 3001). First visit may show a browser warning for the ASP.NET HTTPS development certificate — continue to localhost. Do not pass `--urls http://...` or Chrome HTTPS-only mode will refuse the page.

**Seeded Development user** (created on first run; change in any shared environment):

| Field | Value |
|-------|--------|
| Email / username | `admin@hitshcm.local` |
| Password | `ChangeMe!123` |
| Org id | `demo-org` (login picker can also set `demo-org-east`) |
| Business group id | `demo-bg` (also `demo-bg-hr`, `demo-bg-east` on the login dropdown) |

SQLite files land in `src/Hitshcm.Web/App_Data/` (gitignored). This is the **Identity + OpenIddict** store only — **not** DNACloudDB.

```bash
dotnet test Hitshcm.sln
```

Language: top-bar **EN** / **العربية** segmented control sets `Hitshcm.Culture` and `dir="rtl"` for Arabic. Same pages, no `*_ar` forks.

**UX-0 chrome:** design tokens in `src/Hitshcm.Web/wwwroot/css/tokens.css`, shared styles in `app.css`. Run book + token table: [`docs/UX.md`](docs/UX.md). Track spec: [`docs/architecture/target-ux.md`](docs/architecture/target-ux.md).

## D-013 mapping (short)

| Lock | What this host does |
|------|---------------------|
| **D-013** | OpenIddict local OIDC IdP + BFF cookie `Hitshcm.Auth` (HttpOnly). No JWT in `localStorage`. Entra is **not** wired yet. |
| **D-013a** | Tenant from `org_id` / `bg_id` claims (`ITenantContext`). `ITenantConnectionFactory` builds CURRENT `build_connectionString` server-side. Never a connection string in the identity name. |
| **D-013b** | No InProc Session for business state. Mode A Agenda bridge is out of scope. |

Full flow: [`docs/AUTH.md`](docs/AUTH.md). CURRENT logon study: [`docs/architecture/current-login-flow.md`](docs/architecture/current-login-flow.md). TARGET session spec: [`docs/architecture/target-session.md`](docs/architecture/target-session.md).

Later API project belongs at `src/Hitshcm.Api` (not created this slice).

## Agent / docs workflow

1. Open this folder in Cursor
2. Follow `SETUP.md`
3. Research -> PRD -> tech design (see `docs/prompts/` or `/vibe-research`, `/vibe-prd`, `/vibe-tech-design`)
4. Fill `TASKS.md`, then planner -> implementer -> verifier
5. `/ship-check` before preview or production

## Layout

| Path | Purpose |
|---|---|
| `Hitshcm.sln` | TARGET solution (.NET 10) |
| `src/Hitshcm.Web` | Razor Pages host + local OpenIddict IdP |
| `tests/Hitshcm.Web.Tests` | Auth flow tests |
| `AGENTS.md` | Master agent contract |
| `PRD.md` / `ARCHITECTURE.md` / `DESIGN.md` | Living product + tech docs |
| `TASKS.md` / `MEMORY.md` / `DECISIONS.md` / `GOTCHAS.md` | Execution memory |
| `docs/AUTH.md` | AUTH-1 login/cookie/landing |
| `docs/UX.md` | UX-0 tokens, chrome, how to run |
| `docs/workflow/` | Idea -> Verify overview |
| `docs/prompts/` | Interview-style workflow prompts |
| `docs/playbook/` | Full vibe coding guide |
| `.cursor/` | Agents, skills, rules |

## Agents

planner | implementer | reviewer | verifier

## Key skills

`/vibe-research` `/vibe-prd` `/vibe-tech-design` `/vibe-verify` `/vibe-debug` plus vertical-slice, docs-sync, ship-check, six-part-prompt, structured-debug
