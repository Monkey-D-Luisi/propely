# Licensing Guide

This guide explains the SaaS Starter Kit license in plain language. It covers what you can do with the template, what restrictions apply, and answers common questions. For the full legal terms, refer to [LICENSE](../LICENSE) and [EULA](../EULA.md) in the repository root.

> **Disclaimer:** This guide is a plain-language summary for convenience. It does not constitute legal advice. If there is any conflict between this guide and the LICENSE or EULA, the legal documents take precedence. Consult a qualified attorney for legal questions specific to your situation.

---

## What You Get

When you purchase the SaaS Starter Kit, you receive:

- **Full source code** for the entire monorepo (Next.js frontend, .NET backend services, infrastructure configuration)
- **Documentation** including setup guides, developer cookbook, and architecture references
- **Scripts and tooling** for development, deployment, testing, and CI/CD
- **Configuration files** for Docker Compose, Terraform, GitHub Actions, and more
- **Third-party dependency notices** listing all open-source packages used (see [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md))

Your license is **perpetual** — it does not expire as long as you follow the terms.

---

## What You Can Do

### Build products and services

You can use the template to build any SaaS application, internal tool, or web service. This is the primary intended use. For example:

- A project management tool for your company
- A subscription-based analytics dashboard you sell to customers
- An internal HR portal for your organization
- A marketplace platform with paid listings

### Modify anything

You have full rights to modify, extend, or customize any part of the source code. Change the UI, add new features, remove features you don't need, swap out libraries, restructure the architecture — it's your codebase to shape.

### Deploy to production

Deploy the software (original or modified) to any hosting environment: cloud providers, VPS, on-premises servers, containerized infrastructure, or serverless platforms. There are no restrictions on where or how you deploy.

### Build for clients (agency use)

If you run an agency or consultancy, you can use the template to build End Products for your clients. Each developer on your team who accesses the source code needs their own license (see [Team and Organization Use](#team-and-organization-use) below).

### Multiple projects

Use the template across multiple projects owned by you or your organization. A single license covers all projects for one developer.

---

## What You Cannot Do

### Redistribute the source code

You may not share, give away, upload, or otherwise distribute the template's source code (in whole or in part) to anyone who does not have their own license. This includes:

- Sharing the repository with a friend or colleague who hasn't purchased a license
- Uploading the template code to a public GitHub repository, GitLab, Bitbucket, or any other public hosting
- Including the template source code in a blog post, tutorial, or educational course
- Bundling the template source code in a product you sell

### Resell or sublicense

You may not resell, sublicense, or re-distribute the template as a product. Purchasing a license does not give you distribution rights.

### Create competing templates

You may not use the template to create and sell a competing SaaS template, starter kit, boilerplate, or similar developer-focused foundational product. Building and selling a SaaS *application* is perfectly fine — building and selling a SaaS *template* derived from this code is not.

### Remove license notices

Keep the copyright notices and license headers in the source files. These appear as comments at the top of each file and are not visible to your end users. Do not remove the `LICENSE` or `EULA.md` files from your private copy of the repository.

---

## Team and Organization Use

- A standard license is a **single-seat license** for one named individual.
- Every person who accesses the source code (including employees, contractors, and collaborators) needs their own license.
- Multi-seat team or organization bundles may be available — check the purchase page for current options and pricing.

**Example:** A 3-person development team needs 3 licenses. A designer who only sees the deployed application (not the source code) does not need a license.

---

## Attribution

- You are **not required** to show any attribution in your deployed applications. Your end users will never see a "Powered by SaaS Starter Kit" notice unless you choose to add one.
- You **must retain** the copyright headers in the source files (`// Copyright (c) 2026 SaaS Starter Kit...`). These are developer-facing only and do not affect end users.
- You **must keep** the `LICENSE` and `EULA.md` files in your private repository.

---

## Updates and Support

### Checking for updates

The template includes a built-in version check feature. Administrators can check for new releases from the admin panel (Settings > Version). This feature is opt-in and controlled by the `UpdateCheck` feature flag.

### Receiving updates

Update entitlements depend on the purchase terms at the time of acquisition. Check your purchase confirmation for details on what's included (e.g., lifetime updates, 1-year updates, etc.).

### Support channels

For licensing questions or technical support, contact the Licensor through the channels listed on the purchase page. Support terms (response time, scope of assistance) are defined separately from the license itself.

---

## Frequently Asked Questions

### Can I use this for client projects?

**Yes.** You can build End Products for clients using the template. Each developer on your team who accesses the source code needs their own license, but your clients do not need licenses to use the deployed product.

### Can I modify the code?

**Yes.** You have full rights to modify any part of the source code. Change the UI, swap libraries, add features, restructure the architecture — whatever your project requires.

### Can I use it for multiple projects?

**Yes.** A single license covers unlimited projects for one developer. You can reuse the template as a starting point for as many applications as you want.

### Can I deploy to any cloud provider?

**Yes.** There are no deployment restrictions. AWS, Google Cloud, Azure, DigitalOcean, Hetzner, on-premises — deploy wherever you choose.

### Can I share the code with my team?

**Only if each team member has their own license.** A standard license is a single-seat license. Every developer who accesses the source code needs their own valid license.

### Can I publish my modifications as open source?

**No.** You may not publish any part of the template's source code (original or modified) in a public repository. Your modifications to the template must remain private. However, this restriction applies to the template's codebase; you are free to publish your own separate, original projects as open source, provided they do not include any source code from this template.

### Can I build a SaaS app and sell subscriptions?

**Yes.** This is the intended use case. Build your SaaS application using the template and charge your customers whatever you like. The license places no restrictions on your End Product's pricing or business model.

### Can I build another starter kit with this code?

**No.** Creating a competing SaaS template, starter kit, or boilerplate derived from this code is explicitly prohibited. The distinction is: selling a *product built with* the template is allowed; selling a *template derived from* the template is not.

### What happens if I violate the license?

The Licensor may terminate your license. Upon termination, you must stop using the template for new development and delete all copies of the source code (except copies embedded in End Products already deployed before termination). End Products deployed before termination can continue to operate.

### Is there a refund policy?

Refund terms are defined in the purchase terms, not in the license itself. Check the purchase page for the current refund policy.

### Do I need to display "Powered by" branding?

**No.** There is no attribution requirement in your deployed applications. The only requirement is to keep the copyright headers in the source files, which are not visible to end users.

### What third-party licenses does the template use?

The template depends on many open-source packages (MIT, Apache-2.0, BSD, etc.). A complete list is available in [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md). These open-source licenses apply to their respective packages and do not affect the proprietary license of the template itself.

---

## Quick Reference

| Question | Answer |
|----------|--------|
| Build a SaaS app and sell it | Allowed |
| Modify the source code | Allowed |
| Deploy to any environment | Allowed |
| Use for client projects (agency) | Allowed (each dev needs a license) |
| Use for multiple projects | Allowed |
| Share source code with unlicensed users | **Not allowed** |
| Publish source code publicly | **Not allowed** |
| Resell or redistribute the template | **Not allowed** |
| Build a competing template/kit | **Not allowed** |
| Remove license headers from source | **Not allowed** |
| Visible attribution in end product | Not required |

---

## Legal Documents

For the complete legal terms, refer to:

- [LICENSE](../LICENSE) — Proprietary Software License (summary of terms)
- [EULA.md](../EULA.md) — End User License Agreement (full legal terms)
- [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md) — Third-party dependency licenses
