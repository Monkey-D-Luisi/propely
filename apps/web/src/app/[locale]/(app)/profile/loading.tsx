// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton } from '@/components/ui/skeleton';

export default function ProfileLoading() {
  return (
    <main className="flex-1 w-full max-w-[960px] mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
      <div className="space-y-6">
        <div className="mb-8">
          <Skeleton className="h-8 w-32" />
          <Skeleton className="mt-2 h-4 w-56" />
        </div>

        {/* Profile form */}
        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-6 w-36" />
          <div className="mt-4 space-y-4">
            <div>
              <Skeleton className="mb-1.5 h-4 w-20" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div>
              <Skeleton className="mb-1.5 h-4 w-16" />
              <Skeleton className="h-10 w-full" />
            </div>
            <Skeleton className="h-10 w-32 rounded-lg" />
          </div>
        </div>

        {/* Change password form */}
        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-6 w-44" />
          <div className="mt-4 space-y-4">
            <div>
              <Skeleton className="mb-1.5 h-4 w-36" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div>
              <Skeleton className="mb-1.5 h-4 w-28" />
              <Skeleton className="h-10 w-full" />
            </div>
            <Skeleton className="h-10 w-40 rounded-lg" />
          </div>
        </div>

        {/* Delete account */}
        <div className="rounded-xl border border-red-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-6 w-36" />
          <Skeleton className="mt-2 h-4 w-full" />
          <Skeleton className="mt-4 h-10 w-36 rounded-lg" />
        </div>
      </div>
    </main>
  );
}
