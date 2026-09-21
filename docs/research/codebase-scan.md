# Codebase scan - HITSNasDnaFS

Date: 2026-09-21
Root: D:\Workspaces\HITSNasDnaFS (no git at root)

## Headline

Enterprise HITS DNA/Nas HCM filesystem: many products and client forks. Primary modernization target is **HITSNasDnaV12.1** (ASP.NET Web Forms, VB.NET, net48).

## Core web app

- Path: `HITSNasDnaV12.1\NasDna`
- Project: `NasDna.vbproj`
- TargetFrameworkVersion: **v4.8**
- ~1559 `.aspx`, ~609 `.ascx`
- Packages include AjaxControlToolkit, EF6, OWIN/JWT/OIDC, SAML, AWS S3, Google APIs, log4net, Report-related stack

## Notable sibling trees

| Folder | Approx files | Notes |
|---|---|
| HITSNasDnaV12.1 | ~8228 | Main solution |
| HitsIntegerationV12 | ~3766 | Integrations |
| HITSMobileV12 | ~738 | Mobile/REST |
| HITSAI | ~296 | AI |
| DNAServices | ~91 | Services |
| HITSDNAErec* | n/a | Client-specific e-recruitment |

## Implication

Treat HITSHCM work as **modernization + strangler extraction**, with characterization tests before refactors.
