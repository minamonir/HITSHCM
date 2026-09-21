# HITSHCM TARGET UX (D-012)

Practical notes for the Razor host chrome. Spec: [`docs/architecture/target-ux.md`](architecture/target-ux.md).

## Run

Requires **.NET 10 SDK**. Same profile Mina uses locally:

```bash
dotnet restore Hitshcm.sln
dotnet run --project src/Hitshcm.Web
```

Open http://localhost:5080/ — anonymous users land on `/Account/Login`. Seed: `admin@hitshcm.local` / `ChangeMe!123`.

Culture: top-bar **EN** / **العربية** writes `Hitshcm.Culture` and sets `dir` + Bootstrap LTR/RTL stylesheet. Same pages, no `*_ar` forks.

```bash
dotnet test Hitshcm.sln
```

## Design tokens

Tokens live in [`src/Hitshcm.Web/wwwroot/css/tokens.css`](../src/Hitshcm.Web/wwwroot/css/tokens.css). Shared product CSS is [`app.css`](../src/Hitshcm.Web/wwwroot/css/app.css). Do **not** add page-level CSS dumps or port `App_Themes` / `.skin`.

Bootstrap 5 is themed from the same tokens (`--bs-primary`, body font/color, radii, focus ring).

| Group | Examples | Role |
|-------|----------|------|
| Brand (CURRENT cues) | `--brand-deep` `#105D7E`, `--brand-accent` `#2BAAE3`, `--brand-field-fill` | NasDna logon family on TARGET chrome |
| Color | `--color-primary`, `--color-topbar`, `--color-surface`, `--color-danger` | Product chrome mapped from those brand tokens |
| Type | `--font-size-xs` … `--font-size-display`, `--font-sans` | Scale + Arabic-capable system stack |
| Spacing | `--space-1` … `--space-8` | 4px base |
| Radius | `--radius-sm` … `--radius-pill`, `--brand-field-radius` 8px | Cards, chips, frosted login fields |
| Elevation | `--elevation-0` … `--elevation-3` | Header menus and cards |
| Density | `--density-control-height`, `--density-tap-min` | 44px-class tap targets |

`app.css` maps 375 / 768 / 1280: login **split hero** stacks on small screens; home cards 1 / 2 / 3 columns. Logical properties (`inset-inline`, `margin-inline`) keep RTL mirrored.

**CURRENT login cues applied to TARGET:** see [`docs/architecture/current-login-ux.md`](architecture/current-login-ux.md). Razor `/Account/Login` uses the split hero, teal/cyan tokens, frosted dark fields, business-group picker (claims only), and Office 365 / OKTA stubs.

## Chrome pieces

| Piece | Where |
|-------|--------|
| Skip link | `_Layout` |
| Brand mark + name + tagline | `_Brand` |
| Culture segmented control | `_CultureSwitch` |
| User menu (avatar, tenant chips, logout) | `_Layout` `<details>` |
| Login / logout | `Pages/Account/*` with `ViewData["Shell"] = "auth"` — split hero + frosted form (CURRENT cues) |
| Landing identity strip + what’s-next cards | `Pages/Index.cshtml` |

Auth protocol is unchanged (OpenIddict + HttpOnly BFF cookie). Placeholders on home are not wired to DNACloudDB or Agenda.

## PR UX checklist (UX-0)

- [ ] Uses `_Layout` (or an approved popup/print layout later)
- [ ] Uses tokens + `app.css`; no new theme fork
- [ ] LTR + RTL (`dir`, culture switch, no `*_ar` page)
- [ ] Labels, focus-visible, contrast on primary actions
- [ ] 375 / 768 / 1280 usable
- [ ] No new iframe inside modern UI
