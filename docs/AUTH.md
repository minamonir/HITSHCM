# HITSHCM TARGET auth (AUTH-1 / AUTH-1b)

> **Status:** OpenIddict local IdP + Razor BFF cookie + shared **login orchestrator** on `src/Hitshcm.Web`  
> **Decisions:** D-013 / D-013a / D-013b · D-005 Razor · D-011a .NET 10 · D-012 UX shell  
> **CURRENT behavior study:** [`docs/architecture/current-login-flow.md`](architecture/current-login-flow.md) (`logon.aspx.vb`)  
> **Not this slice:** live DNACloudDB / TripleDES DNA password, real Entra/Okta challenge, Mode A Agenda bridge, DataDictionary port, Blazor

## Login → cookie → landing (AUTH-1b)

Password login (and future SSO resume) share **one** orchestrator. Identity password check is only step 1.

```text
Browser
  GET /  (anonymous)
       → 302 /Account/Login
  POST /Account/Login  (username/email + password; BG only if already chosen or second step)
       → ILoginOrchestrator.AuthenticateAsync
            1. Authenticate person (Identity password today; ExternalSso hook for IdP resume)
            2. Resolve Business Group: empty + one membership → auto-select; empty + many → NeedsBusinessGroup (picker step); explicit id → that group
            3. ITenantConnectionFactory.Resolve(org, bg) — named options, server-side only
            4. ILoginPolicyEvaluator (CheckURules codes 001/002/003/004; fail-closed)
            5. Record ConsecutiveAttempts on the user store
            6. Sign-in HttpOnly cookie Hitshcm.Auth with claims org_id / bg_id
               (+ profile_id / emp_id placeholders)
               NEVER a connection string or user-server-database identity name
       → 001: 302 /
       → 003: 302 /Account/ChangePassword (cookie issued; landing gated)
       → first-logon ack: 302 /Account/FirstLogon (placeholder)
       → 002 / 004 / fail-closed: 200 login with error; no cookie
  GET /  (authenticated)
       → landing shell (user, org_id, bg_id from claims)
  POST /Account/Logout
       → cookie cleared → 302 /Account/Login
```

### Password vs future SSO resume

```
POST /Account/Login (password)
        \
         +--> ILoginOrchestrator.AuthenticateAsync
        /         |
Office 365 stub (later: IdP challenge
then ILoginOrchestrator.ResumeExternalAsync)
                  |
                  v
     1. Authenticate person
     2. Resolve BG / org (ITenantCatalog)
     3. ITenantConnectionFactory (server-side named options)
     4. ILoginPolicyEvaluator  CheckURules 001/002/003/004 (fail-closed)
                  |
      +-----------+-----------+----------------+
      |           |           |                |
     001         003         002/004          000
      |           |           |                |
   cookie     cookie +     error, no       error, no
   org_id/    ChangePassword cookie          cookie
   bg_id          |
      |           v
      +--> landing /  (gated until password changed)
```

IdP: **Office 365** on `/Account/Login` is a stub (same first-paint as live cloud). **OKTA** is not shown on first paint. When Entra/Okta are wired, the callback must **not** pick BG or write tenant claims; it calls the same orchestrator (CURRENT SAML ACS is similarly thin — tenant bind happens on logon resume).

## CURRENT VB → TARGET types

| CURRENT (`logon.aspx.vb`) | TARGET AUTH-1b |
|---------------------------|----------------|
| `LoginButton_Click` → `dologon()` / `dologon_cloud()` | `LoginModel.OnPostAsync` → `ILoginOrchestrator.AuthenticateAsync` |
| Auth mode 3 `DoPassword` vs `NasUsers` | ASP.NET Identity password hasher (SQLite stand-in; no TripleDES) |
| `ADBtn_Click` / `OKTABtn_Click` + Page_Load SSO resume | Stub buttons; `ILoginOrchestrator.ResumeExternalAsync` is the resume seam |
| BG DDL from `[businessgroup]` + `BusinessGroupUsers` | `ITenantCatalog` / `SeedBusinessGroupCatalog` (demo-org/demo-bg + HR + East) |
| `CommonLib.build_connectionString` → `Profile.ConnectionString` | `ITenantConnectionFactory` → `TenantConnectionDescriptor` (named options; **not** in claims) |
| `FormsAuthentication.RedirectFromLoginPage(user-server-db)` | Cookie BFF; identity name = username/email only (D-013 / D-013a) |
| `checkuserpolicy` → `[CheckURules]` | `ILoginPolicyEvaluator` / `LoginPolicyEvaluator` |
| CheckURules `001` | Continue; land on `/` |
| CheckURules `002` inactive | `LoginStatus.PolicyBlocked` — no cookie |
| CheckURules `003` DateControl / expiry | Cookie + `/Account/ChangePassword` |
| CheckURules `004` HR inactive forbidden | `LoginStatus.PolicyBlocked` — no cookie |
| Proc error → default `001` (fail-open) | **Fail-closed** (`000` / `LoginStatus.FailedClosed`) |
| `ConsecutiveAttempts(S/F)` | `ApplicationUser.ConsecutiveAttempts` (+ Identity lockout) |
| `FirstLogonAck` Session flag | `first_logon_ack` claim → `/Account/FirstLogon` placeholder |
| Profile bag (rights, lang, …) | Minimal claims only: `org_id`, `bg_id`, `profile_id`, `emp_id` |

## D-013 mapping

| Decision | This host |
|----------|-----------|
| **D-013** OpenIddict local IdP first + BFF cookie | OpenIddict server + validation in-process; Razor signs in with `Hitshcm.Auth` (HttpOnly, SameSite=Lax) |
| **D-013a** Tenant server-resolved | Login first paint is Username + Password. Multi-BG users get a **second-step** picker (`ITenantCatalog` memberships) which sets `org_id` / `bg_id` on the cookie principal. `ITenantConnectionFactory` resolves a **named** SQLite/dev handle. **Never** a connection string in the identity name or client profile |
| **D-013b** No fat InProc Session | `UseSession` is not registered. Continuation uses claims (`must_change_password`, `first_logon_ack`), not Session bags. Mode A bridge is **not** implemented |

The same host also publishes a **local OpenID Connect provider** (OpenIddict):

| Endpoint | Purpose |
|----------|---------|
| `/.well-known/openid-configuration` | Discovery |
| `/connect/authorize` | Authorization code (cookie must already exist) |
| `/connect/token` | Token (authorization_code, refresh_token, password) |
| `/connect/userinfo` | UserInfo |
| `/connect/logout` | End session |

Razor Pages do **not** put access tokens in the browser. Interactive UI uses the Identity cookie. Tokens are for later API / confidential clients.

## Where Entra plugs in later

Keep this OpenIddict gateway as the app's IdP surface. Later:

1. Register Entra (and/or Okta) as an **external authentication method** on OpenIddict (client web provider / federation).
2. Map the external subject to a local `ApplicationUser` (or a mapping table to Nas `LogonName`).
3. Call **`ILoginOrchestrator.ResumeExternalAsync`** to bind BG + policy (same as password).
4. The Razor BFF still consumes **one cookie / one token shape**. Do not add a second Entra-only login stack beside this host.

Do **not** challenge Entra/Okta in AUTH-1b.

## Mode A (legacy Agenda) — out of scope

NasDna Forms auth (`logon.aspx`) remains on IIS. A later AUTH-3 spike should issue a **session-exchange / bridge cookie** after legacy proof (D-013b).

This host must **not**:

- Share ASP.NET InProc Session with NasDna
- Read `Profile("ConnectionString")`
- Host new feature UI in an iframe inside Razor (legacy Agenda may still iframe a modern URL later)
- Port Forms `user-server-db` cookie name

## Tenant factory (AUTH-1b stub / AUTH-2 next)

`ITenantContext` is claim-backed. `ITenantConnectionFactory` now returns a **descriptor** (`Provider=sqlite`, `OptionsName=Identity`, audit `ApplicationName=HITSHCM-{org}-{bg}`). Development does **not** open DNACloudDB. Next AUTH-2 work: map those ids to SQL from Key Vault / config.

## Seed (Development)

- Email / username: `admin@hitshcm.local`
- Password: `ChangeMe!123`
- Org: `demo-org` (also `demo-org-east` via East Region picker)
- Business groups: `demo-bg` (Demo HITS), `demo-bg-hr` (Demo HR), `demo-bg-east` (East Region)
- Admin is a member of **all three** BGs — after password, the login page shows a BG picker (not on first paint). Policy fixture users (`inactive@`, `mustchange@`, `hrinactive@`, `firstlogon@`) are single-BG and auto-select. `multibg@hitshcm.local` is the extra two-group fixture.
- OpenIddict client id: `hitshcm-web` (confidential; local secret in `appsettings.Development.json`)

| Page | Who |
|------|-----|
| `/Account/Login` | Password login through the orchestrator (Agentic split chrome; BG second step if needed) |
| `/Account/ForgotPassword` | Coming-soon stub (does not reset passwords) |
| `/Account/Register` | Coming-soon stub |
| `/Account/ChangePassword` | Forced after policy `003` |
| `/Account/FirstLogon` | First-logon ack placeholder |
| `/Account/Logout` | Clears the HttpOnly cookie |
