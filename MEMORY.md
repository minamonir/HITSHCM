# MEMORY — HITSHCM

Last updated: 2026-09-21 (Africa/Cairo)

## Current status

- **AUTH-1b (2026-09-21):** Shared `ILoginOrchestrator` on the Razor host — Identity password (SSO resume stub), `ITenantCatalog` BG memberships, CheckURules-shaped `ILoginPolicyEvaluator` (002/003/004, fail-closed), `ITenantConnectionFactory` named-options descriptor, `org_id`/`bg_id`/`profile_id`/`emp_id` claims (never conn strings). Must-change → `/Account/ChangePassword`; first-logon ack placeholder. CURRENT map: `docs/architecture/current-login-flow.md`.
- **UX-0 login chrome (2026-09-21):** `/Account/Login` matches live HITS Agentic logon (hitshcmcloud.org) — full-height teal/patterned split, Username+Password first paint, Log In + Office 365, BG picker only as a second step. Older five-cue frosted card superseded. Study: `docs/architecture/current-login-ux.md`.
- **UX-0 (2026-09-21):** AUTH-1 shell restyled as TARGET product chrome — expanded tokens, `_Layout` brand/user menu/culture switch, landing identity chips + what’s-next placeholders, RTL smoke. Docs: `docs/UX.md`. Auth protocol unchanged (OpenIddict + cookie BFF).
- **AUTH-1 (2026-09-21):** `Hitshcm.sln` + `src/Hitshcm.Web` Razor Pages host on .NET 10 with OpenIddict local IdP, ASP.NET Identity, SQLite (dev), HttpOnly BFF cookie, login + landing + logout. Docs: `docs/AUTH.md`. Seed `admin@hitshcm.local` / `ChangeMe!123`. No DNACloudDB, no Entra, no Mode A bridge.
- **D-009:** CURRENT-state architecture pack written for Cursor (`ARCHITECTURE.md`, `docs/architecture/*`, always-on rule).
- **D-008:** use full DNACloudDB.dacpac (not dbo-only package).
- Source of truth **corrected**: `D:\Workspaces\HITSNasDnaFS\HITSNasDna` (not V12.1).
- Inventory pack exists under `docs/inventory/00–09`; dacpacs under `docs/inventory/db/`.
- First strangler slice still TBD (D-004).

## SoT facts (verified 2026-09-21)

- Solution: `HITSNasDna.sln` (10 VB projects; Azure deploy folders on disk but **not** in sln)
- Web: `NasDna\NasDna.vbproj` — VB.NET Web Forms, **net48**
- Assembly: HITSDNA 2020.01 (`2020.01.2024.0422`)
- ~1573 aspx, ~615 ascx, ~8479 files (SoT root); NasDna tree ~7996 files
- Connection names: LocalSqlServer, DefaultConnection → ASPNETDB; NasDotNetDevConnectionString → DNACloudDB; BGSetupConnectionString → DNACloudDBBG; ViewStateConnectionString → ASPNETDB; StoreConnectionString → hitsstore
- Auth: Forms (`logon.aspx`); Membership/Profile on DefaultConnection
- Data path: `NASDataSource` ← `Profile("ConnectionString")` + L2S `NasDB.dbml` (~503 tables)
- DBs: DNACloudDB, DNACloudDBBG, ASPNETDB present on fz-dv-db01; **hitsstore missing**
- dacpacs: DNACloudDB (canonical), DNACloudDBBG, aspnetdb

## Architecture doc locations

- `ARCHITECTURE.md` — agent entry
- `docs/architecture/current-state.md` — full CURRENT map
- `docs/architecture/context.mermaid` — context diagram
- `.cursor/rules/current-architecture.mdc` — always-on

## Next

- [ ] Optional: quick diff HITSNasDna vs V12.1
- [x] Obtain DNACloudDB schema (dacpac) — 2026-09-21
- [x] Document CURRENT architecture for Cursor — 2026-09-21
- [ ] Pick first strangler slice (ERec / Time / NasAI / …)
- [ ] Phase 1 plan against HITSNasDna paths only
- [ ] Locate hitsstore / Profile tenant connection matrix

- D-011 TARGET baseline: 3-tier UI→ASP.NET Core API→SQL strangler; DNACloudDB SoR; C#/.NET 10; UI = Razor Pages default (D-005); Blazor optional.rchitecture/target-state.md

- D-005 (2026-09-21): TARGET UI = **Razor Pages** default; **Blazor** allowed for interactive modules only; API ASP.NET Core .NET 10 unchanged.

- D-012 (2026-09-21): UI/UX modernization is a first-class track parallel to capability strangler; see docs/architecture/target-ux.md (UX-0..UX-3).

- AUTH-1 (2026-09-21): first TARGET code in-repo — Razor host + OpenIddict cookie BFF.
- AUTH-1b (2026-09-21): login orchestrator + CheckURules-shaped policy stand-in. Next: AUTH-2 live SQL tenant factory; D-004 first strangler slice still TBD.
- UX-0 (2026-09-21): foundations shipped on that host (tokens/chrome/RTL). Login later restyled with CURRENT NasDna cues (all five). Next UX: UX-1 pattern library (list/filter/detail).

