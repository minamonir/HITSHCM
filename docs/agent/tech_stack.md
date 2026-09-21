# Tech stack — HITSHCM

Updated: 2026-09-21

> **CURRENT** = what the brownfield SoT runs today.  
> **TARGET** = modernization direction only (not locked beyond strangler + API-first).

## CURRENT stack (SoT: `D:\Workspaces\HITSNasDnaFS\HITSNasDna`)

| Area | Choice | Why / evidence |
|---|---|---|
| Language | **VB.NET** (primary); some C# in siblings | `NasDna.vbproj`; sibling APIs in C# |
| App framework | **ASP.NET Web Forms** on **.NET Framework 4.8** | `TargetFrameworkVersion` v4.8; ToolsVersion 12 |
| Assembly branding | **HITSDNA 2020.01** (`2020.01.2024.0422`) | `My Project\AssemblyInfo.vb` |
| UI | `.aspx` / `.ascx`, master pages, AjaxControlToolkit, ReportViewer | ~1573 aspx / ~615 ascx |
| Hosting (current) | **IIS** + System.Web | Classic pipeline; Azure Cloud Service folders exist off-sln |
| Data store | **SQL Server** — `DNACloudDB`, `DNACloudDBBG`, `ASPNETDB` (+ `hitsstore` named, missing on sample) | Web.config connection **names**; dacpacs under `docs/inventory/db/` |
| Persistence | **LINQ to SQL** (`NasDB.dbml` / `NASDataSource` + Profile routing), raw ADO.NET, typed DataSets, EntLib; thin **EF6** Identity island | Not a single-ORM app |
| Auth | **Forms auth** + SQL Membership/Profile/Roles; optional Okta OIDC / ComponentSpace SAML / OWIN | `authentication mode=Forms`; dual eras |
| Session | InProc (Redis provider alternate in config) | Do not copy secrets |
| Reporting | ReportViewer + SSRS web refs; Power BI packages; `HITSReports` sibling | |
| Solution peers | ControlLibrary, CulturedControl, BO/AX interfaces, CustomProviders, ViewStateProviders, EncryptQS, RTFHTMLConvert | In `HITSNasDna.sln` |

Canonical schema package: **`docs/inventory/db/DNACloudDB.dacpac`** (D-008).

## TARGET direction (strategy — not an implementation lock)

| Area | Direction | Status |
|---|---|---|
| Approach | **Strangler fig + API-first** | D-001, D-002 — decided |
| New services | Modern .NET API hosts for carved slices | D-005 TBD |
| New UI | Per-slice (Blazor / SPA / selective page modernize) | D-005 TBD |
| First slice | ERec / Time / NasAI hypotheses | D-004 TBD |
| Data | Keep SQL Server; anti-corrupt against DNACloudDB | — |
| Identity | Consolidate on modern OIDC where clients allow | Open |

Do **not** fill this table with aspirational greenfield defaults as if they were CURRENT.

## Local commands (legacy SoT)

| Action | Command / note |
|---|---|
| Open SoT | Open `HITSNasDna.sln` in Visual Studio (Windows / IIS / net48 tooling) |
| Build | MSBuild / VS build of `NasDna.vbproj` (on SoT machine — do not copy tree into HITSHCM) |
| Schema | SqlPackage extract already stored under `docs/inventory/db/` |

HITSHCM repo itself is docs/agent workspace — no app runtime to `npm start` here yet.

## Env var names (no values)

See `.env.example`. Never paste live Web.config secrets.

## TARGET stack (new slices — D-011)

| Layer | Choice |
|-------|--------|
| Language | C# |
| Runtime | .NET 10 |
| API | ASP.NET Core Web API + OpenAPI |
| UI | Razor Pages (default; D-005) |
| Data | DNACloudDB via adapters (Dapper/EF Core); full dacpac D-008 |
| Auth | Forms bridge → OIDC |
| Pattern | Strangler 3-tier; legacy Web Forms remains until cutover |

See `docs/architecture/target-state.md`.


