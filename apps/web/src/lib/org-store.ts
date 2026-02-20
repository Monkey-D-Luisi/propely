// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

/**
 * In-memory store for the active organization ID.
 * Used to send X-Org-Id header on AI API requests so the backend
 * knows which tenant context to apply.
 */

let activeOrgId: string | null = null;

export function getActiveOrgId(): string | null {
  return activeOrgId;
}

export function setActiveOrgId(id: string | null): void {
  activeOrgId = id;
}
