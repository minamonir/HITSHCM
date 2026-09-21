# TARGET session & auth modernization

**Status:** proposed standard (2026-09-21) — aligns with D-011 / D-011a / Mode A–B strangler  
**Counterpart:** [session-management.md](session-management.md) (CURRENT)  
**Related:** target-state.md (OIDC bridge), data-dictionary-controls.md, navigation-hub.md

---

## Goal

Replace the CURRENT triple stack (Forms name = `user - server - db` + Profile.ConnectionString + fat InProc Session) with a **stateless UI + server-resolved tenant** model that still works **beside** legacy Agenda (Mode A).

We are **not** porting `Session("dc")`, Redis session DataTables, or SqlProfile connection strings into ASP.NET Core.

---

## Target model (three concerns, three stores)

| Concern | CURRENT | TARGET |
|---------|---------|--------|
| **Who is the user?** | Forms `.ASPXAUTH` (composite name) | **OIDC subject** (**OpenIddict (local IdP) first**; Entra ID / external OIDC later as an additional authentication method) |
| **Which tenant / DB?** | Encoded in cookie + `Profile.ConnectionString` | **Server-side tenant resolve**: `orgId` / `businessGroupId` claim or lookup → connection from secure config/Key Vault — never a raw conn string in the browser or client profile |
| **Page / UI ephemera** | Fat `Session` + HitsCache | **Almost none in server session**: URL + temp data / PRG; optional short-lived distributed cache for SSRF-safe server scratch only |
| **Warm reference data** | `Session("dc")`, rights DataTables | **API + Redis/Memory cache** keyed by `(orgId, lang, catalogVersion)` — shared across users where safe; user rights keyed by `(userId, orgId)` |

```text
Browser (Razor / future SPA)
  └─ auth cookie (chunked / Aspire-friendly) or BFF cookie
         │
         ▼
ASP.NET Core host (Razor Pages + APIs)
  ├─ AuthN: OIDC handler (OpenIddict local issuer first; Entra later)
  ├─ AuthZ: policies / handlers from claims + rights service
  ├─ TenantContext: middleware resolves org → DbConnection factory
  └─ IDistributedCache / Redis: dict packs, rights — NOT "session bag of DataTables"
         │
         ▼
SQL (DNACloudDB etc.) via repositories — same SoR, new access path
```

---


## IdP phasing (locked)

1. **Now:** OpenIddict hosts local users / password (and can federate later) — issuer for HITSHCM BFF cookie.
2. **Bridge:** Mode A session-exchange maps legacy Forms user → OpenIddict subject (or issues local cookie after legacy proof).
3. **Later:** Add Entra (and/or Okta) as external IdP to the same OpenIddict gateway — app still sees one token shape.
## Recommended auth pattern: **BFF + cookie** (not raw JWT in browser)

For Razor Pages (D-005) on .NET 10 (D-011a):

1. **Backend-for-Frontend**: interactive cookie scheme after OIDC login (HttpOnly, Secure, SameSite=Lax/Strict as appropriate).
2. APIs called by that UI use the same cookie (same site) or a short-lived access token kept **server-side** (no localStorage JWT).
3. Later SPA/mobile: Authorization Code + PKCE; still no fat server Session.

**Why not “just JWT in the SPA” now?** First slices are Razor; BFF keeps tokens off the browser and matches EasyDO-style server apps.

---

## Tenant binding (critical)

| Do | Don't |
|----|-------|
| Put `bg_id` / `org_id` (and maybe `emp_id`) in claims or a server session store after login picker | Put `server - database` in the identity name |
| Resolve connection in `ITenantConnectionFactory` from Key Vault / config | Store `Profile.ConnectionString` equivalent in a client-visible profile |
| Allow explicit **tenant switch** that re-issues cookie/claims and clears user caches | Rely on Agenda iframe + legacy Profile for new pages' SQL |

Login picker (business group list) stays as a **product step**; result is a claim, not a conn string.

---

## What replaces CURRENT Session bags

| CURRENT Session key family | TARGET |
|----------------------------|--------|
| `Empid`, agenda, page item, grid edit flags | Route values, query (careful), or short `TempData` / form post; avoid long-lived server session |
| `dc` (full DataDictionary table) | `IDataDictionaryClient`: get-by-(table,field,lang) or module pack; cache in Redis |
| `NasOptions`, `UserGroupRights`, reports lists | Rights/options **services** + cache; invalidate on role change |
| `AuthToken` twin cookie | ASP.NET Core antiforgery + regenerate session id on login (built-in patterns) |
| `Aduit_Session` | Structured auth events (login/logout/tenant-switch) to same audit store or new sink |
| HitsCache by SessionID | Cache by **stable keys** (user/org/lang/version), not SessionID |

**Rule:** if two requests of the same user on two nodes must see it, it belongs in **DB, Redis, or the token/claims** — not InProc.

---

## Strangler bridge (Mode A — new Razor inside legacy Agenda)

Legacy Agenda still owns Forms auth. New slices must not require InProc sharing.

### Recommended bridge (phased)

**Phase A0 — silent bridge (first slices)**  
- New app trusts a **bridge cookie or signed relay token** issued by a tiny legacy endpoint after Forms login (or reads a server-side mapping: legacy SessionID / auth ticket → TARGET user+org).  
- Prefer: legacy `logon` success also calls TARGET “session exchange” API (server-to-server or set cookie on shared parent domain).  
- New Razor app **does not** open SQL with Profile.ConnectionString; it uses TARGET tenant factory.

**Phase A1 — dual-login fade**  
- Corporate IdP (Entra) becomes primary for new UI; legacy Forms remains for old aspx.  
- Same Entra user linked to Nas `LogonName` / ProfileId in a mapping table.

**Phase B — hub replace**  
- Single OIDC login at TARGET shell; legacy aspx only via reverse-proxy with delegated token or retired.

### Explicit non-goals for Mode A

- Sharing ASP.NET InProc Session between Web Forms and Core  
- Reusing `HITSRedisSessionStateProvider` as the Core session store  
- Passing ConnectionString through query/iframe

---

## API authZ

- Authenticated user + `org_id` on every business request (middleware → `ITenantContext`).
- Replace `UserGroupRights` DataTable checks with **policy handlers** (`CanViewEmployee`, `CanEditLeave`, …) backed by a rights service (can wrap existing SQL procs initially).
- DataDictionary captions are **not** authorization.

---

## Decision lock proposal

| ID | Proposal |
|----|----------|
| **D-013** | TARGET auth = **OpenIddict (local OIDC IdP) first** + **BFF cookie** for Razor; Entra as later external IdP; no fat server Session for business state |
| **D-013a** | Tenant = **server-resolved** from `orgId`/BG claim; never identity-name or client-stored connection string |
| **D-013b** | Mode A uses a **session-exchange / bridge cookie** (shared site) — not InProc sharing with NasDna |

(Confirm in DECISIONS.md when you accept.)

---

## Implementation sequence (practical)

1. Scaffold Core auth (OpenIddict) + cookie BFF on HITSHCM host (even before first slice UI).
2. `ITenantContext` + connection factory (dev: single BG; prod: mapping table).
3. Port one read-only API that needs rights → rights service (wrap SQL).
4. Mode A bridge: after legacy login, issue TARGET cookie (spike on shared domain / reverse proxy).
5. First Razor page uses TARGET auth only; iframe parent remains Agenda.
6. Kill any temptation to `HttpContext.Session["dc"]` — use dict API.

---

## Cursor rules of thumb

- New code: **no** `HttpContext.Session` for domain data.
- Claims: `sub`, `email`/`preferred_username`, `org_id`, `profile_id` (Nas), roles/permissions as needed.
- Cache keys: `dd:{org}:{lang}:{table}`, `rights:{org}:{user}` — not `session:{id}:dc`.
- Strangler pages never call `Profile.ConnectionString`.

