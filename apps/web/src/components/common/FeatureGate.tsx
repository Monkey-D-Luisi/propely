// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import type { ReactNode } from 'react';
import { useFeatureFlag } from '@/hooks/feature-flags';

type Props = {
  flag: string;
  children: ReactNode;
  fallback?: ReactNode;
};

export function FeatureGate({ flag, children, fallback = null }: Props) {
  const { isEnabled, isLoading } = useFeatureFlag(flag);

  if (isLoading) return null;
  if (!isEnabled) return <>{fallback}</>;

  return <>{children}</>;
}
