# CURRENT login UX study (NasDna `logon.aspx`)

**SoT:** `D:\Workspaces\HITSNasDnaFS\HITSNasDna\NasDna\logon.aspx` (+ `logon_ar.aspx`, `logon_Misr*.aspx` variants)  
**Studied:** 2026-09-21  
**Purpose:** Inform TARGET AUTH-1 login chrome (D-012 UX-0) — borrow product cues, do **not** port Web Forms markup.  
**Behavior companion:** [current-login-flow.md](current-login-flow.md) (AUTH-1b orchestrator).

## Layout pattern

Full-viewport **split composition** (table-based, 100% height):

| Zone | Content |
|------|---------|
| **Left ~60–70%** | Brand: `Images/logo.svg` top-left; large illustration `Images/Login-page-art.gif` mid; copyright footer |
| **Right ~400px+** | Login panel on art background `Images/logonart2.png` (cover) |

No master page — standalone page with heavy **inline CSS**.

## Visual language

| Token | Value |
|-------|--------|
| Page / left wash | `#105D7E` (deep teal) |
| Primary CTA | `#2BAAE3` → text `#F8FCFD`, 8px radius, ~30–36px tall |
| Field chrome | Translucent white fill `rgba(255,255,255,0.2)`, no border, 8px radius |
| Field / label text | White on dark / photo panel |
| Select options | `rgb(52, 67, 79)` background |
| Link-style actions | Transparent + underline (Forgot / Register) |
| Title | `HITS Agentic` |

Responsive tweak: `@media (max-height: 768px)` shrinks logo + art height.

## Fields & actions (product, not chrome)

1. Username (`HitsTextBox`)
2. Password
3. **Business Group** dropdown (from `[businessgroup]` via BG setup connection)
4. **Authentication** mode: Windows / Windows different user / Application
5. Forgot password | Lang control (EN/AR/FR links)
6. Register / Re-Activate User
7. Primary row of buttons: **Log In** | **Office 365** | **OKTA**
8. Failure literal (red)

Culture: separate `logon_ar.aspx` fork (CURRENT anti-pattern for TARGET — use one page + `dir`).

## What to carry into TARGET (recommended)

| Carry | Why |
|-------|-----|
| Split hero + form (or form-over-brand wash) | Instantly reads as HITS product, not generic Identity scaffold |
| Teal brand wash `#105D7E` + cyan CTA `#2BAAE3` | Existing customer recognition |
| Frosted / translucent fields on dark panel | Distinctive CURRENT look |
| Business Group as first-class login step | Real multi-tenant product behavior (map to org/BG claims per D-013a) |
| Secondary IdP buttons (Entra/Okta) as siblings of password CTA | Matches CURRENT + future D-013 Entra path |
| Forgot / register as quiet text actions | Don’t bury recovery |

## What NOT to port

- Table layout / inline CSS / GIF art as hard dependency
- Separate `_ar` page
- Windows Auth dropdown as default for cloud TARGET (keep as optional enterprise mode later)
- `HitsCC` / `HitsCL` control trees
- Embedding connection/tenant in identity string

## TARGET mapping (AUTH-1+)

- Tokens: map `#105D7E` → `--brand-deep`, `#2BAAE3` → `--brand-accent` in `tokens.css`
- Login: left brand panel (SVG/illustration) + right form card **or** single card on teal wash for MVP density
- BG picker: after password success or on same form → sets `org_id` / `bg_id` claims (server), never conn string
- OIDC buttons: stubs OK until Entra wired

## Applied to TARGET (2026-09-21)

Mina chose **all five** CURRENT cues. They are on the AUTH-1 Razor login (`/Account/Login`), not a Web Forms port:

1. **HITS teal / cyan tokens** — `--brand-deep` `#105D7E` and `--brand-accent` `#2BAAE3` in `src/Hitshcm.Web/wwwroot/css/tokens.css` (auth chrome + product primary).
2. **Split hero layout** — CSS grid brand/art panel + form panel; stacks below 768px. SVG illustration in `_AuthHero.cshtml` (no table, no GIF).
3. **Frosted fields on dark** — dark teal form panel; inputs `rgba(255,255,255,0.2)`, light labels, 8px radius.
4. **Business Group field** — required dropdown from `SeedBusinessGroupCatalog`; success replaces `org_id` / `bg_id` claims (D-013a). Never a connection string in identity or cookie.
5. **IdP button row** — Office 365 + OKTA stubs (`type="button"`, coming soon). No Entra/Okta federation yet.

Kept: EN/العربية culture switch, validation, focus rings, quiet footer. Not ported: `logon_ar.aspx`, Windows Auth as default, HitsCC, table layout.
