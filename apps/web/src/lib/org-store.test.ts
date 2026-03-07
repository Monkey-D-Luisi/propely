// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it } from 'vitest';
import { getActiveOrgId, setActiveOrgId } from './org-store';

describe('org-store', () => {
  it('returns null initially', () => {
    setActiveOrgId(null);
    expect(getActiveOrgId()).toBeNull();
  });

  it('stores and retrieves an org ID', () => {
    setActiveOrgId('org-123');
    expect(getActiveOrgId()).toBe('org-123');
  });

  it('can be cleared by setting null', () => {
    setActiveOrgId('org-456');
    setActiveOrgId(null);
    expect(getActiveOrgId()).toBeNull();
  });
});
