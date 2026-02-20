// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect } from 'react';
import { useMyOrgs } from '@/hooks/orgs';
import { setActiveOrgId, getActiveOrgId } from '@/lib/org-store';
import type { Org } from '@/lib/schemas';

/**
 * Hook that manages the active organization for AI API calls.
 * - If user has exactly 1 org → auto-selects it.
 * - If user has multiple orgs → returns them for manual selection.
 * - If user has 0 orgs → returns empty state.
 *
 * The selected org ID is stored in the module-level org-store,
 * which is read by aiApiFetch to send the X-Org-Id header.
 */
export function useActiveOrg() {
  const { orgs, isLoading, error } = useMyOrgs(1, 100);

  const currentOrgId = getActiveOrgId();

  useEffect(() => {
    if (isLoading) return;

    if (orgs.length === 1) {
      setActiveOrgId(orgs[0].id);
    } else if (orgs.length > 1 && !currentOrgId) {
      // Auto-select first org if none selected yet
      setActiveOrgId(orgs[0].id);
    } else if (orgs.length === 0) {
      setActiveOrgId(null);
    }
  }, [orgs, isLoading, currentOrgId]);

  const selectOrg = (orgId: string) => {
    setActiveOrgId(orgId);
  };

  const activeOrg: Org | undefined = orgs.find((o) => o.id === getActiveOrgId());

  return {
    orgs,
    activeOrg,
    activeOrgId: getActiveOrgId(),
    isLoading,
    error,
    selectOrg,
    hasOrgs: orgs.length > 0,
    needsSelection: orgs.length > 1,
  };
}
