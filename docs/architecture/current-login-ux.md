# CURRENT login UX study (NasDna `logon.aspx`)

**Visual SoT (TARGET chrome):** live publish [https://www.hitshcmcloud.org/logon.aspx](https://www.hitshcmcloud.org/logon.aspx) (HITS Agentic), captured 2026-09-21. Screenshot: [`assets/login-live-hitshcmcloud.png`](assets/login-live-hitshcmcloud.png).  
**Code SoT (behavior):** `D:\Workspaces\HITSNasDnaFS\HITSNasDna\NasDna\logon.aspx` (+ `logon_ar.aspx`, `logon_Misr*.aspx` variants).  
**Purpose:** Inform TARGET AUTH-1 / AUTH-1b login chrome (D-012 UX-0) — borrow product cues, do **not** port Web Forms markup.  
**Behavior companion:** [current-login-flow.md](current-login-flow.md) (AUTH-1b orchestrator).

The older TARGET “five CURRENT cues” frosted centered form (PR #3) is **superseded for chrome**. Razor `/Account/Login` matches the live Agentic split, not a card on a teal wash.

## Layout pattern (live)

Full-viewport **split composition** (table-based, 100% height):

| Zone | Content |
|------|---------|
| **Left ~60–70%** | Solid teal `#105D7E`. Top-left `Images/logo.svg` (HITS + HUMANIZING TECHNOLOGY). Centered square hero `Images/Login-page-art.gif` (glowing digital profiles + **AGENTIC** wordmark). Copyright bottom-left. |
| **Right ~400px+** | Dark blue geometric line-art `Images/logonart2.png` (cover). Username + Password only on first paint. Cyan primary buttons. |

No master page — standalone page with heavy **inline CSS**. No app header / culture pill / Identity scaffold card.

## Visual language

| Token | Value |
|-------|--------|
| Page / left wash | `#105D7E` (deep teal) |
| Primary CTA | `#2BAAE3` → text `#F8FCFD`, 8px radius, ~30–36px tall |
| Field chrome | Translucent white fill `rgba(255,255,255,0.2)`, no border, 8px radius |
| Field / label text | White on photo panel |
| Select options | `rgb(52, 67, 79)` background |
| Link-style actions | Transparent + underline (Forgot / Register / Français / العربية) |
| Title | `HITS Agentic` |
| Footer | `© {year} HITS Solutions. All Rights Reserved.` |

Responsive tweak: `@media (max-height: 768px)` shrinks logo + art height.

## Fields & actions (product, not chrome)

**First paint (live cloud):**

1. Username
2. Password
3. Forgot your password? | Français | العربية
4. Register / Re-Activate User
5. **Log In** | **Office 365**
6. Failure literal (red)

Business Group and OKTA are **not** on the default live first paint. Cloud `dologon_cloud` resolves BG after the person authenticates; multi-BG users get a picker as a **second step**.

CURRENT on-prem `logon.aspx` still has a BG dropdown and Windows-auth mode on the same form — do not treat that as the cloud visual SoT.

Culture: separate `logon_ar.aspx` fork (CURRENT anti-pattern for TARGET — use one page + `dir`).

## What to carry into TARGET (applied)

| Carry | Why |
|-------|-----|
| Full-height split (teal hero + patterned form column) | Instantly reads as live HITS Agentic, not generic Identity |
| Logo + AGENTIC art + copyright on the left | Product recognition |
| Teal `#105D7E` + cyan CTA `#2BAAE3` | Existing customer recognition |
| Frosted / translucent fields on the right panel | Distinctive CURRENT look |
| Username + Password first paint | Matches live cloud |
| BG picker only after password when memberships &gt; 1 | Mirrors `dologon_cloud`; claims `org_id` / `bg_id` only |
| Log In + Office 365 | Matches live; OKTA omitted from first paint |
| Forgot / Register / language text links | Don’t bury recovery or culture |

## What NOT to port

- Table layout / inline CSS / GIF as a hard long-term dependency (copied under `wwwroot/img/login/` for chrome parity; SVG/CSS recreation is OK later)
- Separate `_ar` page
- Windows Auth dropdown as default for cloud TARGET
- Always-visible Business Group dropdown or OKTA on first paint
- Frosted **centered card** + app header chrome on `/Account/Login` (PR #3)
- `HitsCC` / `HitsCL` control trees
- Embedding connection/tenant in identity string

## TARGET mapping (AUTH-1+)

- Tokens: `#105D7E` → `--brand-deep`, `#2BAAE3` → `--brand-accent` in `tokens.css`
- Login: left brand/art panel + right patterned form column (not a card)
- Assets (copied from live, not hotlinked): `wwwroot/img/login/hits-logo.svg`, `agentic-hero.gif`, `panel-pattern.png`
- BG picker: **second step** after password when `ITenantCatalog.GetMemberships` returns more than one group → `org_id` / `bg_id` claims (server), never conn string. Single membership auto-selects.
- Office 365: stub until Entra wired (`ILoginOrchestrator.ResumeExternalAsync`)

## Applied to TARGET (2026-09-21, updated)

Razor `/Account/Login` (`ViewData["Shell"] = "login"`) matches live Agentic chrome:

1. Full-height split — no app header/footer on the login shell.
2. Left teal panel — live logo + AGENTIC hero GIF + `© 2026 HITS Solutions. All Rights Reserved.`
3. Right patterned panel — translucent Username / Password; cyan **Log In** + **Office 365**.
4. First paint fields — Username + Password only. No BG dropdown, no OKTA, no Remember me.
5. Links — Forgot / Français / العربية / Register (culture via `/Culture/Set`; Forgot + Register are coming-soon stubs).
6. Multi-BG — after a valid password, show the BG picker (AUTH-1b `NeedsBusinessGroup`); single-BG auto-select unchanged.

Kept: EN/FR/AR via one page + `dir`, validation, focus rings, OpenIddict BFF cookie. Not ported: `logon_ar.aspx`, Windows Auth as default, HitsCC, table layout, secrets in the cookie.
