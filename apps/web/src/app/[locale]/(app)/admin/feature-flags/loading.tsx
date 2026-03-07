// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton } from '@/components/ui/skeleton';

export default function FeatureFlagsLoading() {
  return (
    <main className="flex-1 w-full max-w-5xl mx-auto px-10 py-8">
      <Skeleton className="h-8 w-48" />
      <div className="mt-6 space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <div
            key={i}
            className="flex items-center justify-between rounded-lg border border-slate-200 bg-white p-4 shadow-sm"
          >
            <div className="space-y-2">
              <Skeleton className="h-5 w-40" />
              <Skeleton className="h-3 w-64" />
            </div>
            <Skeleton className="h-6 w-12 rounded-full" />
          </div>
        ))}
      </div>
    </main>
  );
}
