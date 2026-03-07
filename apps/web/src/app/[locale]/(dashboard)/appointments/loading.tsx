// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton } from '@/components/ui/skeleton';

export default function AppointmentsLoading() {
  return (
    <div className="flex-1 overflow-y-auto p-8">
      <div className="mx-auto flex max-w-4xl flex-col gap-6">
        {/* Header */}
        <div className="flex flex-col gap-4 border-b border-slate-100 pb-6 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <Skeleton className="h-8 w-44" />
            <Skeleton className="mt-2 h-4 w-56" />
          </div>
          <Skeleton className="h-9 w-44 rounded-lg" />
        </div>

        {/* Filters */}
        <div className="flex flex-wrap items-center gap-3">
          <Skeleton className="h-10 w-36 rounded-lg" />
          <Skeleton className="h-10 w-36 rounded-lg" />
        </div>

        {/* Calendar skeleton */}
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <Skeleton className="h-10 w-full rounded-lg" />
          <Skeleton className="mt-4 h-[600px] w-full rounded-xl" />
        </div>
      </div>
    </div>
  );
}
