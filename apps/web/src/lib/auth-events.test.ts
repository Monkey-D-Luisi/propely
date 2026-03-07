// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi } from 'vitest';
import { AUTH_USER_UPDATED_EVENT, dispatchAuthUserUpdatedEvent } from './auth-events';

describe('dispatchAuthUserUpdatedEvent', () => {
  it('dispatches a custom event on window', () => {
    const spy = vi.spyOn(window, 'dispatchEvent');
    dispatchAuthUserUpdatedEvent();
    expect(spy).toHaveBeenCalledWith(expect.any(Event));
    const event = spy.mock.calls[0][0] as Event;
    expect(event.type).toBe(AUTH_USER_UPDATED_EVENT);
    spy.mockRestore();
  });

  it('exports the correct event name', () => {
    expect(AUTH_USER_UPDATED_EVENT).toBe('auth:user-updated');
  });
});
