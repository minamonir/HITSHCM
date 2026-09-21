# Research - HITSHCM

Date: 2026-09-21
Owner: Mina
Source: Product positioning brief (user-provided)

## One-sentence vision

HITSHCM is a modular, multi-country HCM platform that manages the full employee lifecycle (hire to retire)—personnel, payroll, talent, time, self-service, and analytics—deployable as Azure SaaS, on-prem, or hybrid.

## Problem / opportunity

Organizations need one integrated system for HR and payroll that can adapt to local labor, tax, and social-insurance rules, grow without painful migrations, and reduce manual reaction time for planned and unplanned workforce events.

## Target users (preliminary)

| Persona | Needs |
|---|---|
| HR / Personnel admin | Accurate employee records, org structure, compliance |
| Payroll specialist | Correct calc, bank files, statutory filings |
| Hiring / TA | Applicant tracking through hire |
| L&D / competency owners | Training, skills, career paths |
| Managers (MSS) | Approvals, team visibility, performance |
| Employees (ESS) | Self-service + mobile |
| Leadership / analytics | KPIs, Power BI, cost and retention views |
| IT / security | Role-based access, Azure/O365 MFA, integrations |

## Positioning claims

Easy, customizable, informative, dynamic, flexible, reactive, integrative, intelligent. Think globally / support any culture (local policies, multi-lingual, country-specific tax/SI/payroll). Modular by budget. Scalable without complex migration. Automates processes to support work-life balance and faster reaction to events.

## Deployment

- Full HCM SaaS on Microsoft Azure
- On-premises or hybrid when required

## Module portfolio (from brief)

1. Core HR / Personnel — records, org structure, jobs/positions
2. Payroll — calc, processing, bank transfers, statutory compliance
3. Recruitment / Talent Acquisition — ATS through hiring
4. Training & Development / Competencies
5. Career Path / Succession / Performance
6. Time & Attendance — device/machine integration
7. ESS / MSS — including mobile app
8. Analytics & Reporting — KPIs, ad-hoc engine, Power BI
9. Optional IoT — custody/assets, field ops

## Technology (stated)

- .NET Framework
- MS SQL Server / SQL Azure
- Power BI
- XML / web services
- Azure hosting + security (incl. Azure O365 MFA)

## Differentiators (esp. HITS DNA)

Strong UI; online store for alerts/reports/dashboards; built-in ad-hoc reporting; culture/policy customization; multi-lingual + data dictionary; unlimited RBAC; SME-to-enterprise/gov packaging; strong mobile ESS/MSS; 20+ years templates/best practices.

## Integrations

- International ERPs
- Time-keeping machine DBs
- E-government / social insurance exports
- Banking salary/transfer files
- Microsoft ecosystem
- GL, email, mobile, future devices

## Risks / open questions for MVP

- Which **first country/locale** and statutory pack?
- What is the **true MVP module set** for HITSHCM rebuild vs marketing full suite?
- Greenfield rebuild vs extend existing HITS HCM codebase?
- Target: Azure-first only for v1, or must ship on-prem day one?
- Relationship to EasyDO / other HITS products (SSO, shared identity)?
- Mobile: native vs PWA for ESS/MSS in MVP?

## Decision for next stage

Proceed to MVP PRD once Mina answers scoping questions (country, modules, deploy mode, rebuild vs extend).
