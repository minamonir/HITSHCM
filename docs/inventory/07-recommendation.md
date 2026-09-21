# 07 — Recommendation (hypothesis pending Mina)

> **Status:** Hypothesis for modernization planning — **pending Mina confirmation**. Not a committed delivery plan.

## Recommended strategy

**Strangler Fig + API-first**, not a big-bang rewrite of `NasDna` Web Forms (net48).

Rationale from inventory:

1. Monolith page mass is dominated by **NasSetup / NasBatches / NasForms / WorkFlow** — rewriting those first fails.
2. **Siblings already encode seams**: HITSAPIs, HITSMobileV12 REST, HITSAI, HitsLMS, HITSTKService, client ERec forks.
3. Data access is **multi-era** (DataSets + LINQ-to-SQL + EntLib + EF6 Identity) — new slices should own **new** persistence/API contracts and anti-corrupt against `DNACloudDB` rather than dragging typed DataSets forward.
4. Auth is already dual (SQL Membership + Okta/SAML/OWIN) — new UIs should authenticate via a **modern identity edge**, with Web Forms remaining on current session until cut over.

## Proposed first slice

**Slice A (preferred hypothesis): ERec API + one pilot client**

- Stand up a versioned **ERec service** (applicants, shortlists, interviews) consumed by a new UI.
- Pilot against the smallest fork (`HITSDNAErecCleopatraNew`) or a non-prod tenant.
- Keep `NasDna\ERec\*.aspx` as temporary façade or feature-flagged redirect.
- Success metric: one client path no longer requires NasDna fork for ERec features.

**Slice B (parallel / alternative): NasAI → HITSAI consolidation**

- Route `NasDna\NasAI` screens through HITSAI APIs.
- Lower page count, strategic visibility, validates BFF pattern with less payroll risk.

**Slice C (foundation, can run alongside): Expand HITSAPIs as read-model gateway**

- Employee/org read APIs with modern auth for mobile + future modules.
- Avoid dumping all domains into one god-controller; use bounded route groups.

## Near-term inventory follow-ups (before build)

1. DB schema / stored-proc map for `DNACloudDB` + `hitsstore` + `ASPNETDB` (out of tree).
2. Profile-driven connection string matrix (multi-tenant).
3. Secret hygiene: rotate any Okta client secrets found in Web.config; move to secret store (**values redacted in this pack**).
4. Decide source-of-truth among `HITSNasDnaV12.1` vs `HitsIntegerationV12` vs Gov snapshot.
5. Confirm with Mina: first slice **ERec vs NasAI vs TimeManagement**.

## What not to do next

- Do not start with NasSetup/WorkFlow/NasForms extraction.
- Do not create another full NasDna client fork for a new ERec customer.
- Do not “upgrade in place” to .NET 8 Web Forms — path is strangler to new hosts (API + SPA/Blazor/whatever Mina picks).

---

**Ask for Mina:** Approve or redirect first slice (ERec / NasAI / TimeManagement) and confirm whether client ERec forks must stay binary-compatible during strangulation.
