# 03 — Dependencies (NasDna)

Sources: `NasDna\packages.config`, `NasDna.vbproj` HintPath / Reference Include.  
Target framework on packages: mostly **net46/net48**.

## UI

| Package / ref | Version / notes |
|---------------|-----------------|
| AjaxControlToolkit (+ HtmlEditor.Sanitizer) | 20.1.0 |
| Microsoft.ReportViewer.* (Common, WebForms, Design, DataVisualization, …) | GAC/bin-style refs (not all in packages.config) |
| ActiveDatabaseSoftware.ActiveQueryBuilder2 (+ Web/Control/Server + many DB metadata providers) | Query-builder UI stack |
| DocumentFormat.OpenXml | OpenXML docs |
| HtmlAgilityPack | 1.11.61 |
| Myrmec / Myrmec.Mime | 1.2.0 MIME sniffing |
| Microsoft.PowerBI.JavaScript | 2.23.1 (front-end) |

## Data / persistence

| Package / ref | Version / notes |
|---------------|-----------------|
| EntityFramework (+ SqlServer) | **6.1.3** |
| Microsoft.Practices.EnterpriseLibrary.Data / Common / Logging | Classic EntLib DAL/logging |
| Microsoft.Practices.ObjectBuilder | EntLib dependency |
| Microsoft.SqlServer.Types | Spatial/types |
| Microsoft.Data.Services.Client / OData / Edm / System.Spatial | 5.8.4 OData client |
| System.Configuration.ConfigurationManager | 8.0.0 |
| Local: typed DataSets under `DataSets\` | XSD designers (ADO.NET) |
| Local: `AppCode\NasDB.dbml` + `NASDataSource` | **LINQ to SQL** over profile ConnectionString |

## Auth / identity

| Package / ref | Version / notes |
|---------------|-----------------|
| ComponentSpace.SAML2 (Licensed) | 4.4.0 |
| Okta.AspNet (+ Abstractions) | 1.6.0 / 3.0.5 |
| Microsoft.Owin* (Host.SystemWeb, Security, Cookies, Jwt, OAuth, OpenIdConnect) | 3.0.1–4.2.2 mix |
| Microsoft.IdentityModel.* (Tokens, Protocols, OpenIdConnect, JsonWebTokens, Logging, Abstractions) | 6.34.0 cluster |
| Microsoft.IdentityModel.Clients.ActiveDirectory | 3.13.1 (ADAL) |
| Microsoft.IdentityModel.Protocol.Extensions | 1.0.0 |
| System.IdentityModel.Tokens.Jwt | 6.34.0 |
| IdentityModel | 3.10.10 |
| Microsoft.AspNet.Providers / Providers.Core | 2.0.0 (SQL membership/profile/session providers) |
| BouncyCastle | 1.8.9 |

## Cloud / storage / session

| Package / ref | Version / notes |
|---------------|-----------------|
| AWSSDK.Core / AWSSDK.S3 | 3.7.x |
| WindowsAzure.Storage | 7.0.0 |
| Microsoft.WindowsAzure.ConfigurationManager / SDK (Diagnostics, ServiceRuntime) | 3.1.0 / 2.9.0 |
| Microsoft.Azure.KeyVault.Core | 1.0.0 |
| StackExchange.Redis | 2.0.519 |
| Microsoft.Web.RedisSessionStateProvider | 4.0.1 |
| Microsoft.AspNet.SessionState.SessionStateModule | 1.1.0 |
| Google.Apis* (Auth, Core, Drive.v3, Forms.v1) | 1.69–1.73 |

## Reporting / BI

| Package / ref | Version / notes |
|---------------|-----------------|
| Microsoft.PowerBI.Api / Core | 2.0.3 / 1.1.3 |
| Microsoft.ReportViewer.* | Embedded reports |
| Web References: RSServer, RSServer2008, RSServerAzure | SSRS admin SOAP |

## Other / platform

| Package / ref | Version / notes |
|---------------|-----------------|
| Newtonsoft.Json | 13.0.4 |
| Microsoft.AspNet.WebApi.Client | 5.2.9 (HTTP formatting client) |
| Microsoft.Rest.ClientRuntime | 2.3.24 |
| log4net | 2.0.10 |
| System.Text.Json / Buffers / Memory / Pipelines / Threading.* / ValueTuple / … | Compatibility polyfills |
| System.Management / PerformanceCounter / Runtime.Caching / CodeDom / IO.Compression | Host utilities |
| Local HintPath DLLs | `CryptClass\HITSCryptography.dll`, `HITSIBMClient\HITSIBMClient.dll`, `IOTHeatMap\DrawHeatMap.dll`, Muqeem ELM `*.dll` |
| ElmFramework.Foundations / SOA + ELM.MuqeemIDK.* | Muqeem/ELM stack |
| DrawHeatMap, HITSCryptography, HITSIBMClient | Product-local binaries |

## Notable observations

- **Dual auth eras:** SQL Membership providers + OWIN/OIDC/Okta + ComponentSpace SAML.
- **Dual data eras:** Typed DataSets + EntLib + LINQ-to-SQL (`NasDB`) + small EF6 Identity island.
- **Cloud multi-homing:** Azure Storage + AWS S3 + Google Drive/Forms + Redis session (commented custom provider in Web.config).
- Version skew (e.g. Owin Host 3.0.1 vs Security 4.2.2) is a modernization risk.
