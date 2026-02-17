# CR-0073: Recovery Runbook Review

## PR Metadata
- **PR:** #288 (`docs(infra): recovery runbook (#0074)`)
- **Branch:** `feat/0074-recovery-runbook` → `main`
- **State:** OPEN
- **CI Status:** No checks configured

## Changed Files
- `docs/recovery-runbook.md`
- `docs/walkthroughs/0074-recovery-runbook.md`
- `docs/tasks/0074-recovery-runbook.md`
- `docs/backlog/epic-013-presale-hardening.md`
- `docs/roadmap-v1.md`

## Review Threads

### Source 1: Inline Review Comments (5 total)
1. Gemini #2807609757 — line 452: Incorrect gcloud command for Redis flush
2. Gemini #2807609759 — line 114: $DB_PASSWORD not defined in PITR script
3. Gemini #2807609772 — line 333: Misleading ack_requeue comment
4. Copilot #2807620679 — line 333: Same as #3 (duplicate root cause)
5. Copilot #2807620685 — line 16: ToC links broken (em dashes vs double hyphens)

### Source 2: Reviews (2 total)
- Gemini: General positive review, no additional issues beyond inline comments
- Copilot: Summary review, no additional issues beyond inline comments

### Source 3: Issue Comments (2 total)
- ChatGPT Codex: Usage limit notification (not actionable)
- Gemini: Summary of changes (not actionable)

## Comment Resolution Plan

### MUST_FIX (3 items, 2 root causes)

- [x] **#1 (Gemini)**: Remove incorrect `gcloud redis instances update --update-redis-config="activedefrag=yes"` command. This configures defragmentation, not flushing. The note already says gcloud doesn't expose FLUSHALL.
- [x] **#3/#4 (Gemini + Copilot, same root cause)**: Fix misleading comment on line 333. The comment says "ack_requeue=false means messages stay in the queue" but the actual parameter is `ack_requeue_true`. Fix to accurately describe peek behavior.
- [x] **#5 (Copilot)**: Fix broken ToC links. Headings use `--` (double hyphens) but GitHub Markdown anchor generation may vary. Standardize headings to use em dashes to match ToC links.

### SHOULD_FIX (1 item)

- [x] **#2 (Gemini)**: Add `$DB_PASSWORD` fetch from Secret Manager before constructing connection string in PITR script. Makes the script self-contained during incidents.

## Behavioral Parity Checks

All N/A — this is a documentation-only PR with no code changes:
- [x] Redirect parity: N/A (no auth flows)
- [x] Locale source correctness: N/A (no localized content)
- [x] API/UI contract parity: N/A (no API changes)
- [x] Test parity: N/A (no behavior changes)
