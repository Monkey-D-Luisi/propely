# Epic 006: Billing & Subscriptions

## Overview

Integrate Stripe for subscription billing and one-time payments. This is the core monetization layer for any SaaS product built on this template. Includes plan management, checkout flows, webhook handling, entitlement enforcement, and billing management UI.

## Success Criteria

- Stripe SDK integrated with proper configuration
- Subscription plans definable via configuration
- Users can subscribe to a plan via Stripe Checkout
- Stripe webhooks correctly update subscription status
- Entitlements enforced based on active subscription
- Pricing page displays available plans
- Users can manage billing (upgrade, downgrade, cancel)
- BILLING_MODE toggle (stripe/free) to disable billing for development
- One-time payment support (optional add-on)

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Payment provider | Stripe | Industry standard, excellent API, good .NET SDK |
| Checkout | Stripe Checkout (hosted) | PCI compliant, minimal frontend code, handles SCA |
| Billing portal | Stripe Customer Portal | Built-in upgrade/downgrade/cancel, no custom UI needed |
| Webhook handling | Stripe.net library | Official SDK with signature verification |

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0042 - Stripe integration foundation (SDK, API keys, config)
- **Status:** DONE
- **GitHub Issue:** #194
- **Dependencies:** None
- **File:** `docs/tasks/0042-stripe-foundation.md`
- **Scope:** Add Stripe.net NuGet package. Configure API keys via environment variables. Add BILLING_MODE toggle (stripe/free). Create IPaymentService interface. Create Stripe implementation. Add subscription plan configuration in appsettings.json.
- **Old Issue:** #24

### Task 0043 - Subscription plans and lifecycle
- **Status:** DONE
- **GitHub Issue:** #195
- **Dependencies:** 0042
- **File:** `docs/tasks/0043-subscription-lifecycle.md`
- **Scope:** Create Subscription domain entity. Add checkout session creation endpoint. Handle subscription created/updated/cancelled lifecycle. Link subscriptions to organizations. Add entitlement checking.
- **Old Issue:** #25

### Task 0044 - Stripe webhook handler (events, entitlements)
- **Status:** DONE
- **GitHub Issue:** #196
- **Dependencies:** 0042
- **File:** `docs/tasks/0044-stripe-webhooks.md`
- **Scope:** Create webhook endpoint with Stripe signature verification. Handle checkout.session.completed, invoice.paid, invoice.payment_failed, customer.subscription.updated/deleted events. Update subscription status. Create audit log entries.
- **Old Issue:** #27

### Task 0045 - Pricing page and billing management UI
- **Status:** DONE
- **GitHub Issue:** #197
- **Dependencies:** 0043, 0044
- **File:** `docs/tasks/0045-pricing-billing-ui.md`
- **Scope:** Create pricing page with plan comparison. Add checkout button per plan. Create billing management page (redirect to Stripe Customer Portal). Show current plan and status. Add i18n strings (EN + ES).
- **Old Issue:** #28

### Task 0046 - One-time payments (add-on)
- **Status:** DONE
- **GitHub Issue:** #198
- **Dependencies:** 0042, 0048
- **File:** `docs/tasks/0046-one-time-payments.md`
- **Scope:** Add one-time payment support via Stripe Checkout. Create payment record entity. Handle payment_intent.succeeded webhook. Show payment history.
- **Old Issue:** #26
- **Milestone:** v1.0
- **Roadmap Phase:** B1.3

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Billing | 0042-0046 | 5 | 0 |
| **Total** | **5** | **5** | **0** |

## Dependency Graph

```
0042 (Foundation) ──┬──► 0043 (Subscriptions) ──┐
                    ├──► 0044 (Webhooks) ────────┼──► 0045 (Pricing/Billing UI)
                    └──► 0046 (One-time, v1.0) ◄─── 0048 (Audit log, epic 007)
```
