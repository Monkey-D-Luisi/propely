// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import type { ReactNode } from 'react';
import { ToastProvider } from '@/components/ui/toast';
import { FeatureFlagProvider } from '@/hooks/feature-flags';
import { CommandBarProvider } from '@/hooks/use-command-bar';

export function Providers({ children }: { children: ReactNode }) {
  return (
    <ToastProvider>
      <FeatureFlagProvider>
        <CommandBarProvider>{children}</CommandBarProvider>
      </FeatureFlagProvider>
    </ToastProvider>
  );
}