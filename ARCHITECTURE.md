# Architecture - HITSHCM

Last updated: 2026-09-21 | Owner: Mina

## Stack

| Layer | Choice | Notes |
|---|---|---|
| Frontend | | |
| Backend | | |
| Data | | |
| Auth | | |
| Hosting | | |
| Observability | | |

## Context

```text
User -> App UI -> API / server actions -> Data store
                 \-> Auth provider
```

## Boundaries

- UI never holds privileged secrets.
- Authz is enforced server-side (deny by default).
- External integrations isolated behind thin adapters.

## Module map (fill after tech design)

| Area | Path / package | Owns |
|---|---|---|
| | | |

## Data notes

- Primary entities:
- Migrations strategy:

## Rules

1. Smallest vertical slice per TASK.
2. Secrets never in git or client bundles.
3. Record stack/API shape changes in `DECISIONS.md`.
