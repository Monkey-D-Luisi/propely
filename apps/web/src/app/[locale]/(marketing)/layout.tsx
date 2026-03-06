// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import type { ReactNode } from 'react';
import { MarketingNav } from '@/components/layout/MarketingNav';
import { MarketingFooter } from '@/components/layout/MarketingFooter';

export default function MarketingGroupLayout({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-dvh flex flex-col bg-surface">
      <MarketingNav />
      <main className="flex-1 flex flex-col items-center w-full">
        {children}
      </main>
      <MarketingFooter />
    </div>
  );
}
