// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useCurrentUser } from '@/hooks/orgs';

/**
 * Redirects unauthenticated users to /login.
 * Returns the same shape as useCurrentUser so callers can
 * destructure { user, isLoading } without an extra hook call.
 */
export function useRequireAuth() {
  const { user, isLoading } = useCurrentUser();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && !user) {
      router.replace('/login');
    }
  }, [isLoading, user, router]);

  return { user, isLoading };
}
