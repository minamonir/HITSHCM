# 09 - DB inventory

**Generated:** 2026-09-21 (Africa/Cairo)  
**Prefer tree:** `D:\Workspaces\HITSNasDnaFS\HITSNasDna\NasDna` (SoT; V12.1 for diffs only) (+ workspace `DevDocuments`, Gov `DBDiff`)  
**Secrets:** passwords / ClientSecret / keys redacted to `***`. Never copy live secrets.

## Named databases / catalogs referenced

### Primary (V12.1 `Web.config` connectionStrings)

| Connection name | Initial Catalog | Data Source (dev sample) | Auth |
|-----------------|-----------------|--------------------------|------|
| LocalSqlServer | **ASPNETDB** | fz-dv-db01 | Integrated Security=True |
| DefaultConnection | **ASPNETDB** | fz-dv-db01 | Integrated |
| NasDotNetDevConnectionString | **DNACloudDB** | fz-dv-db01 | Integrated |
| BGSetupConnectionString | **DNACloudDBBG** | fz-dv-db01 | Integrated |
| ViewStateConnectionString | **ASPNETDB** | fz-dv-db01 | Integrated |
| StoreConnectionString | **hitsstore** | fz-dv-db01 | Integrated |

### Integration fork difference

`HitsIntegerationV12\NasDna\Web.config` uses **ASPNETDBHITSIntegration** (instead of ASPNETDB) for membership/viewstate; still points business data at **DNACloudDB** / **DNACloudDBBG** / **hitsstore**.

### Also seen across configs / scripts / docs (workspace-wide)

| Catalog (examples) | Context |
|--------------------|---------|
| DNACloudDB, DNACloudDBBG | Core HCM + business-group setup |
| ASPNETDB, ASPNETDB2, ASPNETDBHITSIntegration | Membership / providers / viewstate |
| hitsstore | Store connection |
| hitssystemsBG, HitsCloudBG, DNACloudDB202001BG, securehits*BG | BG / env variants in scripts |
| DNACloudDBBatchTemp, DNACLOUDDB201909ETAXBatchTemp, … | Integration batch temp DBs (DevDocuments scripts) |
| DNACloudDB201701 / 201909 / 202001 / 202301 | Versioned / snapshot DB names in scripts |
| MyReleaseDB, NasDotNetDev, Cl000155*, Fairmont, HITSETalentDb, HITSTraining, HITSSmartID, LocalTKDB, ZZTKDB | Tenant/legacy/tooling names in older configs/scripts |

EF sibling evidence: `HITSAPIs\Models\DNAEntities.edmx` container **DNACloudDBEntities**; mobile `DNAModel.edmx` / `DNABGS.edmx` (Azure SQL provider token).

## How apps connect

- **Dominant auth in V12.1 / Integration samples:** Windows **Integrated Security** (no SQL password in these `connectionStrings`).
- **Runtime routing:** `NASDataSource` → LINQ-to-SQL `NasDBDataContext` constructed with **`Profile("ConnectionString")`** (~260 Profile hits; ~2254 `NASDataSource` hits under V12.1 NasDna). Per-user/profile may override static Web.config catalogs — full tenant map **not** in-repo.
- **`ConfigurationManager.ConnectionStrings[...]`:** essentially unused in VB/CS under NasDna (0 hits); Profile + named string constants dominate.
- Do **not** assume prod uses fz-dv-db01; treat as non-prod inventory sample.

## Schema artifacts in-repo

| Artifact | Location | Notes |
|----------|----------|-------|
| **LINQ-to-SQL DBML** | `HITSNasDnaV12.1\NasDna\AppCode\NasDB.dbml` (~1.2 MB) | **~500 `<Table Name=`** mappings (dbo.*); paired `NasDB1.designer.vb` (~7.3 MB) |
| Same DBML copies | ERec client trees, Integration, Z* snapshots, V12.1 | Forked copies — prefer **HITSNasDna** SoT |
| **Typed DataSets (.xsd)** | `NasDna\DataSets\` (14 app XSDs) | Prefix/group: AttendaceSheetDS, EmployeesIDDS, ETrainingAttendance, FilterDS, GradeSalaries, HITSMedia, HITSStore, HRKPIDS, IOTEmployeesUDDS, NasDS, SSAppraisalDS, SSBEntry, TrainingEvalDS, UserLogonDS (+ 3 SqlAzure ImportExport XSDs under Service References) |
| **EF EDMX** | `HITSAPIs\Models\DNAEntities.edmx`; Mobile `DNAModel.edmx`, `DNABGS.edmx` | API/mobile slice of DNA schema — not full NasDna |
| **EF Identity island** | `Models\ApplicationDbContext.vb`, ADAL cache | Thin; main HCM path is L2S/ADO |
| **App_Data *.mdf** | None found under V12.1 NasDna | No local attach DB |
| **NasDna\Scripts\** | JS libs (jquery, powerbi, …) | **Not** SQL scripts |

## `*.sql` inventory (workspace)

| Location | Count | Role |
|----------|------:|------|
| **Total under HITSNasDnaFS** | **29** | Sparse — no full schema dump |
| `DevDocuments\...` | 18 | Mobile/EasyGO V12 scripts, AI script, self-service upgrade |
| `Z202301Gov\DevDocuments\` | 10 | `PrepareDatabaseForReplica.sql` + `DBDiff\2014xxxx–2015xxxx.sql` (old diffs) |
| `HITSAI` | 1 | AI-related |

### Sample filenames (10)

1. `DevDocuments\...\Hits AI\AI Final Script.sql`
2. `...\HITS EasyGO Mobile Scripts(2020)\0.Prerequisites_V202001.sql`
3. `...\1.Tables_Data.sql`
4. `...\2.Procedures_Functions_V202001_V201701.sql`
5. `...\3.Notifications_V202001.sql`
6. `...\4.MobileMFA_V202001.sql`
7. `...\5.HITSSystemsBG.sql`
8. `...\HitsEasyGo V12\1.V12_MobileScript_202001&Above.sql`
9. `...\HitsEasyGo V12\3.V12_DataDictionary_All.sql`
10. `Z202301Gov\DevDocuments\PrepareDatabaseForReplica.sql`

Also large **non-.sql** script docs: `DevDocuments\...\Hits Integration Portal Scripts\Integration Script 201909.txt` (heavy DNACloudDBBatchTemp commentary).

## Docs mentioning DNACloudDB / ASPNETDB / hitsstore

- Primary living references: **Web.config** connectionStrings (V12.1 + peers).
- Integration portal script `.txt` files under DevDocuments repeatedly mention DNACloudDB* / BatchTemp catalogs.
- Inventory pack `04-data-touchpoints.md` already lists connection **names** (aligned with this doc).

## Schema in-repo vs out-of-tree gaps

| Have | Missing / out-of-tree |
|------|------------------------|
| L2S table **map** (~500 tables in DBML) | Live column-level / index / FK truth from SQL Server |
| Partial mobile/EasyGO **procedure** scripts | Full DNACloudDB stored-proc library for monolith |
| Old Gov **DBDiff** (2014–2015) | Current migration chain to 12.9727 |
| Catalog **names** | Per-tenant Profile connectionString values |
| EDMX for API/mobile subset | Complete SSRS RDL data sources / report DB objects |

**Was biggest gap:** no dacpac. **Resolved 2026-09-21** — see Live extract section below. Remaining: sibling catalogs, Profile tenant map, broken-proc review.

## Recommended next steps (real schema)

1. **Done:** DNACloudDB dacpac extracted to `docs/inventory/db/DNACloudDB.dacpac`.
2. Optional: extract `DNACloudDBBG` + `aspnetdb`; locate `hitsstore` (missing on fz-dv-db01 under that name).
3. Parse `NasDB.dbml` vs live 805 tables → missing-object diff.
4. Sample Profile(`"ConnectionString"`) from non-prod membership/profile store for multi-tenant catalogs.
5. Review/fix `[dbo].[hits_EmpHistory]` and `[dbo].[hits_GetBudgets]` syntax errors flagged by SqlPackage.
6. Lock Phase 1 data-access strategy against exported schema + SoT `HITSNasDna`.
## Live extract (2026-09-21) — DNACloudDB dacpac

**Server:** `FZ-DV-DB01` — Microsoft SQL Server **2025** (17.0.4075.5) Enterprise Developer  
**Auth used:** Integrated Security (Windows) via Mina's machine  
**Tool:** `microsoft.sqlpackage` **170.5.96** (`dotnet tool install -g microsoft.sqlpackage`)  
Note: built-in DAC **130** SqlPackage failed (`compatibility level '17' is not within … 80 to 130`).

### Artifact

| File | Size | Notes |
|------|-----:|-------|
| `docs/inventory/db/DNACloudDB.dacpac` | ~9.1 MB (9,520,067 bytes) | Schema only (`ExtractAllTableData=False`) |

Extract completed successfully in ~4 min. SqlPackage reported validation errors on two objects (package still written):

- `[dbo].[hits_EmpHistory]` — Incorrect syntax near `FROM`
- `[dbo].[hits_GetBudgets]` — Incorrect syntax near `FROM`

Treat those two procs as **suspect / broken in model** until reviewed in SSMS.

### Live object counts (`DNACloudDB`)

| Metric | Count |
|--------|------:|
| Base tables | **805** |
| Views | **368** |
| User stored procedures | **1804** |
| Compatibility level | **140** |
| Approx DB size | **~30.8 GB** |

Schemas (table ownership):

| Schema | Tables |
|--------|-------:|
| dbo | 555 |
| XWB | 249 |
| XRP | 1 |

Compare: in-repo `NasDB.dbml` maps ~500 tables — live DB has **805** base tables (DBML is a partial façade).

### Sibling catalogs on same server (spot-check)

| Catalog | Online | Compat | Approx size |
|---------|--------|-------:|------------:|
| DNACloudDB | yes | 140 | ~30.8 GB |
| DNACloudDBBG | yes | 100 | ~9.4 MB |
| aspnetdb | yes | 170 | ~400 MB |
| hitsstore | **not found** on this instance under that name | — | — |

### Reproduce

```powershell
$sqlpkg = "$env:USERPROFILE\.dotnet\tools\sqlpackage.exe"
$cs = 'Data Source=fz-dv-db01;Initial Catalog=DNACloudDB;Integrated Security=True;TrustServerCertificate=True'
& $sqlpkg /Action:Extract /SourceConnectionString:$cs `
  /TargetFile:'C:\Users\mina\Projects\HITSHCM\docs\inventory\db\DNACloudDB.dacpac' `
  /p:ExtractAllTableData=False
```

Optional next extracts: `DNACloudDBBG`, `aspnetdb` (and locate `hitsstore` / tenant Profile catalogs).
## dbo-only focus (2026-09-21)

**SqlPackage:** microsoft.sqlpackage **170.5.96** (dotnet tool) — upgraded past DAC 130; required for SQL Server 2025 / model extract.

### Artifacts

| Artifact | Path | Notes |
|----------|------|-------|
| **Canonical package (D-008)** | docs/inventory/db/DNACloudDB.dacpac | All schemas; ~9.1 MB; refreshed with 170.5.96 |
| dbo script tree | docs/inventory/db/DNACloudDB-schema/dbo/ | **3,127** .sql files (SchemaObjectType extract; non-dbo folders removed) |
| dbo scripts zip | docs/inventory/db/DNACloudDB-dbo-scripts.zip | ~7.5 MB browseable zip of dbo tree |
| dbo table list | docs/inventory/db/DNACloudDB-dbo-tables.txt | Live dbo base tables (554) |

### Live dbo object counts (fz-dv-db01 / DNACloudDB)

| Type | Count |
|------|------:|
| USER_TABLE | 554 |
| VIEW | 368 |
| SQL_STORED_PROCEDURE | 1804 |
| SQL_SCALAR_FUNCTION | 377 |
| SQL_TABLE_VALUED_FUNCTION | 23 |
| SQL_INLINE_TABLE_VALUED_FUNCTION | 7 |
| SQL_TRIGGER | 1952 |
| DEFAULT_CONSTRAINT | 5516 |
| FOREIGN_KEY_CONSTRAINT | 713 |
| PRIMARY_KEY_CONSTRAINT | 542 |
| CLR_SCALAR_FUNCTION | 1 |

Non-dbo schemas on this DB (**XWB**, **XRP**) were excluded from the script tree. Full dacpac still contains them if needed for cross-schema refs.

### Why no DNACloudDB-dbo.dacpac

DacFx model rebuild of dbo-only fails on SQL71501 (ambiguous refs from views/computed columns to UDFs). PackageOptions.IgnoreValidationErrors does not suppress these on BuildPackage in 170.5.96. **Use the dbo script tree / zip for dbo-focused inventory**; use the full dacpac when a deployable package is required.

Known extract warnings (unchanged): [dbo].[hits_EmpHistory], [dbo].[hits_GetBudgets] — incorrect syntax near FROM.
## Sibling extracts (2026-09-21) — DNACloudDBBG + aspnetdb

Same server z-dv-db01, SqlPackage **170.5.96**, schema-only (D-008 style — full dacpac each).

| Catalog | Dacpac | Size | Compat | Tables | Views | Procs | Notes |
|---------|--------|-----:|-------:|-------:|------:|------:|-------|
| **DNACloudDBBG** | docs/inventory/db/DNACloudDBBG.dacpac | ~8 KB | 100 | 8 | 0 | 8 | BG setup; all dbo. Object list: DNACloudDBBG-objects.txt |
| **aspnetdb** | docs/inventory/db/aspnetdb.dacpac | ~34 KB | 170 | 17 | 9 | 55 | ASP.NET membership/roles/profile (spnet_*) + alias tables (Users, Roles, …). DB file ~400 MB (data); schema package tiny. |
| hitsstore | — | — | — | — | — | — | **Not present** on fz-dv-db01 under that name |

Live sizes on instance: DNACloudDBBG **9.4 MB**, aspnetdb **400 MB** (data-heavy membership store).

