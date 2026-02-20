// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import type { Role } from '@/lib/schemas';

export function isManager(role: Role | string | null | undefined): boolean {
  return role === 'owner' || role === 'admin';
}
