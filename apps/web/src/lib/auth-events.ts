// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

export const AUTH_USER_UPDATED_EVENT = "auth:user-updated";

export function dispatchAuthUserUpdatedEvent() {
  if (typeof window === "undefined") {
    return;
  }

  window.dispatchEvent(new Event(AUTH_USER_UPDATED_EVENT));
}
