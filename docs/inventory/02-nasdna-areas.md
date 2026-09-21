# 02 — NasDna areas

Primary: `D:\Workspaces\HITSNasDnaFS\HITSNasDnaV12.1\NasDna`  
Totals: **~1559 `.aspx`** site-wide (34 at NasDna root). Counts are recursive under each folder.

## Functional / feature areas

| Folder | Purpose guess | .aspx | .ascx | .vb/.cs | Files |
|--------|---------------|------:|------:|--------:|------:|
| AboutCompany | Company/org profile pages | 11 | 0 | 12 | 23 |
| ERec | E-recruitment (applicants, shortlists, interviews) | 17 | 0 | 20 | 39 |
| ETraining | Training attendance, appraisal, activities | 16 | 0 | 20 | 40 |
| TimeManagement | Attendance sheets, shifts, time sheets | 14 | 0 | 14 | 28 |
| NasForms | Dynamic/custom forms UI (large surface) | 165 | 4 | 178 | 349 |
| WorkFlow | Workflow/action queues and approvals | 135 | 0 | 142 | 281 |
| Reports | Report pages + many report user controls | 69 | 172 | 246 | 490 |
| Security | Password change / first-logon ack | 4 | 0 | 6 | 12 |
| NasSetup | Massive setup/config admin UI | 639 | 0 | 655 | 1302 |
| NasBatches | Batch job / batch UI screens | 207 | 0 | 216 | 425 |
| NasAI | In-app AI / policy ask / appraisal AI screens | 20 | 0 | 24 | 46 |
| NasAX | Dynamics AX company/org/sync setup | 10 | 0 | 12 | 24 |
| NasAgenda | Agenda UI + summary controls sibling | 13 | 36 | 66 | 131 |
| NasAgendaSummaryControls | Agenda summary ASCX library | 0 | 32 | 32 | 64 |
| NasMuqeem | Saudi Muqeem (ELM) residency integration UI | 10 | 2 | 12 | 24 |
| HRKPI | HR KPI / budget / period close | 12 | 0 | 12 | 24 |
| EObjectives | Objectives / categories / performance goals | 10 | 0 | 16 | 32 |
| IOT | IoT assets, visitors, gateways, heatmaps | 48 | 0 | 54 | 107 |
| IOTHeatMap | Heatmap DLL wrapper (`DrawHeatMap.dll`) | 0 | 0 | 0 | 1 |
| SSInquiries | Self-service inquiries | 40 | 0 | 44 | 86 |
| QUIF | Query UI framework assets (few pages, many files) | 2 | 0 | 3 | 385 |
| Common | Shared pages + large ASCX library | 54 | 189 | 273 | 524 |
| NasWebParts | Web Parts / personalization surfaces | 4 | 92 | 106 | 205 |
| NasDataGrids | Shared data grid ASCX components | 14 | 79 | 98 | 194 |
| IBMIntegration | IBM integration UI | 8 | 2 | 12 | 22 |
| SAML | SAML ACS / SLO endpoints | 2 | 0 | 4 | 6 |
| Batches | Thin batch entry (legacy?) | 1 | 0 | 2 | 4 |

## Infrastructure / support folders (low or no ASPX)

| Folder | Purpose guess | Notes |
|--------|---------------|-------|
| AppCode | Shared VB helpers, master page, LINQ-to-SQL `NasDB`, `NASDataSource` | Core coupling hub |
| DataSets | Typed DataSets (XSD/Designer) for attendance, store, KPI, training, etc. | Classic ADO.NET DAL |
| Models | Identity/EF models (`ApplicationDbContext`, ADAL token cache) | Thin EF6 island |
| CryptClass | Local crypto DLL (`HITSCryptography.dll`) | HintPath local |
| HITSIBMClient | IBM client DLL | HintPath local |
| MuqeemDLL | ELM Muqeem WCF adapters / entities | HintPath local |
| OpenXMLSDK | Open XML document tooling | Docs/exports |
| reportviewer2015 | ReportViewer assets | Reporting |
| App_Themes / Styles / css / Content / js / Scripts / ChartJS / Images | Static UI assets | — |
| App_Start | Startup/OWIN wiring (if used) | Auth pipeline |
| Web References | SSRS: RSServer, RS2008, RSServerAzure | SOAP report admin |
| Service References | SqlAzure ImportExport | Azure DB import/export |
| AjaxBin / Certificates / OrgScripts / Properties / My Project | Framework/support | — |

## Sample page evidence (extraction-friendly modules)

- **ERec:** `ApplicantChanges.aspx`, `CreateShortList.aspx`, `Interviews.aspx` (+ `_ar` locales)
- **ETraining:** `TrainingMain.aspx`, `TrainingAttendance.aspx`, `TrainingAppraisalManage.aspx`
- **TimeManagement:** `AttendanceSheet.aspx`, `AttendanceShifts.aspx`, `TimeShiftSheet.aspx`
- **NasAI:** `AI.aspx`, `AskPolicy.aspx`, `AppraisalForm.aspx`
- **IOT:** `AssetAssignment.aspx`, `Gateway.aspx`, `GenerateHeatMap.aspx`
- **SAML:** `AssertionConsumerService.aspx`, `SLOService.aspx`

## Density takeaway

- **NasSetup + NasBatches + NasForms + WorkFlow** dominate page count → high coupling / late stranglers.
- Smaller verticals (**ERec, ETraining, TimeManagement, NasAI, IOT, NasMuqeem, HRKPI, EObjectives**) are better first-slice candidates by surface area alone.
