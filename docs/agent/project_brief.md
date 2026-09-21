# Project brief — HITSHCM

Owner: Mina | Updated: 2026-09-21

## One-liner

Brownfield modernization of **HITS NasDna / HITSDNA** enterprise HCM (VB.NET Web Forms, .NET Framework 4.8) via **strangler + API-first** — not a greenfield rewrite.

## Primary user

- **HR admins / payroll / ESS users** on the existing NasDna IIS app (CURRENT)
- **Cursor agents / engineers** working in `HITSHCM` docs + future slice code (this workspace)

## CURRENT system (must know)

- **SoT:** `D:\Workspaces\HITSNasDnaFS\HITSNasDna` (`HITSNasDna.sln`, `NasDna\NasDna.vbproj`)
- **Assembly:** HITSDNA 2020.01 (`2020.01.2024.0422`), net48
- **Size:** ~1573 aspx, ~615 ascx, ~8479 files in SoT root
- **Data:** DNACloudDB (+ BG + aspnetdb); Profile-driven L2S `NASDataSource`
- **Schema pack:** `docs/inventory/db/DNACloudDB.dacpac` (D-008)
- **Depth:** `ARCHITECTURE.md` → `docs/architecture/current-state.md`

## MVP outcomes (modernization workspace)

- Keep CURRENT architecture documented so agents do not invent stacks
- Inventory + dacpac available for planning (Phase 0 — largely done)
- Pick and deliver first strangler slice behind APIs (Phase 1 — TBD, D-004)

## Non-goals

- Big-bang rewrite of NasSetup / WorkFlow / NasForms
- Treating V12.1 or client ERec forks as SoT
- Touching EasyDO / `HITSDoForC`
- Locking a full target UI stack before first slice choice

## Links

- PRD: `PRD.md`
- Architecture (entry): `ARCHITECTURE.md`
- Current-state depth: `docs/architecture/current-state.md`
- Inventory: `docs/inventory/00-index.md`
- Decisions: `DECISIONS.md`
- Tasks: `TASKS.md`