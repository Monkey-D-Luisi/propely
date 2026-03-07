// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonText } from '@/components/ui/skeleton';

export default function ContactDetailLoading() {
  return (
    <div className="flex-1 overflow-y-auto p-8">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        {/* Breadcrumb */}
        <Skeleton className="h-4 w-40" />

        {/* Title + actions */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Skeleton className="h-12 w-12 rounded-full" />
            <div>
              <Skeleton className="h-7 w-48" />
              <Skeleton className="mt-1 h-4 w-32" />
            </div>
          </div>
          <div className="flex gap-2">
            <Skeleton className="h-9 w-20 rounded-lg" />
            <Skeleton className="h-9 w-20 rounded-lg" />
          </div>
        </div>

        {/* Details grid */}
        <div className="grid gap-6 md:grid-cols-2">
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <Skeleton className="h-5 w-36" />
            <SkeletonText className="mt-4" lines={3} widths={['w-full', 'w-3/4', 'w-1/2']} />
          </div>
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <Skeleton className="h-5 w-28" />
            <SkeletonText className="mt-4" lines={3} widths={['w-full', 'w-2/3', 'w-3/4']} />
          </div>
        </div>
      </div>
    </div>
  );
}
