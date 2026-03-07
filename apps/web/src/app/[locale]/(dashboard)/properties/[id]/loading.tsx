// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonText } from '@/components/ui/skeleton';

export default function PropertyDetailLoading() {
  return (
    <div className="flex-1 overflow-y-auto p-8">
      <div className="mx-auto flex max-w-[1200px] flex-col gap-6">
        {/* Breadcrumb */}
        <Skeleton className="h-4 w-48" />

        {/* Title + actions */}
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-64" />
          <div className="flex gap-2">
            <Skeleton className="h-9 w-20 rounded-lg" />
            <Skeleton className="h-9 w-20 rounded-lg" />
          </div>
        </div>

        {/* Photo gallery */}
        <Skeleton className="h-64 w-full rounded-xl" />

        {/* Details grid */}
        <div className="grid gap-6 md:grid-cols-2">
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <Skeleton className="h-5 w-32" />
            <SkeletonText className="mt-4" lines={4} widths={['w-full', 'w-3/4', 'w-full', 'w-1/2']} />
          </div>
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <Skeleton className="h-5 w-28" />
            <SkeletonText className="mt-4" lines={4} widths={['w-full', 'w-2/3', 'w-full', 'w-3/4']} />
          </div>
        </div>
      </div>
    </div>
  );
}
