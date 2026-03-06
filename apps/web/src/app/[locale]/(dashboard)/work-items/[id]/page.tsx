// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { notFound } from 'next/navigation';
import { z } from 'zod';
import { WorkItemDetail } from '@/components/work-items/WorkItemDetail';

const ParamsSchema = z.object({
  id: z.string().uuid(),
});

type DetailPageParams = z.infer<typeof ParamsSchema>;

interface DetailPageProps {
  params: DetailPageParams | Promise<DetailPageParams>;
}

export default async function WorkItemDetailPage({ params }: DetailPageProps) {
  const resolvedParams = await params;
  const parsed = ParamsSchema.safeParse(resolvedParams);
  if (!parsed.success) {
    notFound();
  }

  return (
    <div className="flex-1 overflow-y-auto p-8">
      <div className="mx-auto flex max-w-[1200px] flex-col gap-6">
      <WorkItemDetail id={parsed.data.id} />
      </div>
    </div>
  );
}
