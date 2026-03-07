// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it } from 'vitest';
import { getStatusColor, paymentStatusColors, subscriptionStatusColors } from './status-colors';

describe('getStatusColor', () => {
  it('returns the mapped color for a known status', () => {
    const colorMap = { active: 'bg-green-100 text-green-800' };
    expect(getStatusColor('active', colorMap)).toBe('bg-green-100 text-green-800');
  });

  it('returns the default color for an unknown status', () => {
    expect(getStatusColor('unknown', {})).toBe('bg-slate-100 text-slate-600');
  });

  it('uses a custom default color when provided', () => {
    expect(getStatusColor('missing', {}, 'bg-red-100')).toBe('bg-red-100');
  });
});

describe('paymentStatusColors', () => {
  it('has entries for succeeded, pending, and failed', () => {
    expect(paymentStatusColors).toHaveProperty('succeeded');
    expect(paymentStatusColors).toHaveProperty('pending');
    expect(paymentStatusColors).toHaveProperty('failed');
  });
});

describe('subscriptionStatusColors', () => {
  it('has entries for active, trialing, pastdue, and cancelled', () => {
    expect(subscriptionStatusColors).toHaveProperty('active');
    expect(subscriptionStatusColors).toHaveProperty('trialing');
    expect(subscriptionStatusColors).toHaveProperty('pastdue');
    expect(subscriptionStatusColors).toHaveProperty('cancelled');
  });
});
