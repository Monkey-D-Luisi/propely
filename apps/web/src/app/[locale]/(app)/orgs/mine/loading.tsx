// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton } from '@/components/ui/skeleton';

export default function MyOrgsLoading() {
  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-6xl flex-col gap-6 p-6 md:p-10">
      <div className="flex items-center justify-between">
        <div>
          <Skeleton className="h-7 w-48" />
          <Skeleton className="mt-2 h-4 w-64" />
        </div>
        <div className="flex gap-2">
          <Skeleton className="h-10 w-36 rounded-md" />
          <Skeleton className="h-10 w-24 rounded-md" />
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <div
            key={i}
            className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm"
          >
            <Skeleton className="h-5 w-32" />
            <Skeleton className="mt-3 h-4 w-20" />
            <Skeleton className="mt-2 h-5 w-16 rounded-full" />
          </div>
        ))}
      </div>
    </div>
  );
}
