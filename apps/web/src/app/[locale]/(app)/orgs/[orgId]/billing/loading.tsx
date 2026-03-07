// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton } from '@/components/ui/skeleton';

export default function BillingLoading() {
  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <div className="space-y-6">
        <header className="flex flex-col gap-1">
          <Skeleton className="h-7 w-28" />
          <Skeleton className="h-4 w-56" />
        </header>

        {/* Current plan card */}
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-5 w-32" />
          <div className="mt-4 flex items-baseline gap-2">
            <Skeleton className="h-8 w-20" />
            <Skeleton className="h-4 w-16" />
          </div>
          <Skeleton className="mt-4 h-10 w-36 rounded-lg" />
        </div>

        {/* Payment method */}
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-5 w-40" />
          <Skeleton className="mt-4 h-12 w-full rounded-lg" />
        </div>
      </div>
    </div>
  );
}
