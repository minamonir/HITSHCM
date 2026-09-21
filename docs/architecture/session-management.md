# Session & auth management (CURRENT)

**Status:** inventory fact (2026-09-21)  
**SoT:** `D:\Workspaces\HITSNasDnaFS\HITSNasDna\NasDna`  
**Related:** [data-dictionary-controls.md](data-dictionary-controls.md), D-010, D-011, navigation hub  
**Why this matters:** NasDna is a **multi-tenant Web Forms** app. "Who am I?" and "which SQL database?" live in different stores than page UI state. TARGET auth/session must not copy this stack 1:1, but must preserve the **tenant-binding** and **heavy-cache** lessons.

---

## 1. Three layers (do not conflate)

| Layer | Mechanism | Lifetime | Holds |
|-------|-----------|----------|-------|
| **Authentication** | Forms cookie `.ASPXAUTH` (+ optional OWIN OIDC/Okta/SAML) | Sliding ~30 min (`Web.config`) | Identity **name** only |
| **Profile** | `SqlProfileProvider` (`ProfileCommon`) | Durable in `aspnetdb` (or DefaultConnection profile DB) | Tenant connection string, lang, emp/profile ids, rights flags, business group, theme, … |
| **Session** | ASP.NET session (`InProc` **today**; Redis provider exists but commented) | Sliding ~30 min | Ephemeral UI state + warm DataTables (`dc`, rights, reports, …) |

Optional fourth: **HitsCache** (`System.Runtime.Caching.MemoryCache`) — process-local heavy blobs keyed by `SessionID + suffix`, used when Redis/session would be too fat.

```text
Browser
  ├─ .ASPXAUTH  →  FormsIdentity.Name = "logon - server - database"
  ├─ ASP.NET_SessionId
  ├─ AuthToken (anti-fixation, optional)
  └─ .ASPXANONYMOUS (pre-login profile migrate)
         │
         ▼
   Profile (SQL) ── ConnectionString, Lang, ProfileId, LogonName, …
         │
         ▼
   Session (InProc | Redis) ── Empid, agenda, dc, NasOptions, …
         │
         └─ HitsCache (MemoryCache) when MemoryCacheKeys bit says so
```

---

## 2. Login → tenant bind (`logon.aspx.vb`)

Primary paths: `dologon` / `dologon_cloud` / `dologonwithselectedbg`, plus SSO (Okta OID C, Azure AD OIDC, SAML via ComponentSpace, AD `IsAuthenticated`).

**Business group selection** picks a row from setup DB (`UserLogonDS.businessgroup`):

1. Build tenant SQL via `CommonLib.build_connectionString(server, database, security, user, password, …, applicationName)`
2. `applicationName` embeds **`"HITSDNA-" + SessionID + "-" + clientIP`** (shows in SQL as app name for auditing)
3. Hydrate **Profile** (not Session) with at least:
   - `ConnectionString`, `BusinessGroup` / name, `LogonBy`
   - `ProfileID` / emp ids, `UserGroup` / `AllUserGroups`
   - `LogonName`, `Name` / `AName`, date/calendar prefs, feature flags
   - `Lang`, attachment path, org view, etc.
4. `Profile.Save()`
5. Issue Forms cookie:

```vb
' Dash = " - "
FormsAuthentication.SetAuthCookie(logonName + Dash + servername + Dash + databasename, persistentcookieflag)
```

So **`User.Identity.Name` = `"user - sqlServer - database"`** — not a GUID. Persistence of the cookie is gated by sys config `Security_NonPersistentCookie`.

6. Anti-fixation: `Session("AuthToken")` + twin `HttpCookie("AuthToken")` = new GUID (checked on non-postback when `EXSecurity_SessionFixation = 1` in `HitsWebPage`).

7. `LoadDC` is **not** always called on login (commented in places); first authenticated page load via `HitsWebPage` fills `Session("dc")` (and related caches).

---

## 3. `Web.config` (as checked in SoT)

| Setting | Value (SoT) |
|---------|-------------|
| `<authentication mode="Forms">` | `.ASPXAUTH`, `logon.aspx`, timeout **30**, sliding, `SameSite=Lax`, `requireSSL=false` |
| `<sessionState>` | **`InProc`** active; custom `HITSRedisSessionStateProvider` **commented** |
| Session timeout | **30** minutes, compression on, cookie-based |
| Profile | `SqlProfileProvider` / `DefaultProfileProvider`, many properties (`ConnectionString`, `Lang`, …) |
| Membership / roles | SqlMembership present; **roleManager often disabled** (app uses custom UserGroup rights) |
| `machineKey` | Explicit AES/SHA1 (required for web-farm Forms tickets if ever multi-node) |
| `<identity impersonate="true">` | Present (Windows identity under IIS — ops concern) |

**Implication:** with InProc, a web-farm or recycle **drops** Session (UI state + `dc`); Profile + Forms cookie can still rehydrate tenant context, then `HitsWebPage` rebuilds caches.

---

## 4. What Session actually stores

### High-churn UI keys (by usage frequency)

`Empid`, `currentagenda`, `pageitemno`, `iconitemno`, `documentno`, `dc`, sort/filter flags, screen size (`ScrRW`/`ScrRH`), change-lang, report ids, grid edit flags, etc.

### Warm caches loaded by `HitsWebPage` (authenticated + EnableDataDictionary)

| Key | Source |
|-----|--------|
| `dc` | `CommonLib.LoadDC` ← DataDictionary (see data-dictionary doc) |
| `DicLang` | `EN` / `_ar` |
| `AllReportsDT` / `Reports` / `HRInqReports` | report procs / adapters |
| `NasOptions` | NasOptions table |
| `UserGroupRights` / `HRUserGroupRights` | rights by AllUserGroups |
| `UserRoles` | `SSGroupRole` by ProfileId |
| `ItemNoList` | `WEB_DDLLoad` |
| `SSDocumentStatus` | status lookup |
| `Aduit_Session` | flag so audit fires once |
| `AuthToken` | fixation pair |
| `validLic` / `LoginVaild` | license / login gate |
| `Nas_ReportRS` | reporting helper object |

Most page code reads **Profile** for connection/lang/identity and **Session** for the current employee/agenda/dict.

---

## 5. Redis + HitsCache (optional scale path)

### `HITSRedisSessionStateProvider`

- Extends `Microsoft.Web.Redis.RedisSessionStateProvider`
- On get/set exclusive: for fat items (`dc`, `NasOptions`, `LicTable`, `AllReportsDT`, `UserGroupRights`, `Reports`, `DateCalc`, …) either:
  - **decompress** bytes stored in Redis session, or
  - **rehydrate from HitsCache** when `MemoryCacheKeys` bitmask position is `"1"`
- `MemoryCacheKeys` (appSetting / `Global_asax.MemoryCacheKeys`, default `"000000"`) is a **6-char bit string** controlling which blobs live in process MemoryCache vs session store

### `HitsCache`

- `MemoryCache.Default`, keys like `UserCache_{SessionID}dc`, `…NasOptions`, …
- Sliding expiration (~30 min from config `MemoryCacheSlidingExpiration`)
- `Session_End` / LogOff: `RemoveFromCache(SessionID)` when Redis enabled
- `CheckLoadedDC(lang)`: invalidate/reload dict if language mismatch

**Today in SoT Web.config:** Redis session provider is **off** (InProc). Code paths still exist for farms that flip the comment and set Redis host/port/accessKey.

---

## 6. Page gate (`HitsWebPage`)

Base page for most UI:

1. License check via `Session("validLic")`
2. Optional **AuthToken** cookie == Session value (session fixation)
3. `PageAuthorization()` (menu/item rights)
4. Ensure report helper in Session
5. If authenticated + DataDictionary enabled → ensure `dc` (Redis/HitsCache or `LoadDC`)
6. First-time **`Aduit_Session(1, …)`** with app name `HITSDNA-{SessionID}-{clientIP}`
7. Lazy-fill warm DataTables listed above

Unauthenticated / logon / redirect pages skip dict load.

---

## 7. Logout & session end

**`NasForms/LogOff.aspx.vb`:**

1. HitsCache purge (if Redis flag)
2. `Session.Clear` / `Abandon` / `RemoveAll`
3. If fixation flag: expire **all** response cookies
4. `Aduit_Session(2, …)` (logout audit)
5. Optionally clear `Profile.ConnectionString` when `EXSecurity_WinAuthLogOutAllSessions=1`
6. `FormsAuthentication.SignOut()`
7. If OWIN cookie auth (`LogonBy=1`): also `Authentication.SignOut` OIDC + cookie types; else redirect to logon

**`Global.asax` `Session_End` / `Session_OnEnd`:** same audit(2) + HitsCache remove + Clear/Abandon (InProc only reliably fires with `InProc` mode).

---

## 8. SSO / external auth (beside Forms)

| Path | Mechanism |
|------|-----------|
| Azure AD / OIDC | `App_Start/Startup.Auth.vb` — OWIN cookie + OpenIdConnect |
| Okta | `Startup.Auth.okta.vb` — OIDC `CodeIdToken`, auth type `"OktaLogin"` |
| SAML | ComponentSpace (`SAMLServiceProvider.InitiateSSO`, ACS sets Forms cookie) |
| AD password | `IsAuthenticated(domain\user, pwd)` on logon |
| Query triggers | `adal=1`, `okta=` on logon URL |

External success still converges on **Profile hydration + Forms cookie with `user - server - database`** (or SSO username parsing that strips Dash suffix).

---

## 9. TARGET implications (do not implement here)

| CURRENT | TARGET guidance |
|---------|-----------------|
| Forms name encodes tenant SQL | Prefer opaque subject + **server-side tenant claim** (BG/org id); never put server/db credentials shape in the browser identity string |
| Profile.ConnectionString in SQL profile | Replace with **tenant resolver** (org → connection) on the API; no per-user raw connection strings in client-visible profile |
| Fat Session DataTables | API + distributed cache (Redis) with **explicit keys** (user/org/lang); don't port Session-as-DTO |
| InProc default | TARGET should be **stateless UI** (Razor/Blazor) + JWT/cookie auth; server session only if needed for BFF |
| AuthToken twin cookie | Keep CSRF/fixation defenses; prefer framework antiforgery + rotated session id on login |
| `Aduit_Session` + SQL app name | Keep audit trail; map to structured auth events (login/logout/tenant-switch) |
| OWIN + Forms dual stack | Consolidate on **one** auth stack (e.g. ASP.NET Core Identity / Entra) with external IdP handlers |

**Strangler tip (Mode A):** Agenda iframe can keep legacy session; new Razor slice should authenticate via TARGET cookie/JWT and call API with org context — **do not** share InProc Session with Web Forms.

---

## 10. Cursor rules of thumb

1. `User.Identity.Name` is **not** just the logon — parse / treat as composite with server + database when reading legacy code.
2. Prefer **Profile.*** for tenant/lang/emp; **Session.*** for page-local and cached DataTables.
3. Assume `Session("dc")` may be missing after recycle — `HitsWebPage` reloads it.
4. Redis provider + MemoryCacheKeys are an **ops toggle**, not a separate product mode — document env when enabling.
5. LogOff must clear Forms + Session + optional OWIN; TARGET logout must revoke refresh tokens similarly.

---

## 11. Open follow-ups

- [ ] **INV-SESS-002** Document Profile property list vs aspnetdb schema (which DB hosts profiles in each env).
- [ ] **INV-SESS-003** Confirm production: InProc vs Redis + actual MemoryCacheKeys value.
- [ ] **INV-SESS-004** Map SSO claim → Nas user → BusinessGroup selection for Okta/Entra paths end-to-end.
- [ ] Decide TARGET auth: Entra-only vs hybrid during strangler (ties to D-004 / shell).
