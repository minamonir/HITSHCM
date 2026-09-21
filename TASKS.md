# TASKS - HITSHCM

## Phase 0 - Inventory (current)

- [x] INV-001 Map top-level solution/folders in HITSNasDnaFS
- [x] INV-002 Map NasDna V12.1 WebForms areas (folders, masters, key modules)
- [x] INV-003 List major dependencies (NuGet, COM, external services)
- [x] INV-004 Identify DB touchpoints (connection strings patterns, DAL/EF)
- [x] INV-005 Catalog sibling apps (mobile, APIs, integrations, client forks)
- [x] INV-006 Rank candidate strangler modules (risk vs value)
- [x] INV-007 Write inventory report + recommend strategy

## Phase 1 - TBD after inventory

- [ ] Choose strategy (likely strangler + API-first on first slice)
- [ ] Characterization tests for critical paths
- [ ] First slice implementation plan

## UI/UX modernization track (D-012)

| ID | Task | Status |
|----|------|--------|
| UX-0 | Foundations: TARGET layout, tokens, base CSS, RTL smoke, PR checklist | **done** — AUTH-1 product chrome (login + landing + `_Layout`); see `docs/UX.md` |
| UX-1 | Pattern library: list/filter, detail form, modal, picker, empty/loading/error | todo |
| UX-2 | Enforce UX DoD on each strangler slice | todo |
| UX-3 | Mode B: replace Default/Agenda hub UX | later |

## Inventory — data dictionary / controls
- [x] **INV-DD-001** Document DataDictionary + HITSCulturedControl + HITSControlLibrary (docs/architecture/data-dictionary-controls.md)
- [ ] **INV-DD-002** Map GetOrgDataDictionary vs base table / org overlays
- [ ] **INV-DD-003** Per-slice TableName usage export (before first form rebuild)

## Inventory — session / auth
- [x] **INV-SESS-001** Document CURRENT session & auth (docs/architecture/session-management.md)
- [ ] **INV-SESS-002** Profile store DB / property map per environment
- [ ] **INV-SESS-003** Prod InProc vs Redis + MemoryCacheKeys
- [ ] **INV-SESS-004** SSO claim → Nas user → BusinessGroup path

## TARGET auth / session
- [x] **AUTH-0** Confirm D-013 / D-013a / D-013b (OpenIddict-first)
- [x] **AUTH-1** Spike OpenIddict + cookie BFF on HITSHCM host (`src/Hitshcm.Web`, see `docs/AUTH.md`)
- [ ] **AUTH-2** ITenantContext + connection factory (Key Vault later) — `ITenantContext` claims stub exists; factory/SQL not started
- [ ] **AUTH-3** Mode A session-exchange spike with legacy logon
- [ ] **AUTH-4** Rights service wrapper (replace Session UserGroupRights reads)

