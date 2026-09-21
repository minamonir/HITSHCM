# Manifest - Vibe Coding Guide Pack

> What this pack is: an original beginner-to-production playbook for structured AI-assisted development ("vibe coding"), plus fill-in templates, Cursor rules, and a lean BEST-OF Cursor agents + skills pack. It teaches a docs-first pipeline from idea through preview, production, and iteration, with PulseBoard (habit tracker) as a running case study. Copy templates into a new repo, follow QUICKSTART for a weekend MVP, install agents/skills via CURSOR_SETUP.md, and use the full guide when you need professional structure.

## Files created

### Playbook (pack root)
- `QUICKSTART.md`
- `VIBE_CODING_GUIDE.md`
- `MANIFEST.md`
- `CURSOR_SETUP.md`

### Project templates
- `templates/AGENTS.md`
- `templates/ARCHITECTURE.md`
- `templates/DECISIONS.md`
- `templates/DESIGN.md`
- `templates/MEMORY.md`
- `templates/PRD.md`
- `templates/README.md`
- `templates/RULES.md`
- `templates/SECURITY.md`
- `templates/TASKS.md`
- `templates/TEST_PLAN.md`
- `templates/.env.example`

### Cursor rules
- `templates/.cursor/rules/backend.mdc`
- `templates/.cursor/rules/frontend.mdc`
- `templates/.cursor/rules/general.mdc`
- `templates/.cursor/rules/testing.mdc`

### Cursor agents
- `templates/.cursor/agents/planner.md`
- `templates/.cursor/agents/implementer.md`
- `templates/.cursor/agents/reviewer.md`
- `templates/.cursor/agents/verifier.md`

### Cursor skills
- `templates/.cursor/skills/six-part-prompt/SKILL.md`
- `templates/.cursor/skills/vertical-slice/SKILL.md`
- `templates/.cursor/skills/structured-debug/SKILL.md`
- `templates/.cursor/skills/docs-sync/SKILL.md`
- `templates/.cursor/skills/ship-check/SKILL.md`

## How to use

1. Read `QUICKSTART.md` for the minimum path.
2. Read `CURSOR_SETUP.md` to install agents, skills, and rules in Cursor.
3. Read `VIBE_CODING_GUIDE.md` for the full Parts 0-6 playbook (+ Appendix G for Cursor agents/skills).
4. Copy everything under `templates/` into your project (adjust paths as needed).
5. Replace `{{PLACEHOLDERS}}` in docs/rules before prompting the AI.
6. Daily loop: planner -> implementer -> verifier -> docs-sync (reviewer before merge; `/ship-check` before preview/prod).
