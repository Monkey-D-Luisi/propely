// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it } from 'vitest';
import { isManager } from './roles';

describe('isManager', () => {
  it('returns true for owner', () => {
    expect(isManager('owner')).toBe(true);
  });

  it('returns true for admin', () => {
    expect(isManager('admin')).toBe(true);
  });

  it('returns false for member', () => {
    expect(isManager('member')).toBe(false);
  });

  it('returns false for null', () => {
    expect(isManager(null)).toBe(false);
  });

  it('returns false for undefined', () => {
    expect(isManager(undefined)).toBe(false);
  });
});
