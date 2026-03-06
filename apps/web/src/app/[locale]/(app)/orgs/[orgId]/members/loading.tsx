// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonTable } from '@/components/ui/skeleton';

export default function MembersLoading() {
  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <div className="space-y-6">
        <header className="flex flex-col gap-1">
          <Skeleton className="h-7 w-32" />
          <Skeleton className="h-4 w-48" />
        </header>

        <SkeletonTable rows={4} columns={4} />

        <div className="grid gap-6 md:grid-cols-[2fr,1fr]">
          <div className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
            <Skeleton className="h-5 w-32" />
            <div className="mt-3 flex flex-col gap-3">
              <div>
                <Skeleton className="mb-1.5 h-4 w-16" />
                <Skeleton className="h-10 w-full" />
              </div>
              <div>
                <Skeleton className="mb-1.5 h-4 w-20" />
                <Skeleton className="h-10 w-full" />
              </div>
              <Skeleton className="h-10 w-full rounded-md" />
            </div>
          </div>
          <div className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
            <Skeleton className="h-5 w-28" />
            <Skeleton className="mt-2 h-4 w-full" />
            <Skeleton className="mt-1 h-4 w-3/4" />
            <Skeleton className="mt-4 h-10 w-full rounded-md" />
          </div>
        </div>
      </div>
    </div>
  );
}
