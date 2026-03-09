// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonTable } from '@/components/ui/skeleton';

export default function WorkItemsLoading() {
  return (
    <div className="flex-1 overflow-y-auto p-8">
      <div className="mx-auto flex max-w-6xl flex-col gap-6">
        {/* Header */}
        <div className="flex flex-col gap-4 border-b border-slate-100 pb-6 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <Skeleton className="h-8 w-36" />
            <Skeleton className="mt-2 h-4 w-52" />
          </div>
          <Skeleton className="h-9 w-36 rounded-lg" />
        </div>

        {/* Filter bar */}
        <div className="flex flex-wrap items-center gap-3">
          <Skeleton className="h-10 w-64 rounded-lg" />
          <Skeleton className="h-10 w-32 rounded-lg" />
        </div>

        {/* Table */}
        <SkeletonTable rows={8} columns={5} />
      </div>
    </div>
  );
}
