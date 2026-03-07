// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it } from 'vitest';
import { formatDateShort, formatDateLong, formatDateTime } from './format';

describe('formatDateShort', () => {
  it('formats a valid ISO date string', () => {
    const result = formatDateShort('2026-01-15T00:00:00Z', 'en-US');
    expect(result).toContain('2026');
    expect(result).toContain('15');
  });

  it('returns the raw string for invalid dates', () => {
    expect(formatDateShort('not-a-date')).toBe('not-a-date');
  });
});

describe('formatDateLong', () => {
  it('formats a valid ISO date string with time', () => {
    const result = formatDateLong('2026-06-20T14:30:00Z', 'en-US');
    expect(result).toContain('2026');
    expect(result).toContain('20');
  });

  it('returns the raw string for invalid dates', () => {
    expect(formatDateLong('invalid')).toBe('invalid');
  });
});

describe('formatDateTime', () => {
  it('formats a valid ISO date string with time and seconds', () => {
    const result = formatDateTime('2026-03-10T09:15:30Z', 'en-US');
    expect(result).toContain('2026');
    expect(result).toContain('10');
  });

  it('returns the raw string for invalid dates', () => {
    expect(formatDateTime('bad-date')).toBe('bad-date');
  });
});
