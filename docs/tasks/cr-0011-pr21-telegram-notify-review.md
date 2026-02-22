# CR-0011: PR #21 — Telegram Copilot Notification Review

## PR Metadata

- **PR**: [#21 — chore(ci): add Telegram notification for Copilot code reviews](https://github.com/Monkey-D-Luisi/propely/pull/21)
- **Branch**: `chore/telegram-copilot-notification` → `main`
- **CI Status**: All checks green (SUCCESS or SKIPPED where expected)

## Changed Files

| File | +/- |
|------|-----|
| `.github/workflows/notify-copilot-review.yml` | +23 |

---

## Section 1: Agent Review Findings

| # | Severity | Category | File:Line | Description | Suggested Fix |
|---|----------|----------|-----------|-------------|---------------|
| A1 | MUST_FIX | Security | notify-copilot-review.yml:9 | Missing `permissions` block — grants default token permissions unnecessarily | Add `permissions: {}` since workflow only makes an external HTTP call |
| A2 | SHOULD_FIX | Consistency | notify-copilot-review.yml:6 | Missing `defaults: run: shell: bash` block present in all other workflows | Add defaults section after `on:` block |
| A3 | SHOULD_FIX | Consistency | notify-copilot-review.yml:10 | Hardcoded `ubuntu-latest` instead of `${{ vars.RUNNER_LABEL || 'ubuntu-latest' }}` | Use variable pattern matching ci.yml, claude.yml, infra-ci.yml |
| A4 | MUST_FIX | Code Quality | notify-copilot-review.yml:23 | Spanish text ("Copilot terminó el code review", "Estado", "Ver review") violates "English only in repo" rule | Translate to English |
| A5 | MUST_FIX | Security | notify-copilot-review.yml:20-23 | PR_TITLE is not escaped before use in Telegram message — shell metacharacters or Markdown special chars could cause injection or parse errors | Use jq to build JSON payload safely |
| A6 | SHOULD_FIX | Code Quality | notify-copilot-review.yml:20 | `-sf` silently suppresses errors; debugging workflow failures becomes impossible | Remove `-s` to allow error output |
| A7 | SHOULD_FIX | Code Quality | notify-copilot-review.yml:22-23 | Using `%0A` URL-encoding for newlines is fragile; JSON POST body via jq is standard practice | Switch to JSON Content-Type with jq |
| A8 | MUST_FIX | Process | general | Missing walkthrough file per project governance rules | Create `docs/walkthroughs/cr-0011-pr21-telegram-notify-review.md` |

---

## Section 2: Review Comment Threads (Copilot bot)

| # | Comment ID | Path:Line | Summary | Classification |
|---|-----------|-----------|---------|----------------|
| C1 | 2838162355 | yml:9 | Missing `permissions` block | MUST_FIX — overlaps A1 |
| C2 | 2838162330 | yml:6 | Missing `defaults: run: shell: bash` | SHOULD_FIX — overlaps A2 |
| C3 | 2838162323 | yml:10 | Hardcoded runner label | SHOULD_FIX — overlaps A3 |
| C4 | 2838162350 | yml:23 | Spanish text violates English-only rule | MUST_FIX — overlaps A4 |
| C5 | 2838162335 | yml:23 | Unescaped PR_TITLE injection risk; use jq | MUST_FIX — overlaps A5 |
| C6 | 2838162341 | yml:20 | `-sf` suppresses errors | SHOULD_FIX — overlaps A6 |
| C7 | 2838162345 | yml:23 | Markdown special chars not escaped; suggests HTML mode | SHOULD_FIX — subsumed by A5/C5 jq fix |
| C8 | 2838162357 | yml:1 | Missing walkthrough file | MUST_FIX — overlaps A8 |

---

## Resolution Plan

### MUST_FIX

- [x] A1/C1: Add `permissions: {}` to job
- [x] A4/C4: Translate all text to English
- [x] A5/C5/C7: Use jq to build JSON payload, proper escaping, switch to HTML parse mode
- [x] A8/C8: Create walkthrough file

### SHOULD_FIX

- [x] A2/C2: Add `defaults: run: shell: bash`
- [x] A3/C3: Use `${{ vars.RUNNER_LABEL || 'ubuntu-latest' }}`
- [x] A6/C6: Remove `-s` from curl, keep `-f`
- [x] A7: Use JSON Content-Type with jq (implemented as part of A5)
