# HITSHCM TARGET auth (AUTH-1)

> **Status:** OpenIddict local IdP + Razor BFF cookie on `src/Hitshcm.Web`  
> **Decisions:** D-013 / D-013a / D-013b · D-005 Razor · D-011a .NET 10 · D-012 UX shell  
> **Not this slice:** DNACloudDB, Entra, Mode A Agenda bridge, DataDictionary port, Blazor

## Login → cookie → landing

```text
Browser
  GET /  (anonymous)
       → 302 /Account/Login
  POST /Account/Login  (username/email + password)
       → ASP.NET Identity validates against SQLite user store
       → SignInManager issues HttpOnly cookie `Hitshcm.Auth`
         (BFF session; no JWT in localStorage)
       → 302 /
  GET /  (authenticated)
       → landing shell (user, org_id, bg_id from claims)
  POST /Account/Logout
       → cookie cleared → 302 /Account/Login
```

The same host also publishes a **local OpenID Connect provider** (OpenIddict):

| Endpoint | Purpose |
|----------|---------|
| `/.well-known/openid-configuration` | Discovery |
| `/connect/authorize` | Authorization code (cookie must already exist) |
| `/connect/token` | Token (authorization_code, refresh_token, password) |
| `/connect/userinfo` | UserInfo |
| `/connect/logout` | End session |

Razor Pages do **not** put access tokens in the browser. Interactive UI uses the Identity cookie. Tokens are for later API / confidential clients.

## D-013 mapping

| Decision | This host |
|----------|-----------|
| **D-013** OpenIddict local IdP first + BFF cookie | OpenIddict server + validation in-process; Razor signs in with `Hitshcm.Auth` (HttpOnly, SameSite=Lax) |
| **D-013a** Tenant server-resolved | `org_id` / `bg_id` claims from `ApplicationUser.OrgId` / `BusinessGroupId`. `ITenantContext` reads claims. **Never** a connection string in the identity name or client profile |
| **D-013b** No fat InProc Session | `UseSession` is not registered. No `HttpContext.Session` bags. Mode A bridge is **not** implemented |

## Where Entra plugs in later

Keep this OpenIddict gateway as the app's IdP surface. Later:

1. Register Entra (and/or Okta) as an **external authentication method** on OpenIddict (client web provider / federation).
2. Map the external subject to a local `ApplicationUser` (or a mapping table to Nas `LogonName`).
3. The Razor BFF still consumes **one cookie / one token shape**. Do not add a second Entra-only login stack beside this host.

Do **not** add Entra in AUTH-1.

## Mode A (legacy Agenda) — out of scope

NasDna Forms auth (`logon.aspx`) remains on IIS. A later AUTH-3 spike should issue a **session-exchange / bridge cookie** after legacy proof (D-013b).

This host must **not**:

- Share ASP.NET InProc Session with NasDna
- Read `Profile("ConnectionString")`
- Host new feature UI in an iframe inside Razor (legacy Agenda may still iframe a modern URL later)

Stub only: see comments in `Program.cs` / `AuthorizationController.cs`.

## Tenant (AUTH-2 next)

`ITenantContext` is claim-backed. Next step is `ITenantConnectionFactory`: `orgId` / `businessGroupId` → SQL connection from Key Vault / config. SQLite here is **Identity + OpenIddict only**, not DNACloudDB.

## Seed (Development)

- Email / username: `admin@hitshcm.local`
- Password: `ChangeMe!123`
- Org: `demo-org`
- Business group: `demo-bg`
- OpenIddict client id: `hitshcm-web` (confidential; local secret in `appsettings.Development.json`)
