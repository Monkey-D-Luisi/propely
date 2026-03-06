// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import type { ReactNode } from 'react';
import { AppTopHeader } from '@/components/layout/AppTopHeader';

export default function AppGroupLayout({ children }: { children: ReactNode }) {
  return (
    <div className="relative flex min-h-dvh w-full flex-col overflow-x-hidden">
      <AppTopHeader />
      <main className="flex-1 w-full">
        {children}
      </main>
    </div>
  );
}
