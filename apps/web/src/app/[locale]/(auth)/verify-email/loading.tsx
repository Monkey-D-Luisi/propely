// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonText } from '@/components/ui/skeleton';

export default function VerifyEmailLoading() {
  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-lg flex-col justify-center px-6 py-16">
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <Skeleton className="h-7 w-48" />
        <SkeletonText className="mt-2" widths={['w-full']} />
      </div>
    </div>
  );
}
