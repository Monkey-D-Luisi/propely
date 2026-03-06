// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import type { ReactNode } from 'react';
import { AppSidebar } from '@/components/layout/AppSidebar';

export default function DashboardGroupLayout({ children }: { children: ReactNode }) {
  return (
    <div className="h-dvh flex overflow-hidden">
      <AppSidebar />
      <main className="flex-1 flex flex-col h-full overflow-hidden">
        {children}
      </main>
    </div>
  );
}
