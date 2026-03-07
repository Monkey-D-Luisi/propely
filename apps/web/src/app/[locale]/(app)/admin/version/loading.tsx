// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Skeleton } from '@/components/ui/skeleton';

export default function VersionLoading() {
  return (
    <main className="flex-1 w-full max-w-[1200px] mx-auto px-4 lg:px-8 py-8">
      <Skeleton className="h-8 w-48" />
      <Skeleton className="mt-2 h-4 w-64" />
      <Skeleton className="mt-6 h-40 w-full rounded-xl" />
    </main>
  );
}
