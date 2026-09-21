# Cursor setup - agents, skills, and rules

How to install and use the BEST-OF pack from `templates/` in a Cursor project.

## Install (copy into your repo)

From this pack's `templates/` directory into your project root:

```bash
# Docs orientation
cp templates/AGENTS.md your-repo/
# (also copy PRD, TASKS, RULES, etc. as in QUICKSTART)

# Cursor config
mkdir -p your-repo/.cursor/agents your-repo/.cursor/skills your-repo/.cursor/rules
cp templates/.cursor/agents/*.md your-repo/.cursor/agents/
cp -R templates/.cursor/skills/* your-repo/.cursor/skills/
cp templates/.cursor/rules/*.mdc your-repo/.cursor/rules/
```

Replace `{{PLACEHOLDERS}}` in docs and rule stubs. Agents and skills intentionally say "read project docs" - they do not hard-code a product.

Open the project in Cursor. Agents appear under **Agents** / subagent pickers; skills appear as skills (slash skills when `disable-model-invocation: true`).

## What you get

### Agents (`.cursor/agents/*.md`)
| Agent | Role | readonly |
|---|---|---|
| `planner` | Plan ONE next TASKS item from docs | true |
| `implementer` | Implement that slice (smallest change) | false |
| `reviewer` | Doc-aligned review; no code changes | true |
| `verifier` | Run tests; prove claims; use after "done" | false |

Frontmatter fields used: `name`, `description`, `model`, `readonly` (optional `is_background`). No `tools` field.

### Skills (`.cursor/skills/<name>/SKILL.md`)
| Skill | When | Slash-only |
|---|---|---|
| `six-part-prompt` | Build CONTEXT/TASK/FILES/CONSTRAINTS/ACCEPTANCE/TESTING | yes (`/six-part-prompt`) |
| `vertical-slice` | Starting a feature - thin E2E path first | no |
| `structured-debug` | Something broke - diagnose then smallest fix | no |
| `docs-sync` | After a completed slice - update TASKS/MEMORY/... | no |
| `ship-check` | Before preview/production | yes (`/ship-check`) |

Folder name must match the `name` field in each `SKILL.md`.

### Rules (`.cursor/rules/*.mdc`)
Always-apply / glob rules for general, frontend, backend, testing. Complement `RULES.md`.

## Daily workflow

1. **Plan** - invoke `planner` (or ask Agent to use it) for the next TASKS.md item.  
2. **Implement** - `implementer`, or `/six-part-prompt` then implement. Use `vertical-slice` when starting a feature.  
3. **Verify** - `verifier` (proactively after marking done). Use `structured-debug` on failures.  
4. **Review** (optional but recommended before merge) - `reviewer`.  
5. **Docs** - `docs-sync`. Commit.  
6. **Ship** - `/ship-check` before preview or production.

## Tips
- Keep agent prompts short; long prompts dilute focus.
- Descriptions tell Cursor **when** to delegate - leave them specific.
- Tie work to PRD / TASKS / RULES; do not invent MVP features in chat.
- See `QUICKSTART.md` and `VIBE_CODING_GUIDE.md` for the full docs-first pipeline.
