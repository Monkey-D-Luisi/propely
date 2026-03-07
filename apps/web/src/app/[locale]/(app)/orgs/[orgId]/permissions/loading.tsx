// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton, SkeletonTable } from '@/components/ui/skeleton';

export default function PermissionsLoading() {
  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <div className="space-y-6">
        <header className="flex flex-col gap-1">
          <Skeleton className="h-7 w-36" />
          <Skeleton className="h-4 w-56" />
        </header>

        <SkeletonTable rows={5} columns={3} />
      </div>
    </div>
  );
}
