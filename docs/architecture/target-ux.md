# TARGET — UI/UX modernization track

> **Status:** First-class program track (parallel to API/data strangler)  
> **Updated:** 2026-09-21  
> **Decisions:** D-005 (Razor default / Blazor optional) · D-011 / D-011a (.NET 10) · **D-012 (this track)**  
> **Related:** [target-state.md](target-state.md) · [navigation-hub.md](navigation-hub.md) · [current-state.md](current-state.md)

## Why a separate track

CURRENT UX is inseparable from the Web Forms shell:

- Agenda + **iframe** (`tabframe`) navigation
- `App_Themes` (Blue default, Black / DarkBlue / White + `_ar`) — skins, not a design system
- EN/AR **duplicate** masters/pages
- Mixed eras (HITS controls, postbacks, some jQuery/Bootstrap)

Modernizing modules onto ASP.NET Core **without** a UI/UX track produces “new stack, old experience.”  
So HITSHCM runs **two tracks** that meet at each slice:

| Track | Focus |
|-------|--------|
| **A — Capability strangler** | API + domain + SQL adapters + cutover of a journey |
| **B — UI/UX modernization** | Shell, design system, patterns, RTL, accessibility, visual consistency |

Every D-004 slice ships **both**: working capability **and** TARGET UX standards for that surface.

## Goals

1. One modern **app chrome** (layout) for all new Razor/Blazor surfaces.
2. A small **design system** (tokens + components) — do not port `.skin` / `App_Themes`.
3. **RTL + culture** via layout/resources from day one (no `_ar` page forks).
4. Reusable **page patterns** (list / filter / detail / wizard) so slices look related.
5. **No new iframes** inside modern UI; Mode A may still *be hosted by* legacy Agenda temporarily.
6. Measurable UX quality: consistency checklist, keyboard/a11y basics, mobile-usable ESS where relevant.

## Non-goals (near term)

- Full visual redesign of all ~1500 legacy pages
- Replacing Agenda hub on day one (Mode B is later)
- React/Vue/Next as default (D-005)
- Pixel-perfect clone of Blue theme in Razor

## CURRENT → TARGET UX mapping

| CURRENT | TARGET |
|---------|--------|
| Many `.master` shells | 1–3 Razor layouts (`_Layout`, popup, print/report) |
| `App_Themes` + `.skin` | CSS tokens + shared components (Bootstrap 5 *or* approved kit) |
| `*_ar` page/master copies | One view + `Culture` / `dir="rtl"` |
| Agenda iframe hosts features | Features are first-class routes; bridge from Agenda until Mode B |
| One-off aspx layouts | Shared partials / Tag Helpers / ViewComponents |
| Dense interactive aspx | Blazor island/module (D-005) only where needed |

## Workstreams (UI/UX track)

### UX-0 — Foundations (before / with first slice)
- [x] TARGET layout(s) + login chrome — AUTH-1 `_Layout` + CURRENT-cue login (split hero, frosted panel)
- [x] Design tokens (color, type, spacing, density) documented — `wwwroot/css/tokens.css` + [`docs/UX.md`](../UX.md)
- [x] Base stylesheet wired into Razor — `wwwroot/css/app.css` (Bootstrap 5 themed via tokens)
- [x] RTL smoke (Arabic culture) — `dir` + `bootstrap.rtl` + EN/العربية control
- [x] UX checklist for PRs (see below)

### UX-1 — Pattern library
- [ ] List + filter bar
- [ ] Detail form + validation summary
- [ ] Confirm/delete modal
- [ ] Employee/org picker (replace DNA search iframe pattern over time)
- [ ] Empty / loading / error states

### UX-2 — Per strangler slice
- [ ] Screens use layout + patterns (no one-off CSS dumps)
- [ ] EN/AR via resources, not duplicated pages
- [ ] If opened from legacy Agenda (Mode A): document bridge; avoid nested iframe inside Razor
- [ ] Blazor only with explicit “interactive module” note

### UX-3 — Hub replacement (Mode B, later)
- [ ] Modern home replacing `Default.aspx` web-parts feel
- [ ] Modern menu replacing Agenda iframe launcher
- [ ] Deprecate encrypted-`value` deep links for modern routes (keep bridge for legacy)

## Definition of done (a modern screen)

- Uses TARGET `_Layout` (or approved popup layout)
- Uses shared tokens/components; no new theme fork
- Works LTR + RTL
- Matches list/detail pattern unless exception recorded
- Accessible enough: focus order, labels, contrast on primary actions
- Linked from strangler slice acceptance criteria

## PR UX checklist

Copy into the slice PR (also in [`docs/UX.md`](../UX.md)):

- [ ] `_Layout` (or approved popup/print layout)
- [ ] Tokens + `app.css` only — no page CSS dump, no `App_Themes` port
- [ ] LTR + RTL via culture/`dir` (no `*_ar` fork)
- [ ] Labels + `:focus-visible` + contrast on primary actions
- [ ] Usable at ~375 / ~768 / ~1280
- [ ] No new iframe inside the modern surface
- [ ] List/detail (or recorded exception) once UX-1 patterns exist

## Cursor / agent rules

When building TARGET UI:

1. Read this file + D-005 + D-012.
2. Prefer Razor Pages patterns; Blazor only for interactive modules.
3. Do not invent a second design system or JS SPA.
4. Do not “skin” legacy Web Forms as the modernization path.
5. Touch SoT UI only if Mina explicitly asks (read-only default).

## References in SoT (inspiration only)

- Shell: `NasAgenda/NasWebGeneralMP.master`, `NasAgenda/NasAgendaMP.master`
- Themes: `App_Themes/Blue` (+ `Blue_ar`, etc.) — **reference, do not port skins**
- Hub: [navigation-hub.md](navigation-hub.md)
