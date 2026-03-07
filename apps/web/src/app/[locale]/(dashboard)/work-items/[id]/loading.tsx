// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonText } from '@/components/ui/skeleton';

export default function WorkItemDetailLoading() {
  return (
    <div className="flex-1 overflow-y-auto p-8">
      <div className="mx-auto flex max-w-[1200px] flex-col gap-6">
        {/* Breadcrumb */}
        <Skeleton className="h-4 w-44" />

        {/* Title + actions */}
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-56" />
          <div className="flex gap-2">
            <Skeleton className="h-9 w-20 rounded-lg" />
            <Skeleton className="h-9 w-20 rounded-lg" />
          </div>
        </div>

        {/* Status badge */}
        <Skeleton className="h-6 w-20 rounded-full" />

        {/* Details */}
        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-5 w-28" />
          <SkeletonText className="mt-4" lines={4} widths={['w-full', 'w-3/4', 'w-full', 'w-1/2']} />
        </div>

        {/* Additional info */}
        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-5 w-36" />
          <SkeletonText className="mt-4" lines={3} widths={['w-full', 'w-2/3', 'w-3/4']} />
        </div>
      </div>
    </div>
  );
}
