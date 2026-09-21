# Security - HITSHCM

Updated: 2026-09-21

## Baseline

- Authenticate private routes and APIs
- Authorize by ownership or role (deny by default)
- Validate inputs at trust boundaries
- No secrets in git, chat, or client bundles
- Privileged keys never in `NEXT_PUBLIC_*` (or equivalent)
- `.env` local only; `.env.example` names only

## Threat notes (fill for this product)

| Asset | Threat | Mitigation |
|---|---|---|
| User data | Unauthenticated access to HCM | Login required on Razor folder; DNACloudDB not connected this slice |
| Auth session | Token theft / XSS reading JWT | BFF HttpOnly cookie `Hitshcm.Auth`; no localStorage JWT |
| Admin actions | Seed credentials in shared env | Dev-only seed `admin@hitshcm.local`; change before any shared host |

## Before deploy

Run `/ship-check` and confirm SECURITY items pass.
