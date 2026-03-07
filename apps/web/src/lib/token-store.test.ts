// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it } from 'vitest';
import { getAccessToken, setAccessToken, clearAccessToken } from './token-store';

describe('token-store', () => {
  it('returns null initially', () => {
    clearAccessToken();
    expect(getAccessToken()).toBeNull();
  });

  it('stores and retrieves a token', () => {
    setAccessToken('jwt-abc-123');
    expect(getAccessToken()).toBe('jwt-abc-123');
  });

  it('clears the token', () => {
    setAccessToken('some-token');
    clearAccessToken();
    expect(getAccessToken()).toBeNull();
  });

  it('can set token to null explicitly', () => {
    setAccessToken('token');
    setAccessToken(null);
    expect(getAccessToken()).toBeNull();
  });
});
