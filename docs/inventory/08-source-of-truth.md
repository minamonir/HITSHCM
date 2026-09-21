# 08 - Source of truth (revised)

Updated: 2026-09-21 (Mina: **HITSNasDna is the last/current code folder**)

## Decision

**Source of truth for HITSHCM modernization: `D:\Workspaces\HITSNasDnaFS\HITSNasDna`**

Earlier inventory wrongly treated this tree as "sparse." Re-scan shows a full product solution.

## Evidence (re-check)

| Tree | Files | .aspx | Framework | Assembly branding | Notes |
|---|---:|---:|---|---|---|
| **HITSNasDna** (SoT) | ~8479 | **1573** | net48 | **HITSDNA 2020.01** (`2020.01.2024.0422`) | Full `HITSNasDna.sln`, libs, Azure cloud variants, `.vs` opened today |
| HITSNasDnaV12.1 | ~8228 | 1559 | net48 | HITS V12 (`12.9727.9727.27000`) | Parallel V12-branded line; not SoT per Mina |
| HitsIntegerationV12 | ~3766 | 234 | - | - | Integration fork |
| Z202301Gov | ~7905 | 1490 | net46-ish | HITSDNA 2020.01 era snapshot | Gov snapshot / archive |

### HITSNasDna solution shape

- `HITSNasDna.sln`
- Web app: `NasDna\NasDna.vbproj` (VB.NET Web Forms, **v4.8**)
- Supporting: HITSCulturedControl, HITSControlLibrary, HITSBOInterface, HITSAXInterface, CustomProviders, CustomViewStateProviders, HITSEncryptQS, RTFHTMLConvert, DevDocuments
- Deploy: `NasDna.Azure`, multiple `NasDnaHTTPS*.Azure`, `NasDna.hitsdnalite`, `AzureCloudService1`

### Top NasDna areas by .aspx count

NasSetup 639, NasBatches 209, NasForms 167, WorkFlow 145, Reports 69, Common 54, IOT 48, SSInquiries 40, NasAI 20, ERec 17, ETraining 16, TimeManagement 14, ...

### Same DB catalogs (Web.config, Integrated Security, server fz-dv-db01)

ASPNETDB, DNACloudDB, DNACloudDBBG, hitsstore

## Implications

1. Phase 1 planning points at **`HITSNasDna\NasDna`**, not V12.1.
2. V12.1 remains useful as a compare/diff line only.
3. Strangler candidates still apply; paths should use HITSNasDna.
4. Assembly title "HITSDNA 2020.01" is branding; folder SoT is what matters for code.

## Pending (optional)

- Diff HITSNasDna vs HITSNasDnaV12.1 for unique modules
- DB dacpac still required (unchanged gap)
