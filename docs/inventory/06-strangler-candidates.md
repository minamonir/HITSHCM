# 06 — Strangler candidates (ranked)

Ranked for **first extraction** from `HITSNasDnaV12.1\NasDna`.  
Criteria: business value, surface size, existing sibling API/fork evidence, coupling to NasSetup/WorkFlow/Common.

| Rank | Module | Why valuable | Coupling risk | Suggested approach | Evidence paths |
|-----:|--------|--------------|---------------|--------------------|----------------|
| 1 | **ERec (e-recruitment)** | Clear product boundary; multiple paying client forks already exist; interviews/shortlist domain is marketable | **Medium** — still shares Common/AppCode/DB profile conn; customer forks diverge | Extract **ERec API + new UI**; freeze fork proliferation; migrate one client (e.g. Cleopatra small tree) as pilot | `NasDna\ERec\` (~17 aspx); `HITSDNAErec*` siblings; sample `Interviews.aspx`, `CreateShortList.aspx` |
| 2 | **TimeManagement (+ TK service)** | Operationally hot; dedicated `HITSTKService` / `DNATimeClient2`; DataSet `AttendaceSheetDS` is localized | **Medium** — attendance often feeds payroll/workflow queues | API-first attendance/shifts service; leave Web Forms as façade then replace mobile/time clients | `NasDna\TimeManagement\`; `DataSets\AttendaceSheetDS.*`; `HITSTKService`; `DNATimeClient2` |
| 3 | **NasAI → HITSAI** | Strategic differentiation; external HITSAI tree already exists; small ASPX surface (~20) | **Low–Medium** — pages may call shared AppCode/FilterAI; auth/session shared | Strangle UI to call HITSAI APIs; retire in-monolith AI code paths | `NasDna\NasAI\`; `AppCode\FilterAI.vb`; `HITSAI\AIDocSearch`, `HITSAIDataChat`, `HitsAIApis` |
| 4 | **ETraining (+ HitsLMS)** | Bounded learning domain; LMS sibling API exists; dedicated DataSets | **Medium** — appraisal/workflow overlap possible | Publish training API (align HitsLMS); keep Web Forms read-only façade initially | `NasDna\ETraining\`; `DataSets\ETrainingAttendance.*`, `TrainingEvalDS.*`; `HitsLMS\` |
| 5 | **IOT** | Distinct ops domain (assets/visitors/gateways/heatmaps); local DLL `DrawHeatMap`; mobile IoT history | **Medium** — employee master data dependency | Extract IoT microservice + device gateway; keep employee ID as anti-corruption link | `NasDna\IOT\` (~48 aspx); `IOTHeatMap\DrawHeatMap.dll`; `DataSets\IOTEmployeesUDDS.*`; legacy `HITSmobile\IOTHITSRestServices` |
| 6 | **HITSAPIs employee façade (expand)** | Already a thin C# API for employee CRUD/info — natural **strangler seam** for all modules | **High systemic value / Medium risk** — must not become another god-API without bounded contexts | Grow versioned **HCM API gateway** (auth, employee read models) that Web Forms and mobile both call | `HITSAPIs\Controllers\*`; `HITSMobileV12\HITSRestServicesV12` |
| 7 | **NasMuqeem (ELM)** | Regulatory KSA integration; already isolated DLLs (ELM WCF adapters) | **Medium–High** — external ELM contracts + local crypto | Wrap Muqeem as integration service; hide WCF behind modern HTTP | `NasDna\NasMuqeem\`; `MuqeemDLL\*.dll`; vbproj HintPaths |
| 8 | **SAML/Okta edge (auth BFF)** | Centralizes ComponentSpace SAML + Okta OIDC; unlocks any new front-end | **High** — touches every page via session/membership | Extract auth BFF / identity service first-or-parallel; do not rewrite Membership in-place | `NasDna\SAML\`; packages Okta/ComponentSpace/Owin; Web.config okta:* / PartnerIdP (secrets redacted) |

## Explicitly **defer** (high coupling / late)

| Module | Why defer |
|--------|-----------|
| NasSetup (~639 aspx) | Configuration spine of the monolith |
| WorkFlow (~135 aspx) | Cross-cutting queues (see LINQ helpers on NasDB) |
| NasForms (~165 aspx) | Dynamic forms platform used widely |
| NasBatches (~207 aspx) | Batch ops tightly bound to setup/data |
| Reports (+ SSRS refs) | Entangled with ReportViewer + RS web refs + HITSReports packages |
| Common / NasDataGrids / NasWebParts | Shared UI kernel |

## Ranking rationale (short)

Prefer modules that are **already partially outside** the Web Forms host (ERec forks, HITSAI, HitsLMS, TK/mobile REST) and have **modest ASPX counts**, so the first slice proves API-first strangulation without boiling NasSetup/WorkFlow.
