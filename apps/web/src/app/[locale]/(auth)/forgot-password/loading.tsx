// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonText } from '@/components/ui/skeleton';

export default function ForgotPasswordLoading() {
  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-md flex-col justify-center px-6 py-16">
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <Skeleton className="h-7 w-40" />
        <SkeletonText className="mt-2" widths={['w-3/4']} />

        <div className="mt-6 space-y-4">
          <div>
            <Skeleton className="mb-1.5 h-4 w-16" />
            <Skeleton className="h-10 w-full" />
          </div>
          <Skeleton className="h-10 w-full rounded-md" />
        </div>

        <Skeleton className="mx-auto mt-4 h-4 w-36" />
      </div>
    </div>
  );
}
