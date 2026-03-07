// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonTable } from '@/components/ui/skeleton';

export default function AuditLogsLoading() {
  return (
    <main className="flex-1 w-full max-w-[1440px] mx-auto px-10 py-8">
      <div className="space-y-6">
        <div>
          <Skeleton className="h-8 w-36" />
          <Skeleton className="mt-2 h-4 w-56" />
        </div>

        {/* Filters */}
        <div className="flex flex-wrap items-center gap-3">
          <Skeleton className="h-10 w-48 rounded-lg" />
          <Skeleton className="h-10 w-36 rounded-lg" />
          <Skeleton className="h-10 w-36 rounded-lg" />
          <Skeleton className="ml-auto h-10 w-24 rounded-lg" />
        </div>

        {/* Table */}
        <SkeletonTable rows={10} columns={5} />
      </div>
    </main>
  );
}
