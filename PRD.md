# PRD - HITSHCM

> Status: Draft (modernization) · Owner: Mina · Updated: 2026-09-21

## Vision

Modernize HITS Nas/DNA HCM (`D:\Workspaces\HITSNasDnaFS`) into a maintainable, cloud-ready HITSHCM platform—preserving hire-to-retire coverage and multi-country payroll/HR depth—without a risky all-at-once rewrite.

## Problem

The current core (`HITSNasDnaV12.1\NasDna`) is a large **VB.NET ASP.NET Web Forms (.NET Framework 4.8)** application (~1.5k pages). That stack slows delivery, hiring, cloud-native ops, and UX upgrades, while the product still must support payroll, statutory rules, ESS/MSS, and deep integrations.

## Product (unchanged business intent)

Modular HCM: Core HR, Payroll, Recruitment, Training/Competencies, Career/Performance, Time & Attendance, ESS/MSS (+ mobile), Analytics (incl. Power BI), optional IoT. Azure SaaS / on-prem / hybrid. Multi-lingual, multi-country, highly configurable.

## Approach

**Strangler modernization** of the existing codebase—not greenfield rebuild of the full suite on day one.

### Goals

1. Keep production clients stable while new slices ship.
2. Move high-value journeys to modern .NET + clearer APIs/UI.
3. Reduce WebForms surface area over time.
4. Unify identity, observability, and deployment toward Azure-friendly patterns.

### Non-goals (near term)

- Rewriting all 1500+ pages in one program
- Throwing away payroll/statutory engines before characterization
- Merging every client fork into core on day one

## In / Out (MVP of modernization program)

**In (program MVP - TBD after strategy pick)**

- Inventory + module map of NasDna V12.1
- Characterization/smoke harness for critical paths (login, employee master, one payroll path)
- First strangler slice (module TBD)
- Target architecture + CI for new code (.NET 10 (LTS))

**Out**

- Full UI rewrite
- Replacing SQL schema wholesale
- IoT modules

## Success metrics (draft)

| Metric | Direction |
|---|---|
| Critical regression rate on touched flows | Down |
| Time to ship a change in strangler module | Down |
| % traffic / journeys on new stack | Up over releases |
| Build/deploy of legacy vs new | Predictable CI |

## Codebase map (reference)

| Path | Role |
|---|---|
| `HITSNasDnaV12.1` | Core WebForms product |
| `HITSMobileV12` / `HITSmobile` | Mobile clients/services |
| `HITSAPIs` / REST services | API surface |
| `HitsIntegerationV12` | Integrations |
| `HITSAI` | AI experiments |
| Client `HITSDNAErec*` folders | Customer-specific recruitment variants |

Detail: `docs/research/HITSHCM-research.md` + `docs/research/codebase-scan.md`

## Open questions

1. Modernization strategy: strangler / modular rewrite / UI-first / API-first?
2. First module to modernize?
3. Must on-prem stay first-class for year 1?
4. Keep VB.NET for legacy while C# for new, or migrate language over time?
5. Relationship to EasyDO identity/SSO?

## Revision log

| Date | Change | Author |
|---|---|---|
| 2026-09-21 | Vision from positioning | Mina / Playbook |
| 2026-09-21 | Reframed as modernization of HITSNasDnaFS | Mina / Playbook |

### UI/UX modernization track (D-012)

Program includes a dedicated UI/UX track (shell, design system, RTL, patterns) parallel to API/data strangler. Detail: docs/architecture/target-ux.md.

