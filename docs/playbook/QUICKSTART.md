# Vibe Coding Quickstart

> Minimum path from idea to preview in one focused session.  
> Full playbook: [`VIBE_CODING_GUIDE.md`](./VIBE_CODING_GUIDE.md)

## 1. Answer the five questions (10 min)

Write them in a scratch note or start `PRD.md`:

1. Problem  
2. User  
3. Outcome  
4. MVP  
5. Out of scope  

**PulseBoard sketch:** busy people lose habit streaks -> one daily check-in app -> signup + <=5 habits + checkbox + streak -> no social/AI coaching.

## 2. Copy starter docs (5 min)

From `templates/` into your new repo:

- `PRD.md`, `TASKS.md`, `RULES.md`, `AGENTS.md`  
- `.env.example`  
- Optional later: `ARCHITECTURE.md`, `DESIGN.md`, `DECISIONS.md`, `MEMORY.md`, `TEST_PLAN.md`, `SECURITY.md`  
- Cursor: `.cursor/rules/*.mdc`, `.cursor/agents/*.md`, `.cursor/skills/*/SKILL.md`

Replace `HITSHCM` and other placeholders. Full install steps: [`CURSOR_SETUP.md`](./CURSOR_SETUP.md).

## 3. Scaffold + git (15 min)

```bash
npx create-next-app@latest HITSHCM --typescript --tailwind --eslint --app
cd HITSHCM
# copy templates in
git init && git add . && git commit -m "chore: initial scaffold"
```

Create a Supabase project; put **empty key names** in `.env.example` and real values only in `.env.local` (gitignored).

Verify:

```bash
node -v && npm -v && git --version
npm install && npm run dev
```

## 4. Break work into vertical slices (15 min)

In `TASKS.md`, list 8-12 slices, e.g.:

1. Health page  
2. Auth  
3. Habit create/list  
4. Check-in toggle  
5. Streak display  
6. E2E smoke  

## 5. Build with the six-part prompt (loop)

For each slice:

```text
CONTEXT: ...  TASK: ...  FILES: ...  CONSTRAINTS: ...  ACCEPTANCE CRITERIA: ...  TESTING: ...
```

Then: implement -> test -> review vs docs -> commit -> update TASKS.

## 6. Cursor agents & skills

After templates are copied (see [`CURSOR_SETUP.md`](./CURSOR_SETUP.md)):

| Step | Agent / skill |
|---|---|
| Plan next TASKS item | `planner` |
| Implement one slice | `implementer` or `/six-part-prompt` |
| Prove it works | `verifier` |
| Sync docs | `docs-sync` |
| Before preview/prod | `/ship-check` |

Optional: `reviewer` before merge; `vertical-slice` when starting a feature; `structured-debug` when something breaks.

## 7. Preview deploy

Push to GitHub -> Vercel project -> open preview URL -> run the happy path.

## 8. Do not skip

- Git from day one  
- One feature at a time  
- Preview before calling it production  
- No secrets in git or chat  

## Done when

- [ ] MVP happy path works on preview  
- [ ] Env vars set for that environment  
- [ ] At least one written test or checklist executed  
- [ ] PRD/TASKS still match what you built  

Next: read **Part 5-6** of the playbook (Ship / Run) before real users.
