# DNACloudDB family — inventory artifacts

**Canonical package (D-008):** `DNACloudDB.dacpac` (full schema, all schemas)

| File | Role | Notes |
|------|------|-------|
| `DNACloudDB.dacpac` | **Canonical** HCM schema | ~9.1 MB; dbo+XWB+XRP; SqlPackage 170.5.96 |
| `DNACloudDBBG.dacpac` | Business-group sibling | ~8 KB; **8** tables / **8** procs / dbo; compat 100 |
| `aspnetdb.dacpac` | Membership / profile / roles | ~34 KB; **17** tables / **9** views / **55** procs; classic aspnet_* + thin aliases; compat 170 |
| `DNACloudDB-schema/dbo/` | Browse-only dbo scripts | Secondary; not a package |
| `DNACloudDB-dbo-scripts.zip` | Zip of dbo scripts | Secondary |
| `DNACloudDB-dbo-tables.txt` | Live dbo table list | 554 tables |
| `DNACloudDBBG-objects.txt` | BG object list | tables + procs |

**Server:** `fz-dv-db01` · Integrated Security · schema-only extracts  
**Not found on this instance:** `hitsstore` (still unresolved)

**Tool:** microsoft.sqlpackage **170.5.96**
