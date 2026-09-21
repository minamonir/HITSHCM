# 04 — Data touchpoints

Source: `D:\Workspaces\HITSNasDnaFS\HITSNasDnaV12.1\NasDna\Web.config` (+ code patterns under AppCode/DataSets/Models).  
**Secrets redacted** (`***`). Do not copy live secrets into tickets/repos.

## Connection string names

`<connectionStrings>` is present. Dev box samples use **Integrated Security** (no SQL password in these entries). Server/catalog names left for inventory; treat as non-prod.

| Name | Provider | Redacted connection summary |
|------|----------|-----------------------------|
| LocalSqlServer | System.Data.SqlClient | Data Source=`fz-dv-db01`; Initial Catalog=`ASPNETDB`; Integrated Security=True; MultipleActiveResultSets=True |
| DefaultConnection | System.Data.SqlClient | Data Source=`fz-dv-db01`; Initial Catalog=`ASPNETDB`; Integrated Security=True; MultipleActiveResultSets=True |
| NasDotNetDevConnectionString | System.Data.SqlClient | Data Source=`fz-dv-db01`; Initial Catalog=`DNACloudDB`; Persist Security Info=True; Integrated Security=True |
| BGSetupConnectionString | System.Data.SqlClient | Data Source=`fz-dv-db01`; Initial Catalog=`DNACloudDBBG`; Persist Security Info=True; Integrated Security=True |
| ViewStateConnectionString | System.Data.SqlClient | Data Source=`fz-dv-db01`; Initial Catalog=`ASPNETDB`; Persist Security Info=True; Integrated Security=True |
| StoreConnectionString | System.Data.SqlClient | Data Source=`fz-dv-db01`; Initial Catalog=`hitsstore`; Persist Security Info=True; Integrated Security=True |

Also: `<remove name="LocalSqlServer" />` then re-add (override machine.config default).

## Providers bound to connection names

- **SqlViewStateProvider** → `ViewStateConnectionString` (CustomViewStateProviders)
- **DNAPersonalizationProvider** (SqlPersonalizationProvider) → `DefaultConnection` (app `/hragentic`)
- **DefaultProfileProvider / DefaultMembershipProvider / DefaultRoleProvider** → `DefaultConnection`
- **DefaultSessionProvider** (System.Web.Providers) → session section (InProc currently)

## Profile / runtime connection pattern

- `AppCode\NASDataSource` inherits LINQ-to-SQL `NasDBDataContext` and constructs with **`Profile("ConnectionString")`** — per-user/profile DB routing, not only Web.config static strings.
- Widespread `SqlConnection` / `SqlCommand` usage in sampled areas; `NasDotNetDevConnectionString` referenced in code samples.

## EF / DataSets / DAL patterns

| Pattern | Evidence | Coupling note |
|---------|----------|---------------|
| **Typed DataSets** | `DataSets\*.xsd` (+ Designer): AttendaceSheetDS, EmployeesIDDS, ETrainingAttendance, FilterDS, GradeSalaries, HITSMedia, HITSStore, HRKPIDS, IOTEmployeesUDDS, NasDS, SSAppraisalDS, SSBEntry, TrainingEvalDS, UserLogonDS | Classic TableAdapter DAL; high codegen volume |
| **LINQ to SQL** | `AppCode\NasDB.dbml`, `NasDB1.designer.vb`, `NASDataSource` | Central DBML; workflow helpers on context |
| **EF6** | packages EntityFramework 6.1.3; `Models\ApplicationDbContext.vb`, `IdentityModels.vb`, `AdalTokenCache.vb`; `<entityFramework>` in Web.config (LocalDbConnectionFactory default) | Thin Identity/ADAL island vs main HCM data path |
| **Enterprise Library Data** | `Microsoft.Practices.EnterpriseLibrary.Data` reference | Legacy block DAL alongside raw ADO.NET |
| **Raw ADO.NET** | Heavy `SqlConnection`/`SqlCommand` hit counts in sampled folders | Dominant access style for pages |
| **SSRS Web References** | RSServer / RSServer2008 / RSServerAzure | Report deployment/admin |
| **Azure SQL ImportExport** | Service Reference `SqlAzure.ImportExport` | Cloud DB move tooling |

## Auth / cloud secrets in appSettings (names only — values redacted)

Web.config contains Okta-related keys (SAML PartnerIdP + OIDC). **Values must not be committed or shared:**

- `Enableokta`, `OKTABtn`, `PartnerIdP`
- `okta:ClientId`, `okta:ClientSecret`=**\*\*\***, `okta:OrgUri`, `okta:RedirectUri`, `okta:PostLogoutRedirectUri`

Redis custom session provider exists **commented out** (`HITSRedisSessionStateProvider` host/port/accessKey); live session is **InProc**.

## Blockers / gaps for inventory

- No live `bin\` folder sampled on this machine (refs come from packages + local HintPath DLLs).
- Per-tenant/profile connection strings are **not** fully enumerated (Profile-driven).
- Stored procedures / DB schema live outside this tree (DNACloudDB / ASPNETDB / hitsstore) — DB inventory still needed.
