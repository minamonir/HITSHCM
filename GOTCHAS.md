# Gotchas — HITSHCM

Append after real bugfixes. Keep rows factual.

| Date | Symptom | Cause | Fix |
|---|---|---|---|
| 2026-09-21 | Agents invent greenfield / Next.js stack | PRD/tech_stack empty; ARCHITECTURE drafted as target-heavy | Read ARCHITECTURE.md CURRENT pack (D-009); tech_stack describes brownfield first |
| 2026-09-21 | Agents edit V12.1 as SoT | Early inventory hypothesis (D-006) | SoT is HITSNasDna (D-007); V12.1 = diff only |
| 2026-09-21 | Assume Web.config catalog is the only DB | Profile("ConnectionString") routes L2S | Always account for Profile-driven tenant DBs |
| 2026-09-21 | Arabic UI strings show as `&#xNNNN;` in HTML | ASP.NET Core `HtmlEncoder` encodes non-Latin by default | Register `HtmlEncoder` with Arabic Unicode ranges in `Program.cs` |
| 2026-09-22 | Landing leak test failed on `ConnectionString` | Footer copy said `build_connectionString` | Do not put that identifier in user-facing strings; say “CommonLib builder” |
| 2026-09-22 | Browser blank / HTTPS-only Chrome on localhost:3000 | Host bound `http://0.0.0.0:3000` (`--urls http://...` overrides launchSettings) | Serve Kestrel HTTPS on 3000 (dev cert); HTTP fallback 3001; OpenIddict callbacks `https://localhost:3000/callback/*` |