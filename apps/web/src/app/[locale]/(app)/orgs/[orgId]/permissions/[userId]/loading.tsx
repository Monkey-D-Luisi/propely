// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton } from '@/components/ui/skeleton';

export default function PermissionDetailLoading() {
  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <div className="space-y-6">
        {/* Back link */}
        <Skeleton className="h-4 w-32" />

        {/* User header */}
        <div className="flex items-center gap-4">
          <Skeleton className="h-12 w-12 rounded-full" />
          <div>
            <Skeleton className="h-6 w-40" />
            <Skeleton className="mt-1 h-4 w-56" />
          </div>
        </div>

        {/* Permission toggles */}
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-5 w-32" />
          <div className="mt-4 space-y-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="flex items-center justify-between">
                <div>
                  <Skeleton className="h-4 w-36" />
                  <Skeleton className="mt-1 h-3 w-56" />
                </div>
                <Skeleton className="h-6 w-12 rounded-full" />
              </div>
            ))}
          </div>
        </div>

        <Skeleton className="h-10 w-32 rounded-lg" />
      </div>
    </div>
  );
}
