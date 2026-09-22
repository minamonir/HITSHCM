# HITSHCM whole-system migration plan

> **Status:** Program plan (hypothesis for D-004 first *capability* slice)
> **Updated:** 2026-09-21
> **Decisions:** D-001 brownfield; D-002 strangler; D-005 Razor default; D-007 SoT; D-010 split logic; D-011/.011a TARGET; D-012 UX track; D-013 auth; **D-014 this plan**
> **Evidence:** live SoT scan `D:\Workspaces\HITSNasDnaFS\HITSNasDna` + inventory `docs/inventory/`
> **Not:** a big-bang rewrite, a Gantt with fake dates, or permission to edit SoT

## 1. End state

HITSHCM is the product: **C# / .NET 10** Razor (Blazor only where dense) to **ASP.NET Core API** to SQL. Users sign in once (OpenIddict, later Entra/Okta on the same gateway). Tenant/BG is a **server-resolved claim**, never a connection string in the cookie. `DNACloudDB` stays system of record until a slice *explicitly* takes data ownership.

NasDna Web Forms is gone. Client ERec forks are gone. Mobile and integrations call the same APIs. Agenda iframes, `_ar` page twins, Profile.ConnectionString, and fat Session bags are gone.

Until then, **legacy IIS stays up**. Every wave cuts over *journeys*, not folders.

## 2. How to use this document

| You want | Do this |
|----------|---------|
| Sequence for the whole product | Section 6 wave map |
| Where a SoT folder lands | Section 7 |
| How to ship *one* journey | Section 5 recipe (repeat ~40 times) |
| What to build next week | Section 12 (Wave 1) |
| What not to start | Section 10 |

Waves are **order**, not calendar. Duration depends on team size and characterization depth. Treat this as a multi-year coexistence program.

## 3. Non-negotiables

1. Legacy keeps running until that journey's traffic is on TARGET (D-002).
2. New code lives under `C:\Users\mina\Projects\HITSHCM\` -- SoT is read-only unless Mina orders otherwise.
3. Each slice inventories **VB + T-SQL** before rewrite (D-010). Prefer wrapping existing procs (`WebAPI2*`, `Insert_Empvacat`, `WFAction`, ...) over boiling 1,800 procs.
4. Each slice ships **capability + TARGET UX DoD** (D-012): layout, tokens, LTR+RTL, list/detail, no new iframe inside Razor.
5. No `Profile.ConnectionString` and no fat `HttpContext.Session` in new code (D-013 / D-013a).
6. Mode A = session-exchange / bridge cookie, not InProc sharing (D-013b).
7. Do not start with NasSetup, NasBatches, WorkFlow *engine*, SSRS hub, or replacing `logon.aspx` in place.
8. Do not create another full NasDna client fork.
9. Do not touch EasyDO / HITSDoForC.

## 4. Coexistence (the migration *runtime*)

```text
Mode A (years)
  Browser -> NasDna IIS (Agenda, remaining aspx)
               iframe / reverse-proxy / encrypted QS bridge
               -> Hitshcm.Web  (.NET 10 Razor + BFF cookie)
               -> Hitshcm.Api  (ASP.NET Core) -- adapters --> DNACloudDB
                                                             DNACloudDBBG
                                                             (aspnetdb until membership dies)

Mode B (late)
  Browser -> Hitshcm.Web shell (replaces Default/Agenda)
               leftover aspx reverse-proxied until 0
```

- **Mode A:** user still logs into `logon.aspx`; after AUTH-3, legacy login also issues the TARGET cookie. New screens launch from Agenda without nested iframes *inside* Razor.
- **Mode B (UX-3):** TARGET is the hub. Leftover aspx are proxied or linked until retired.
- **Mobile:** HITSMobileV12 REST is a sibling seam. Over waves 2-5, those controllers become *clients of* Hitshcm.Api (or are replaced by it). Do not keep two employee-leave contracts forever.

## 5. Repeatable slice recipe

One vertical slice = one user-visible journey (example: "employee submits vacation"). Not "the TimeManagement folder."

| Step | Done when |
|------|-----------|
| **0. Name the outcome** | One sentence. Must-have vs defer listed. |
| **1. Characterize** | List aspx/ascx + `NASDataSource` methods + procs/views + Session keys + rights checks. Golden-path test against live/legacy (characterization harness). |
| **2. Adapter** | Typed C# facade over *existing* procs/views. No new undocumented T-SQL pile. Tenant from `ITenantContext`. |
| **3. API** | ASP.NET Core endpoints + OpenAPI. AuthZ policies via rights service (AUTH-4), not Session DataTables. |
| **4. UI** | Razor Pages (Blazor only if dense). UX-1 patterns. EN/AR resources. UX PR checklist. |
| **5. Bridge** | Mode A launch from Agenda *or* direct URL for ESS. Feature flag. Dual-run if writes are dangerous (payroll). |
| **6. Cut over** | Legacy aspx redirects or menu `ItemProg` points at new route. Monitor. |
| **7. Retire** | Delete/stop shipping that aspx/_ar pair when traffic is zero. Update inventory. |

**Definition of done (every slice):** characterization evidence; adapter tests; API tests; LTR+RTL; rights on every command; no conn string in claims; docs-sync (`TASKS`, `GOTCHAS` if bugs, `MEMORY`).

## 6. Wave map (whole system)

| Wave | Name | What moves | Depends on | Exit |
|------|------|------------|------------|------|
| **0** | Know the beast | Inventory, CURRENT/TARGET docs, AUTH-1 host, UX-0 chrome | -- | **Done** 2026-09-21 |
| **1** | Platform to touch DNA | AUTH-2 live tenant SQL, AUTH-3 Mode A spike, AUTH-4 rights, UX-1 patterns, login/employee-master/payroll-path characterization, CI for `Hitshcm.sln`. AUTH-1b (orchestrator + named-options factory stub) already landed. | Wave 0 | First API can open a tenant DNACloudDB without Profile |
| **2** | First journey -- **vacation** (D-004 *proposal*) | ESS request + balance + mobile `EmpVacations` contract unified on Hitshcm.Api. Wrap `Insert_Empvacat` / vac functions. Thin WF page still legacy. | Wave 1 | One hire-to-retire step on TARGET |
| **3** | ESS read cluster | Directory, profile read, my-team, payslip *view*, web-part equivalents | Wave 1-2 | Managers/employees can live on TARGET for read |
| **4** | Time (write) | Incomplete TK, geo, attendance/shifts -- wrap `Hits_CheckShifts` / `HITS_UpdateEmpTimeReading` + mobile TK controllers. HITSTKService behind API. | Wave 1 + time characterization | Time entry not on aspx for pilot orgs |
| **5** | Requests / benefits / COS | Mobile `EmpHRRequest`, `EmpBenefits`, `EmpChangeOfStatus` + matching `Common` forms | Waves 2-4 | More WF *documents* on TARGET; WF *inbox* still legacy |
| **6** | ERec + freeze forks | Applicants, interviews, shortlist API + Razor. **Stop new HITSDNAErec* full-tree forks.** Pilot one client (smallest tree). | Wave 1; Mode A public pages as needed | One client ERec path not a NasDna fork |
| **7** | Training | `ETraining` + HitsLMS proxy as TARGET API | Wave 1 | Training attendance/services on TARGET |
| **8** | NasAI | Talk-to-data and AI screens call HITSAI through Hitshcm BFF; kill in-monolith iframe/JWT sprawl; secret hygiene on sibling Python | Wave 1 | NasAI aspx facade only |
| **9** | Talent loop | EObjectives, appraisal forms, HRKPI *entry* (not all NasSetup KPI config) | Waves 2-5 | Performance cycle on TARGET for pilot |
| **10** | Workflow *platform* | Inbox/outbox, `WFAction` / `wfDocumentApproval` adapter, attachments (`WFAttachmentHandler`) | Several document types already on TARGET (2, 5, 9) | New requests do not need `WorkFlow\WF*New.aspx` wrappers |
| **11** | Payroll | **Inquiry first** (payslip already in 3), then termination/vacation-pay forms, then **batches last** (`PayrollCalculation` query-string proc dispatch). Dual-run + characterization mandatory. | Waves 2-5, 10 | Pilot BG period-close on TARGET or documented wrap |
| **12** | Integrations | HITSAPIs employee export to Hitshcm.Api; NasAX; Muqeem; IBM; DNAServices workers stay SQL but config/auth via TARGET | Wave 1 + owning domain | No second employee-export stack |
| **13** | IoT | Assets/visitors/heatmaps -- PRD Out for *program MVP*, still on the map so it is not forgotten | Wave 3 employee id | Optional / later |
| **14** | Reports | Keep SSRS; wrap filter UI per already-strangled domain; then report hub | Per-domain waves | Agenda Reports iframe not required for migrated domains |
| **15** | NasSetup | Org, pay codes, groups, calendars, security groups -- **last of core**. Config as APIs the earlier slices already needed as *read* | Almost everything | Admin no longer lives in 639 aspx |
| **16** | Mode B hub | Replace `Default.aspx` / Agenda (UX-3). Single OIDC login. Encrypted `?value=` only for leftover aspx | Enough journeys that Agenda is a launcher of leftovers | TARGET is the product chrome |
| **17** | Drain | Remaining Common/DataGrids/WebParts/QUIF/AboutCompany as they lose callers | 16 | Markup count near zero |
| **18** | Decommission | NasDna IIS site off. SQL Membership/Profile ConnectionString off. HitsIntegeration forks off or re-pointed. | 17 + zero critical traffic | Program complete |

**Why this order:** prove tenant+rights+one write journey before touching payroll engines; reuse mobile proc contracts; freeze ERec forks once the platform exists; extract WorkFlow *after* it has TARGET documents to serve; NasSetup last because it is the configuration spine (639 pages).

**D-004:** this plan's default first *capability* slice is **vacation**. Alternate if Mina's commercial goal is fork-freeze: **ERec (wave 6 pulled forward after wave 1)**. NasAI is a BFF demo, not the HCM heart -- do it as wave 8 or a thin parallel spike, not the program's first business cutover.

## 7. SoT folder to wave

| SoT area | Files (aspx+ascx) | Wave | Notes |
|----------|------------------:|------|--------|
| `NasSetup\` | 639 | **15** | Read-models may leak earlier (pay codes for vacation) |
| `Common\` | 245 | **2-11** | Split by form: VacationForm with 2, Payroll* with 11 |
| `Reports\` | 241 | **14** (+ per-slice filters) | SSRS stays |
| `NasBatches\` | 209 | **11 / 15** | Payroll/period with 11; org-chart/Visio late or replace |
| `NasForms\` | 171 | **owning domain** | Not a wave by itself |
| `WorkFlow\` | 145 | **10** (+ wrappers die as 2,5,9 land) | Thin `WF*New` pages die first |
| `NasDataGrids\` | 97 | with domain | Agenda grids |
| `NasWebParts\` | 96 | **3 / 16** | ESS widgets then hub |
| `NasAgenda\` + masters | 49+ | **16** | Mode A host until then |
| `IOT\` | 48 | **13** | PRD Out near-term |
| `SSInquiries\` | 40 | **2-5** | ESS |
| Root (`logon`, Default, ...) | 34 | **1 then 16** | Bridge, then hub |
| `NasAgendaSummaryControls\` | 32 | **3 / 16** | |
| `NasAI\` | 20 | **8** | |
| `ERec\` | 17 | **6** | Forks are full trees -- see 8 |
| `ETraining\` | 16 | **7** | |
| `TimeManagement\` | 14 | **4** | Thick ADO -- characterization heavy |
| `HRKPI\` | 12 | **9** | |
| `AboutCompany\` | 11 | **16 / 17** | |
| `NasMuqeem\` | 12 | **12** | |
| `NasAX\` | 10 | **12** | |
| `EObjectives\` | 10 | **9** | |
| `IBMIntegration\` | 10 | **12** | |
| `Security\` | 4 | **1 / 16** | Password/ack on TARGET |
| `SAML\` | 2 | **1 then retire** | External IdP on OpenIddict |
| `QUIF\` | 2 | **17** | ActiveQueryBuilder -- isolate or drop |
| `AppCode\`, control libs | -- | dissolve | HitsWebPage/NASDataSource to API; HITSCulturedControl to resources |

Payroll has **no folder**; it is NasForms + NasBatches + NasSetup + WF + SS. It is wave **11**, inquiry earlier in **3**.

## 8. Sibling trees (collapse, do not clone)

| Sibling | Fate |
|---------|------|
| `HITSMobileV12` REST | Consume Hitshcm.Api; retire duplicate `WebAPI2*` controllers once parity exists |
| `HITSmobile` (Xamarin) | Follow mobile product plan; no new features on old hub |
| `HITSAPIs` | Merge employee export/import into Hitshcm.Api bounded routes |
| `HITSAI` | Stay separate process; Hitshcm is BFF; rotate hardcoded keys before any cutover |
| `HITSDNAErec*` | No new forks. Wave 6 pilot replaces the reason they exist |
| `HitsIntegeration*` | Re-point batches at Hitshcm.Api; do not modernize the fork as SoT |
| `DNAServices` / `HITSTKService` | Keep as workers; auth/config via TARGET; TK WCF behind API in wave 4 |
| `HitsLMS` | Wave 7 gateway |
| `HITSNasDnaV12.1`, `Z*` snapshots | Compare only |
| `ZHITSTalentSM01` | Out of SoT; do not merge unless Mina expands scope |

## 9. Parallel tracks (every wave)

| Track | Always | This program |
|-------|--------|----------------|
| **Capability** | Adapter + API + cutover | Waves 2-15 |
| **UX (D-012)** | DoD on every screen | UX-1 in wave 1; UX-2 every slice; UX-3 = wave 16 |
| **Identity** | OpenIddict BFF | AUTH-2/3/4 in wave 1; Entra/Okta later on same gateway |
| **Data** | DNACloudDB SoR | dacpac in CI; per-slice proc map; `hitsstore` still missing -- find before store/media |
| **Characterization** | Before writes | Harness: login, employee master, one payroll path (PRD) + per-slice |
| **Ops** | New host only first | OpenTelemetry, CI for Hitshcm.sln, IIS side-by-side, Azure-ready |

## 10. Explicitly not a wave-0 rewrite

- All 1,573 aspx in one program increment
- In-place .NET upgrade of Web Forms
- Porting `App_Themes` / `_ar` twins
- Replacing payroll/statutory engines before characterization
- Sharing InProc Session with Core
- Greenfield React/Next
- NasSetup-first because it is 40% of pages

## 11. Program done when

| Metric (PRD direction) | Gate |
|------------------------|------|
| Critical regression on touched flows | Characterization + slice tests green |
| Time to ship in a strangled module | Weeks, not "open the 2,300-line aspx" |
| % journeys on new stack | Mode B only after a *majority of daily ESS/MSS* is TARGET |
| Build/deploy | Hitshcm CI predictable; legacy CI untouched until drain |
| Forks | Zero new full-tree ERec/NasDna copies |
| Decommission | NasDna site off; no Profile.ConnectionString |

## 12. Next 90 days (Wave 1 -- executable)

Do **not** start wave 2 UI until AUTH-2 can open SQL.

1. **AUTH-2** -- Live DNACloudDB from `ITenantConnectionFactory` (AUTH-1b already has the named-options stub and claims; Key Vault later). No conn string in cookies.
2. **Characterization harness** -- scripted/manual: `logon.aspx` to Default to one employee master to one payroll inquiry path. Record as baseline.
3. **UX-1** -- list/filter, detail form, modal, picker, empty/loading/error on the AUTH-1 host.
4. **AUTH-4** -- rights service wrapping `UserGroupRights` (read).
5. **INV-SESS-002 / tenant matrix** -- Profile/BG to database map for environments you will test.
6. **AUTH-3** -- Mode A session-exchange spike (shared host/domain). Can overlap 4-5.
7. **D-004 lock** -- confirm vacation vs ERec-first with Mina.
8. **INV-DD-003** -- TableName usage for the chosen first slice.

Then wave 2: vacation vertical slice (UI to API to `Insert_Empvacat` / vac functions to confirmation), Mode A launch, mobile contract aligned.

## 13. Open decisions (Mina)

| ID | Question | Plan default if unanswered |
|----|----------|----------------------------|
| **D-004** | First capability slice? | Vacation (wave 2) |
| Hosting year-1 | On-prem IIS only vs Azure App Service for Hitshcm? | Side-by-side IIS, Azure-ready |
| Mobile | Collapse HITSRestServicesV12 in wave 2 or after ESS cluster? | Same API in wave 2, old controllers facade |
| ERec forks | Must one paying client move in year 1? | If yes, pull wave 6 forward after wave 1 |
| Payroll | Dual-run duration for period-close? | TBD at wave 11 characterization |

## 14. Related

- CURRENT: [ARCHITECTURE.md](../../ARCHITECTURE.md), [current-state.md](current-state.md)
- TARGET: [target-state.md](target-state.md), [target-ux.md](target-ux.md), [target-session.md](target-session.md)
- Scan notes: inventory `06`/`07`; SoT deep-scan 2026-09-21
- Tasks: [TASKS.md](../../TASKS.md)
