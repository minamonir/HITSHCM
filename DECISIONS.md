# DECISIONS — HITSHCM

| ID | Date | Decision | Why |
|---|---|---|---|
| D-001 | 2026-09-21 | HITSHCM is modernization of HITSNasDnaFS, not greenfield | Existing production DNA/Nas suite |
| D-002 | 2026-09-21 | Prefer strangler over big-bang when building | ~1500+ WebForms pages |
| D-003 | 2026-09-21 | Phase 0 = Inventory first | Map before strategy |
| D-004 | TBD | First strangler slice | After inventory + SoT |
| D-005 | 2026-09-21 | **UI = Razor Pages default; Blazor optional for interactive modules** | Web Forms strangler fit; API stays ASP.NET Core .NET 10 |
| D-006 | 2026-09-21 (superseded) | Provisional SoT = HITSNasDnaV12.1 | Inventory hypothesis — WRONG |
| D-007 | 2026-09-21 | **SoT = `D:\Workspaces\HITSNasDnaFS\HITSNasDna`** | Mina: last/current code folder; re-scan ~8479 files, 1573 aspx, HITSDNA 2020.01 / net48 |
| D-009 | 2026-09-21 | **Current-state architecture pack is Cursor SoT for system shape** | Agents must read ARCHITECTURE.md + docs/architecture/current-state.md; label CURRENT vs TARGET; do not invent greenfield |

## D-008 — DNACloudDB schema package = full dacpac (2026-09-21)

**Decision:** Use the **full** `docs/inventory/db/DNACloudDB.dacpac` as the canonical schema artifact for HITSHCM modernization (all schemas: dbo, XWB, XRP, …).

**Rejected:** Building a dbo-only dacpac via DacFx filter (blocked by SQL71501 ambiguous UDF/view refs; IgnoreValidationErrors ineffective on BuildPackage).

**Still useful (secondary):** `DNACloudDB-schema/dbo/` script tree + `DNACloudDB-dbo-scripts.zip` + `DNACloudDB-dbo-tables.txt` for dbo-focused browsing — not a substitute for the package.

**Tool:** SqlPackage / microsoft.sqlpackage **170.5.96**; source `fz-dv-db01` / DNACloudDB (Integrated Security).

## D-009 — Current-state architecture documentation (2026-09-21)

**Decision:** Maintain a CURRENT-state architecture pack that Cursor agents must treat as truth for the brownfield system:

- Entry: `ARCHITECTURE.md`
- Depth: `docs/architecture/current-state.md` + `docs/architecture/context.mermaid`
- Always-on rule: `.cursor/rules/current-architecture.mdc`
- Agent docs: `docs/agent/tech_stack.md`, `docs/agent/project_brief.md` describe CURRENT stack first

**Note:** UI later locked by **D-005** (Razor default / Blazor optional). First slice module remains **D-004**.

## D-010 — Current architecture is 2-tier; business logic is split (2026-09-21)

**Decision:** Document and treat SoT HITSNasDna as **2-tier** (Web Forms + SQL). Business rules are **not** SQL-only: large T-SQL surface (~1804 procs / ~407 funcs) **plus** significant VB code-behind.

**Evidence:** live DNACloudDB object counts; SoT file counts; near-zero .svc in NasDna; direct SqlConnection/NASDataSource usage from VB.

**Implication:** Strangler slices must inventory both VB and T-SQL for the chosen module.

## D-011 — TARGET modernized architecture baseline (2026-09-21)

**Decision:** Cursor agents treat the following as the **TARGET** baseline for new HITSHCM work:

- Strangler around live SoT (D-002); not big-bang rewrite
- **3-tier** for new slices: Modern UI → **ASP.NET Core (.NET 10, C#) API** → SQL Server
- **`DNACloudDB` stays system of record** initially; legacy procs/views accessed via **anti-corruption adapters**
- New code lives under `C:\Users\mina\Projects\HITSHCM\` (SoT read-only by default)
- Auth evolves Forms bridge → OIDC; hosting IIS side-by-side + Azure-ready
- Docs: `docs/architecture/target-state.md` + `.cursor/rules/target-architecture.mdc`

**UI:** see **D-005** — Razor Pages default; Blazor optional for interactive modules. 3-tier / API-first unchanged.

**Still open:** D-004 first strangler slice.

**Rejected for TARGET:** continuing to add features as VB Web Forms pages; silent greenfield on non-Microsoft stacks; claiming CURRENT is already 3-tier.

## D-011a — TARGET runtime = .NET 10 LTS (2026-09-21)

**Decision:** New HITSHCM slices target **.NET 10 (LTS)**, not .NET 8.

**Why:** By program start (2026) .NET 10 is the current LTS; longer support window for a multi-year strangler.

**Docs updated:** `docs/architecture/target-state.md`, `ARCHITECTURE.md`, `.cursor/rules/target-architecture.mdc`, agent tech stack, D-011 baseline wording.

**Unchanged:** 3-tier UI→API→SQL strangler; `DNACloudDB` SoR; C#; UI locked by D-005 (Razor default / Blazor optional).

## D-005 — TARGET UI = Razor Pages default; Blazor optional (2026-09-21)

**Decision:** For new HITSHCM strangler slices:

- **Default UI:** ASP.NET Core **Razor Pages** on **.NET 10**
- **Optional UI:** **Blazor** (Server or WebAssembly) only for dense interactive modules (e.g. live grids, org-chart style UX, complex wizards)
- **API:** unchanged — ASP.NET Core Web API (.NET 10, C#) — both UI styles call the same APIs/adapters

**Why Razor default:** Closer to Web Forms mental model; form-heavy HCM admin/ESS; simpler hosting next to legacy IIS; no SignalR sticky-session / WASM tax for typical CRUD journeys.

**Why allow Blazor:** Keep a path for high-interactivity screens without forcing SPA/React.

**Docs:** `docs/architecture/target-state.md`, `.cursor/rules/target-architecture.mdc`, `ARCHITECTURE.md`, agent tech stack.

## D-012 — UI/UX modernization track (2026-09-21)

**Decision:** HITSHCM includes an explicit **UI/UX modernization track** alongside capability strangler (API/data).

**Track delivers:** TARGET shell/layouts, design tokens/components, RTL-first culture model, shared page patterns, per-slice UX DoD, later Agenda/Default hub replacement (Mode B).

**Stack alignment:** D-005 (Razor Pages default; Blazor optional) · D-011/D-011a (.NET 10).

**Doc:** `docs/architecture/target-ux.md`

**Rejected:** Treating UX as “optional polish after backend”; porting `App_Themes`/.skin wholesale; redesigning all legacy aspx in place.

## D-013 — TARGET session & auth (locked 2026-09-21)
- **D-013:** **OpenIddict (local OIDC IdP) first** + BFF cookie for Razor; no fat server Session for business state. Entra/external IdP added later on the same gateway.
- **D-013a:** Tenant server-resolved from org/BG id; never connection string in identity or client profile.
- **D-013b:** Mode A = session-exchange / bridge cookie — not InProc Session sharing with NasDna.
- Doc: `docs/architecture/target-session.md`
- Status: **locked** (OpenIddict-first per Mina 2026-09-21)

