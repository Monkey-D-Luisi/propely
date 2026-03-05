# Epic P6 -- Portal Reference (Deprioritized)

## Status: DEPRIORITIZED

Portal publication is not actively developed. This document preserves the original specification for reference but all tasks are deferred.

## Overview

The original goal was to enable real estate agents to publish property listings to external portals (Kyero, Thribee/Trovit/Mitula/Nestoria/Nuroa, SpainHouses) and receive leads back from those portals. This has been **deprioritized** in favor of the AI Action Engine (Phase 3) as the platform's primary differentiator.

## What is kept

1. **Portal schemas as field reference** — The XML schemas from Kyero v3.9, SpainHouses XSD, and Thribee are used as reference documentation for the Property domain model (Phase 2, task 2.1). Fields like catastro reference, IBI tax, community fees, energy certificate class/value, INE province/municipality codes, and property type taxonomies are included in the property model because portals require them and they represent the standard real estate data model in Spain.

2. **Service scaffold** — `services/publishing-api/` exists as a Clean Architecture shell with all 4 layers scaffolded. It compiles and is included in CI, but has no domain logic.

3. **Lead intake webhooks (recoverable)** — Task 6.6 (Lead Intake Webhooks from portals) remains a useful feature even without publication. When portals send leads via webhook, they can be routed to the contacts/leads system. This task can be recovered from the backlog when there is business demand.

## What is deferred

All 7 original tasks are deferred from the active roadmap:

| # | Title | Original Status | New Status |
|---|---|---|---|
| 6.1 | Publishing Domain Model | PENDING | DEFERRED |
| 6.2 | Portal Adapter: Kyero | PENDING | DEFERRED |
| 6.3 | Portal Adapter: Thribee | PENDING | DEFERRED |
| 6.4 | Portal Adapter: SpainHouses | PENDING | DEFERRED |
| 6.5 | Publishing Worker | PENDING | DEFERRED |
| 6.6 | Lead Intake Webhooks | PENDING | DEFERRED (recoverable) |
| 6.7 | Frontend Publication Management | PENDING | DEFERRED |

## Recovery path

If portal publication becomes a business priority:

1. Recover task specs from this document (original specification preserved below the fold)
2. Re-add tasks to `docs/roadmap.md` as a new phase or appended to an existing one
3. The `publishing-api` scaffold is ready — no additional infrastructure work needed
4. The property model (Phase 2) already includes portal-informed fields, so no domain changes required

## Portal Schema Reference (for property model fields)

### Kyero v3.9 required fields
`id`, `date`, `ref`, `price`, `currency`, `type`, `town`, `province`, `country`, `location_detail`, `beds`, `baths`, `surface_area`

### Kyero v3.9 optional fields
`pool`, `parking`, `garden`, `url`, `desc` (multi-language: en, es), `images`, `features`, `energy_rating`

### SpainHouses required fields
`id`, `reference`, `operation` (sale/rent), `type`, `price`, `province_code` (INE 2-digit), `municipality_code` (INE 5-digit), `address`

### SpainHouses optional fields
`catastro_reference`, `ibi_tax`, `community_fees`, `energy_certificate_class` (A-G), `energy_certificate_value`

### SpainHouses property types (Spanish)
piso, atico, bajo, duplex, adosado, chalet, finca, cortijo, local, oficina, nave, solar, garaje

### Thribee required fields
`id`, `url`, `title`, `type` (For Sale/For Rent), `property_type`, `price`, `currency`, `city`, `region`, `latitude`, `longitude`

### Thribee optional fields
`description`, `bedrooms`, `bathrooms`, `floor_area`, `plot_area`, `parking`, `pool`, `year_built`, `energy_certificate`

---

## Original task specifications (preserved for recovery)

The full original task specifications for tasks 6.1 through 6.7 are available in git history (commit prior to the roadmap restructuring). If recovery is needed, check out the previous version of this file.
