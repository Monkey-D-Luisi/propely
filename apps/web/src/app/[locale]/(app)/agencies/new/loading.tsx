// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton } from '@/components/ui/skeleton';

export default function NewAgencyLoading() {
  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <div className="space-y-6">
        <header className="flex flex-col gap-1">
          <Skeleton className="h-7 w-40" />
          <Skeleton className="h-4 w-56" />
        </header>
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <div className="space-y-6">
            {Array.from({ length: 3 }).map((_, i) => (
              <div key={i}>
                <Skeleton className="mb-2 h-4 w-24" />
                <Skeleton className="h-10 w-full rounded-lg" />
              </div>
            ))}
          </div>
          <div className="mt-8 flex justify-end gap-3">
            <Skeleton className="h-10 w-24 rounded-lg" />
            <Skeleton className="h-10 w-24 rounded-lg" />
          </div>
        </div>
      </div>
    </div>
  );
}
