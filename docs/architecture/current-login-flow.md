# CURRENT login flow (`logon.aspx.vb`)

**SoT:** `D:\Workspaces\HITSNasDnaFS\HITSNasDna\NasDna\logon.aspx.vb` (~93KB / ~1874 lines)  
**Studied:** 2026-09-21  
**Companion:** [current-login-ux.md](current-login-ux.md) (chrome only)  
**TARGET AUTH-1b:** [`docs/AUTH.md`](../AUTH.md) — `ILoginOrchestrator` maps this flow without porting Forms `user-server-db` cookie names.

This is the **behavior** TARGET must understand. UI restyle without this flow is incomplete.

## Entry points

| Trigger | Handler | What happens |
|---------|---------|----------------|
| Log In | `LoginButton_Click` → `dologon()` | Password / Windows path |
| Office 365 | `ADBtn_Click` | OWIN OIDC `Challenge` (Entra/ADAL) |
| OKTA | `OKTABtn_Click` | ComponentSpace SAML `InitiateSSO` |
| `?adal=1` / `?okta=` | `Page_Load` | Auto-start SSO |
| SSO return | `Page_Load` (authenticated OWIN + referrer) | Resume cloud logon with SSO user |

`dologon()` router:
- **On-prem / classic:** `CloudInstallation≠1` and `UniversalLogin≠1` → `dologon_orginal()`
- **Cloud / universal:** → `dologon_cloud()` → eventually `dologonwithselectedbg(bg, user, pwd)`

## Business Group is tenant selection (not decoration)

1. DDL bound from setup DB `[businessgroup]` (`BGSetupConnectionString`).
2. `Businessgroup_PreRender` filters by **client IP ranges** (`BusinessGroupIPRanges`) and optional per-user allow list (`Businessgroupwhereforuser`).
3. Selected BG row supplies **server / database / credentials / share folder**.
4. `CommonLib.build_connectionString(...)` → **`Profile.ConnectionString`** (durable SqlProfile).
5. App name in SQL includes `HITSDNA-{SessionID}-{clientIP}` for audit.

Without a BG, there is no tenant DB. TARGET’s “Business Group” field must map to **server-side org/BG → connection factory** (D-013a), never embed conn string in the auth cookie name.

## Auth modes (`DDLAuthenticationMode` / cases)

| Mode | Meaning |
|------|---------|
| 1 | Windows (current IIS identity) → lookup `NasUsers` by `User.Identity.Name` |
| 2 | Windows different user → `LogonUser` (advapi32) then `NasUsers` |
| 3 | Application password → `NasUsers` where password = `CommonLib.DoPassword(...)` |
| 4 | SSO/ADAL path (`ADAL=1`) → treat as federated; `Profile.LogonBy=1` |

Cloud path still validates the user against the **selected BG’s** DNA DB after Profile.ConnectionString is set.

## Profile hydration (before cookie)

After user match, code loads into **Profile** (not Session as SoR):

- Identity: `ProfileID`/`EmpId`, `UserGroup`, `LogonName`, names, employee id  
- Tenant: `BusinessGroup`, `BusinessGroupName`, `ConnectionString`, `AttachmentPath`  
- Prefs: `Lang`, date formats, Hijri flags, decimal, feature toggles, org view (`SysParam.EnableOrgView`)  
- Then **`Profile.Save()`**

Session gets ephemeral flags (`Businessgroupwhere`, optional `firstlogonack`, later `dc` on first page).

## Forms cookie (CRITICAL CURRENT smell)

```
FormsAuthentication.RedirectFromLoginPage(
  LogonName & "-" & servername & "-" & databasename,
  persistent)
```

Identity **name** = `user - sqlServer - database`.  
TARGET must **not** copy this (D-013 / D-013a): opaque subject + server tenant resolve.

Optional anti-fixation: if `Security_SessionFixation=1`, twin `AuthToken` in Session + cookie.

## Policies / hardening

- `checkuserpolicy` / consecutive attempts (`ConsecutiveAttempts` S/F)  
- First-logon ack (`FirstLogonAck`)  
- `Security_NonPersistentCookie` sys config  
- IP-filtered BG list  

## SSO buttons (behavior, not stubs forever)

- **Office 365:** OWIN OpenIdConnect challenge; on return, cloud logon continues with SSO username (strip domain suffix sometimes).  
- **OKTA:** SAML partner IdP; same resume path.  
Config flags: `EnableAD`, `Enableokta`, `okta:OrgUri`, `PartnerIdP`.

## TARGET mapping (AUTH-1 → AUTH-2)

| CURRENT | TARGET |
|---------|--------|
| BG DDL → Profile.ConnectionString | BG/org picker → claims `org_id`/`bg_id` → `ITenantConnectionFactory` |
| Auth mode 1–3 | Password + optional Windows later; cloud default = app password / IdP |
| Forms name embeds server/db | Cookie BFF with stable user id only |
| Profile bag | Claims + server profile/tenant store |
| OIDC/SAML buttons | Real handlers after OpenIddict local; Entra/Okta as external IdPs (D-013) |
| `LoadDC` after login | Metadata API / cache keyed by org+lang — not Session dump |

## Implication for current Razor login PR

Visual CURRENT cues (live Agentic split, Username/Password first paint) are **necessary but not sufficient**.  
Next behavior slice:

1. BG list from real/setup source (or seeded stand-in with same semantics).  
2. On login success: set org/BG claims; resolve connection **server-side**.  
3. Wire Office 365 / OKTA from stubs → OpenIddict external IdPs when ready.  
4. Port policy hooks (lockout / first login) as AUTH-2+.


## Deeper: policies, SSO return, first logon (2026-09-21)

### `CheckURules` policy codes

`checkuserpolicy(username)` calls DNA proc **`[CheckURules]`** on **already-selected BG** connection (`Profile.ConnectionString`) with `@LogonName`, `@Lang`.

| Code | Meaning in logon VB |
|------|---------------------|
| `001` | OK — continue logon |
| `003` | OK but **first-logon ack** path (`Session("firstlogonack")`) |
| `002` / `004` | **Blocked** — message text is the remainder after stripping codes; shown in `FailureText` |

Result also stashed in `Session("LoginValid")`. On proc failure, defaults to `001` (fail-open — TARGET should fail-closed).

### Consecutive attempts / lockout

`ConsecutiveAttempts(user, SorF, bgConn)`:

- **`SorF = "S"`** (success on known BG): `NasUsers.ConsecutiveAttempts = 0`, set `LastLogonDate = Now`.
- **`SorF = "F"`** (failure): `ConsecutiveAttempts = ConsecutiveAttempts + 1`.
- **`bgConn = "ALL"`** (cloud fail before BG): for every BG in `BusinessGroupUsers` for that logon, build that BG’s connection string and increment attempts on **each** tenant `NasUsers` row.

Also reads `V_NasUsers.DisableFeatures` → `Session("DNAlicRestrictedforuser")` (license feature gating after login).

Actual lock threshold lives inside **`CheckURules`** / DNA data (not hard-coded in the page).

### First-logon acknowledgement

`FirstLogonAck(user, bgConn)`:

```sql
SELECT CASE WHEN ENABLEfirstlogonack = 1 THEN FIRSTlogonack ELSE 1 END
FROM NasUsers CROSS JOIN sysparam WHERE NasUsers.LogonName = @logonname
```

→ `Session("firstlogonack")`. If policy `003` or ack flag requires it, post-login UX forces acknowledgement page (not completed inside logon itself).

### BG IP filtering (two layers)

1. **BG-level** (`Businessgroup_PreRender`): `BusinessGroupIPRanges` (IPStart/IPEnd) vs client IP — drops BGs outside range from DDL.
2. **User-level** (`Businessgroupwhereforuser`): `BusinessGroupUsersIPRanges` for `(BusinessGroup, LogonName)` — if rows exist, further intersect; if no rows, keep prior list.

Uses `IPAddressRange` helper + `CommonLib.getclientip()` (respects proxy headers when enabled).

Cloud SSO path often **skips** IP gating when `ADAL=1` (commented intentional fix).

### SSO return path (Page_Load)

1. Normalize `ReturnUrl` (always bounce to `logon.aspx?ReturnUrl=.../Default.aspx` if missing).
2. If referrer is **not** Entra/Okta host → **SignOut** Forms + OWIN cookies (fresh login).
3. If referrer **is** Entra (`ida:AADInstance`) or Okta (`okta:OrgUri`) **and** OWIN cookie authenticated:
   - Extract SSO name (Okta may strip `" - "` suffix).
   - Prefill `LogonNameT`, lock user/password fields.
   - Set `ADAL=1` / ViewState.
   - `Businessgroup_PreRender` ends with `dologon_cloud()` when ADAL=1 (password check relaxed in SQL for ADAL branch).
4. Auto-challenge: `?adal=1` → OIDC Challenge; `?okta=` → SAML InitiateSSO.
5. On-prem only: `?tenantid=` or `DefaultBG` preselects BG; `AutoLogon=1` can call `dologon()` on first load (Windows).

### Error messages

`ErrorMsgArray()` pulls captions via **`Hits_GetDCCaption('Logon', n, @lang)`** (data-dictionary!) with appSettings/English fallbacks:

1. Windows auth required  
2. Windows password required (mode 2)  
3. Invalid user/password  

So even logon errors are **DC-driven** in CURRENT.

### TARGET implications (deeper)

| CURRENT | TARGET AUTH-1b+ |
|---------|-----------------|
| `CheckURules` | Port as tenant-scoped policy service; map codes to ProblemDetails; **fail-closed** |
| ConsecutiveAttempts | Per-tenant counter + optional global; expose lockout via policy |
| FirstLogonAck | Claim or server flag → force `/Account/FirstLogon` page |
| BG + user IP ranges | Optional edge middleware; document cloud SSO bypass |
| SSO resume | External IdP callback → same BG resolve pipeline as password |
| DC logon captions | Reuse DataDictionary/API for localized auth errors |


## Deeper still: `CheckURules` SQL + SSO callbacks (2026-09-21)

### Exact `CheckURules` logic (DNACloudDB)

Source: `docs/inventory/db/DNACloudDB-schema/dbo/StoredProcedures/CheckURules.sql`

Inputs: `@LogonName`, `@Lang` (default `E`).  
Reads `NasUsers` + `EmpAssignment`/`HRStatus` + `SysParam`.

| Result | When |
|--------|------|
| **`002` + caption** | `NasUsers.InActive = 1` (account inactive) |
| **`004` + caption** | `SysParam.ForbiddenInactiveEmployees=1` AND employee pay status ≠ active AND (no term date OR term date &lt; today) |
| **`003`** (bare) | Not a Windows-only user AND (password aged past `PasswordExpireDays` OR `DateControl = '0000000003'`) — **password change / first-control required** |
| **`001`** | Otherwise OK |

Captions via `Hits_GetDCCaptionWzLogon('CheckURules', code, @Lang, @LogonName)`.

**Correction vs earlier note:** `003` is **password-expiry / date-control**, not a generic “first logon ack” flag. The page maps `003` into the continue path and then uses **`FirstLogonAck` / Session** separately for ack UX. Treat `003` in TARGET as **must-change-password** (or forced credential step).

`WebAPICheckURules` wraps the same proc and adds:
- `@ADAL=1` → if user is not pure Windows-only, append code **`006`** (“use Microsoft account”) when base was `001`
- `@version=0` mobile → replace bare `001` with update-required caption **`005`**

### SSO callback sequences

```text
Entra (Office 365 button / ?adal=1)
  logon ADBtn → OWIN Challenge(OpenIdConnect)
    → Azure AD
    → redirect back to site root (ida:RedirectUri)
    → OWIN cookie authenticated
    → user navigates/lands on logon.aspx
    → Page_Load sees UrlReferrer ∈ ida:AADInstance
    → prefill LogonName, ADAL=1, lock fields
    → Businessgroup_PreRender → dologon_cloud() (password branch relaxed for ADAL)
    → BG resolve → Profile.ConnectionString → CheckURules → Forms cookie

Okta SAML (OKTA button / ?okta=)
  logon OKTABtn → SAMLServiceProvider.InitiateSSO
    → IdP
    → /SAML/AssertionConsumerService.aspx
         ReceiveSSO → FormsAuthentication.SetAuthCookie(NameId)
         Session("saml-attributes") = attrs
         Redirect(targetUrl or ~/)
    → eventually logon.aspx with Forms/OWIN identity
    → Page_Load Okta OrgUri referrer path (or Forms name)
    → same cloud BG pipeline

Okta OIDC (optional Startup.Auth.okta)
  OpenIdConnect "OktaLogin" (code id_token)
  → token + userinfo claims on ticket
  → same resume idea via cookie auth
```

SAML ACS is intentionally thin: **it does not pick BG or call CheckURules**. Tenant binding still happens on **logon resume**. TARGET should keep that split: IdP callback authenticates person; **app login orchestrator** binds tenant + policy.

SLO: `/SAML/SLOService.aspx` — IdP logout request → `FormsAuthentication.SignOut` + SendSLO; else redirect home.

### TARGET AUTH-1b checklist (refined)

1. `ITenantResolver` from BG/org (setup DB)  
2. `IUserAuthenticator` (password / external)  
3. `ILoginPolicy` porting `CheckURules` semantics: inactive, HR-forbidden, password-expired(`003`)  
4. Attempt counters per tenant (+ optional fan-out)  
5. First-logon ack as separate step  
6. External IdP callbacks **only** establish identity; then call the same orchestrator as password login  
7. Never encode server/db into the auth cookie name  


## Deeper still: connection string, NasUsers, password reset (2026-09-21)

### `CommonLib.build_connectionString`

Builds classic ADO.NET SQL connection from BG row fields:

- `data source=` + server  
- `initial catalog=` + database  
- If `securityinfo` upper = `FALSE` → `integrated security=SSPI` (Windows)  
- Else → `user id=` + userid, `password=` + **`HITSCryptography.DecryptDataTripleDES(password)`** (BG SQL password stored encrypted at rest)  
- Always: `persist security info=`, `Application Name=` (login passes `HITSDNA-{SessionId}-{clientIp}`), `Connection Lifetime=0`, `packet size=`

**TARGET:** same idea as `ITenantConnectionFactory` — decrypt secrets from vault/secure store, never from the auth cookie; stamp an application name for SQL audit.

### Password hashing at login (`DoPassword`)

`DoPassword(encrypt As Boolean, pass)` → TripleDES via `HITSCryptography` (encrypt on write, decrypt on compare in change-password; cloud logon compares with `DoPassword(True, typed)` against `BusinessGroupUsers` / DNA).

Legacy `encryptpassword` (10-char Caesar-style) still exists — treat as obsolete.

**TARGET:** do not port TripleDES for new users; support verify-legacy + rehash-on-login to ASP.NET Identity / PBKDF2 while DNA remains SoR optionally.

### `NasUsers` columns login actually cares about

| Column | Role |
|--------|------|
| `LogonName` (PK) | Identity key |
| `PassWord` | Encrypted secret |
| `EmpId`, `UserGroup`, `OtherUserGroups`, `UserOrganization` | Profile hydration / authZ scope |
| `WindowsOnly`, `PasswordRequired` | Auth mode gates |
| `InActive` | → CheckURules `002` |
| `PasswordDate`, `PasswordNeverExpire` | → expiry / `003` |
| `DateControl` | Special `0000000003` forces `003` (must change) |
| `ConsecutiveAttempts`, `LastLogonDate` | Lockout bookkeeping |
| `FirstlogonAck` | Ack state (with `SysParam.EnableFirstLogonAck`) |
| `DisableFeatures` | Post-login license/feature clamp |
| Rights bitfields (`ProfileRights`, …) | Loaded into Profile after success |

`SysParam` also: `PasswordExpiredays`, `ForbiddenInactiveEmployess`, `ConsecutiveAttemptsCount`, `EnableFirstLogonAck`, password history/validation flags.

### Forgot password (`ForgotPassword.aspx.vb`)

1. Resolve BG(s): on-prem from hidden BG; cloud from `BusinessGroupUsers` by logon.  
2. For each BG: `build_connectionString` → `EXEC ForgotUserPassword @UserName, @Email` → returns password material + msg code.  
3. Optional `sysparamconfig.ForgotPasswordResetRandom=1`: generate random password, `DoPassword(True,…)`, update **tenant** `NasUsers` **and** (cloud) `BusinessGroupUsers`, set **`DateControl='0000000003'`** (forces CheckURules `003` on next login = must change).  
4. Email body/subject from DNA proc `ForgotPasswordEmailCaptions` (lang-aware) + SMTP.

### Change password (`Security/changepassword.aspx`)

If `Session("LoginValid")` contains **`003`**, page runs in forced-change mode. Verifies current via `DoPassword(False, stored)`, writes `DoPassword(True, new)`.

### TARGET mapping extras

| CURRENT | TARGET |
|---------|--------|
| TripleDES BG SQL pwd + user pwd | Vault / config plaintext for tenant SQL (AUTH-2 factory does not decrypt TripleDES); Identity password hasher + legacy verifier |
| `DateControl=0000000003` | Explicit `must_change_password` claim / flag |
| Forgot fan-out all BGs | Same multi-tenant reset policy or single-BG with explicit picker |
| App Name in conn string | OpenTelemetry / SQL `Application Name` = user+tenant+request |

