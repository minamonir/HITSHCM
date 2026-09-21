# 05 — Siblings (mobile / API / integration / AI / client-ERec)

How related trees under `D:\Workspaces\HITSNasDnaFS` sit around primary `HITSNasDnaV12.1\NasDna`.

## Primary vs older packaging

| Sibling | Relation |
|---------|----------|
| **HITSNasDnaV12.1** | Current primary solution (NasDna + BO/AX/control/providers) |
| HITSNasDna | Older/smaller packaging (EncryptQS + NasDna) |
| Z201701 … Z202406, Z202301Gov | Dated **snapshots / forks** of NasDna+libs (Gov snapshot is huge) |

## Mobile

| Sibling | Relation |
|---------|----------|
| HITSmobile | Legacy: HITSDNA + HITSNHubServer + IOTHITSRestServices |
| HITSMobileV12 | Current-ish mobile: HITSDNAV12 client + **HITSRestServicesV12** REST surface |

**Implication:** Mobile already talks over REST siblings — good strangler edge for API-first slices (reuse/extend HITSMobileV12 / HITSAPIs rather than scraping Web Forms).

## API / services

| Sibling | Relation |
|---------|----------|
| HITSAPIs | Small C# API: EmployeeData / GetEmployeeInformation / InsertEmployeesData / SendEmployeeData controllers |
| DNAServices | Windows services: AX sync, backup, BG users sync, clean, emails, mobile users, deactivate users |
| DNADigitalSignature | Digital signature web service + library |
| DNATimeClient2 | Time client utility |
| HITSTKService | Timekeeping service |
| HITSBackupApi | Backup API placeholder |
| HitsLMS | LMS API + console (learning adjacent to ETraining) |

## Integration

| Sibling | Relation |
|---------|----------|
| HitsIntegeration | Older integration embedding NasDna + HITSBOInterface + control libs |
| HitsIntegerationV12 | V12 integration solution (AX/BO/providers/NasDna) — parallel to HITSNasDnaV12.1 |
| ZHitsIntegeration | Z-lineage integration solution (+ DevDocuments) |

**Implication:** Integration is often a **second copy of NasDna inside another .sln**, not a clean bounded context. Prefer extracting shared contracts (BO/AX) before forking more NasDna trees.

## AI

| Sibling | Relation |
|---------|----------|
| HITSAI | Sidecar: AIDocSearch (~199 files), HITSAIDataChat (~90), HitsAIApis (thin) |
| NasDna\NasAI | In-monolith AI pages (AskPolicy, AppraisalForm, AI*.aspx) |

**Implication:** AI already has an external tree — candidate to **unify** NasAI UI behind HITSAI APIs (strangler via BFF).

## Client ERec forks

| Sibling | Relation |
|---------|----------|
| HITSDNAErecBaitElZakah | Full client solution (NasDna + Azure + providers + controls) |
| HITSDNAErecFaisalIslamicBank | Same pattern |
| HITSDNAErecPalmHills | Same pattern |
| HITSDNAErecCleopatraNew | Smaller NasDna-only variant |

**Implication:** E-recruitment is already productized per-customer via **copy/fork**. That both validates ERec as a product boundary **and** raises merge-debt risk — strangler should introduce a shared ERec service/API so new clients stop cloning NasDna.

## Reports / other

| Sibling | Relation |
|---------|----------|
| HITSReports | Arabic/English reports + alerts + skills reports packages |
| NasDna\Reports + Web References RS* | In-app ReportViewer + SSRS admin |
| NasAudit / BoNBE / DevDocuments | Small utilities / docs |

## Relationship diagram (logical)

```
                    ┌──────────── HITSAI ────────────┐
                    │  AIDocSearch / DataChat / APIs │
                    └──────────────▲─────────────────┘
                                   │ (should consume)
┌──────────────┐    ┌──────────────┴──────────────┐    ┌─────────────────┐
│ HITSMobileV12│───▶│  HITSNasDnaV12.1 / NasDna   │◀───│ Client ERec forks│
│ REST + client│    │  Web Forms monolith         │    │ (copy/fork)      │
└──────┬───────┘    └──────────────┬──────────────┘    └─────────────────┘
       │                           │
       ▼                           ▼
┌──────────────┐    ┌──────────────────────────────┐
│   HITSAPIs   │    │ HitsIntegerationV12 / DNA*   │
│ employee CRUD│    │ BO/AX/services/background    │
└──────────────┘    └──────────────────────────────┘
```
