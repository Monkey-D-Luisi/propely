// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonText } from '@/components/ui/skeleton';

export default function AgencyDetailLoading() {
  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <Skeleton className="h-7 w-48" />
            <Skeleton className="mt-2 h-4 w-64" />
          </div>
          <Skeleton className="h-9 w-28 rounded-lg" />
        </div>

        {/* Stats cards */}
        <div className="grid gap-4 md:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
              <Skeleton className="h-4 w-20" />
              <Skeleton className="mt-2 h-7 w-16" />
            </div>
          ))}
        </div>

        {/* Details */}
        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-5 w-32" />
          <SkeletonText className="mt-4" lines={3} widths={['w-full', 'w-3/4', 'w-1/2']} />
        </div>
      </div>
    </div>
  );
}
