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
| Color | `--color-primary`, `--color-topbar`, `--color-surface`, `--color-danger` | Product teal chrome + surfaces (not legacy Blue theme) |
| Type | `--font-size-xs` … `--font-size-display`, `--font-sans` | Scale + Arabic-capable system stack |
| Spacing | `--space-1` … `--space-8` | 4px base |
| Radius | `--radius-sm` … `--radius-pill` | Cards, chips, culture switch |
| Elevation | `--elevation-0` … `--elevation-3` | Header menus and cards |
| Density | `--density-control-height`, `--density-tap-min` | 44px-class tap targets |

`app.css` maps 375 / 768 / 1280: single-column home cards on small screens, two columns at tablet, three at desktop. Logical properties (`inset-inline`, `margin-inline`) keep RTL mirrored.

## Chrome pieces

| Piece | Where |
|-------|--------|
| Skip link | `_Layout` |
| Brand mark + name + tagline | `_Brand` |
| Culture segmented control | `_CultureSwitch` |
| User menu (avatar, tenant chips, logout) | `_Layout` `<details>` |
| Login / logout card | `Pages/Account/*` with `ViewData["Shell"] = "auth"` |
| Landing identity strip + what’s-next cards | `Pages/Index.cshtml` |

Auth protocol is unchanged (OpenIddict + HttpOnly BFF cookie). Placeholders on home are not wired to DNACloudDB or Agenda.

## PR UX checklist (UX-0)

- [ ] Uses `_Layout` (or an approved popup/print layout later)
- [ ] Uses tokens + `app.css`; no new theme fork
- [ ] LTR + RTL (`dir`, culture switch, no `*_ar` page)
- [ ] Labels, focus-visible, contrast on primary actions
- [ ] 375 / 768 / 1280 usable
- [ ] No new iframe inside modern UI
