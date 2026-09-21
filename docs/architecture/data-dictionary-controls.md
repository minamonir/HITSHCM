# Data dictionary + control libraries (CURRENT)

**Status:** inventory fact (2026-09-21)  
**SoT:** `D:\Workspaces\HITSNasDnaFS\HITSNasDna`  
**Related decisions:** D-007, D-010, D-011, D-012  
**Why this matters:** NasDna UI is **metadata-driven**. Labels, tooltips, many validators, grid headers, and parts of filter/SQL clause building resolve from `dbo.DataDictionary` via custom Web Forms controls — not hard-coded English strings alone.

---

## 1. Two assemblies (solution projects)

| Project | Path | Role |
|---------|------|------|
| **HITSCulturedControl** | `HITSCulturedControl\HITSCulturedControl.vb` (~252 KB) + `DataDictDS.xsd` | Culture/caption layer: labels, buttons, panels, validators, FormView, DataGrid + typed columns |
| **HITSControlLibrary** | `HITSControlLibrary\HITSControlLibrary.vb` (~961 KB) | Input / filter / data-bound controls: text boxes, dates (Gregorian + Hijri), DDLs, trees, SQL/Linq data sources, RTF, autocomplete helpers |

NasDna references both. Markup uses prefixes such as `HitsCC:` (see ascx samples under `NasDna\Common\`).

---

## 2. DataDictionary (SQL SoR for UI captions)

### Table (live DNACloudDB snapshot)

- **~37,998 rows**, covering **~4,116 distinct `TableName` values**
- Composite key conceptually: **`TableName` + `FieldName`** (controls look up with uppercase trim)
- Multilingual columns (among others):
  - Captions: `Caption` (EN), `ACaption` (AR), `FCaption` (FR)
  - Descriptions: `Description`, `ADescription`, `FDescription`
  - Also: `DataType` / field typing, audit / AX flags, lookup-related fields, etc.

> Inventory note: typed procs such as `GetDataDictionary` exist; runtime load prefers **`GetOrgDataDictionary`** (org-scoped) with a **direct SELECT fallback** that maps language columns into a single `Caption`/`Description` pair for the in-memory table.

### Related procs / entry points (non-exhaustive)

| Name | Use |
|------|-----|
| `GetOrgDataDictionary` | Primary fill used by `CommonLib.LoadDC` |
| `GetDataDictionary` | Generic get by table/field/lang |
| `WebAPI2GetDataDictionary` | API surface |
| `hits_GenDataDictionary` | Generation / maintenance |
| `Report_GetDataDictionary` | Reporting / autocomplete paths |
| `Pin_GetDataDictionary` | Pinning feature |

Profile also exposes `UserDC`-style usage in places; session remains the hot path for page controls.

---

## 3. Session cache: `Session("dc")`

**Loader:** `NasDna\AppCode\CommonLib.vb` → `LoadDC(PageName)`

Flow:

1. Build `HITSCulturedControl.DataDictDS`
2. Prefer `EXEC [GetOrgDataDictionary] @LogonName, @date, @Lang`
3. On failure, fallback SQL selects from `DataDictionary` and **aliases language columns into `Caption` / `Description`**:
   - Page name contains `_ar` → Arabic (`ACaption` / `ADescription`), `Session("DicLang") = "_ar"`
   - Else `Profile("lang")` or appSetting `LatinLanguage`:
     - `E` → `Caption` / `Description`
     - `F` → `FCaption` / `FDescription`
     - else → Arabic columns
4. `Session.Add("dc", DataDictDS1.DataDictionary)` — typed `DataDictionaryDataTable`
5. Optional Redis: `HitsCache.StoreUserDataInCache(SessionID + "dc", …)` + `dcloadedlang`

**Implication:** controls always read **`mycap.Caption`** / **`mycap.Description`**. Language is chosen **at load time**, not per-control at render.

This is the CURRENT reason `_ar` page forks and `Profile.lang` matter for chrome text — not only CSS themes (see `target-ux.md`).

---

## 4. Control contract (how pages bind)

Markup pattern (example from `Common\ActivitiesForm.ascx`):

```aspx
<HitsCC:HitsLabel ID="lblActivity" runat="server"
    Tablename="Activities" Fieldname="ActivityName">Activity</HitsCC:HitsLabel>
```

- Designer text (`Activity`) is a **fallback / design-time** string.
- At runtime, control looks up `Session("dc").FindByTableNameFieldName(TableName, FieldName)` (names uppercased/trimmed in practice).
- Sets `Text` / `HeaderText` / `ToolTip` from dictionary `Caption` / `Description`.

Same `Tablename` + `Fieldname` pattern appears on:

- Labels, buttons, hyperlinks, panels (`GroupingText`)
- Validators (`HitsRequiredFieldValidator`, `HitsRangeValidator`, …)
- Grid columns (`HitsBoundColumn` and siblings)
- Many `HITSControlLibrary` inputs (validation via `hits_validateinput`, `RegTable`/`RegField`, `GetWhere` / `GetSelectClause` consulting DataDict)

---

## 5. Library split (mental model)

```text
                    ┌─────────────────────────┐
                    │  dbo.DataDictionary     │
                    │  (+ GetOrgDataDictionary)│
                    └───────────┬─────────────┘
                                │ LoadDC
                    ┌───────────▼─────────────┐
                    │ Session("dc") DataTable │
                    │ (+ optional Redis)      │
                    └───┬─────────────────┬───┘
                        │                 │
         ┌──────────────▼──┐   ┌──────────▼──────────────┐
         │ HITSCultured    │   │ HITSControlLibrary      │
         │ captions / UX   │   │ inputs / filters / SQL  │
         │ chrome text     │   │ clause helpers          │
         └────────┬────────┘   └──────────┬──────────────┘
                  │                       │
                  └───────────┬───────────┘
                              ▼
                     aspx / ascx markup
```

---

## 6. TARGET implications (do not implement here)

| CURRENT | TARGET guidance |
|---------|-----------------|
| Session-sized DataTable of ~38k rows | Prefer **API + cache** (org/lang scoped), not dumping full dict into every web session |
| `Tablename`+`Fieldname` on every control | Keep the **same metadata keys** as the stable contract for Razor label helpers / form metadata |
| Language baked into `Caption` at load | Expose `lang` explicitly; return localized caption in API response |
| Dual assemblies + Web Forms base classes | Replace with **shared metadata service** + Razor tag helpers / components; do **not** port control inheritance trees 1:1 |
| Dict drives validators + SQL fragments | Split concerns: **display metadata** vs **validation rules** vs **query builders**; avoid regenerating dynamic SQL from UI metadata without review |
| `_ar` pages reload dict | Align with D-012: one UI + `CultureInfo` / `dir`, one metadata call with `lang` |

**Strangler tip:** any first slice that rebuilds forms should either (a) call a thin DataDictionary API for labels, or (b) freeze a small slice-local resource file **sourced from** DataDictionary export for that module — do not invent parallel EN/AR string tables by hand.

---

## 7. Cursor rules of thumb

1. When reading legacy ascx/aspx, treat `Tablename`/`Fieldname` as the **real** field identity; visible English in markup is secondary.
2. When proposing TARGET forms, ask: "What DataDictionary `(TableName, FieldName)` keys does this screen use?"
3. Do not assume SQL-only UI — controls + dict + code-behind share responsibility (D-010).
4. Inventory of dict coverage per module belongs in slice prep (export distinct TableNames used by that folder's ascx).

---

## 8. Open follow-ups

- [ ] Document `GetOrgDataDictionary` result shape vs base `DataDictionary` (org overlays?).
- [ ] Sample top TableNames by control usage for candidate slices (ERec, TimeManagement, …).
- [ ] Decide TARGET metadata API shape (keyed get vs module pack download).
