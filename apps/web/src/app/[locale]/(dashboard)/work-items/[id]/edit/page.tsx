// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { notFound } from 'next/navigation';
import { z } from 'zod';
import { WorkItemEdit } from '@/components/work-items/WorkItemEdit';

const ParamsSchema = z.object({
  id: z.string().uuid(),
});

type EditPageParams = z.infer<typeof ParamsSchema>;

interface EditPageProps {
  params: EditPageParams | Promise<EditPageParams>;
}

export default async function WorkItemEditPage({ params }: EditPageProps) {
  const resolvedParams = await params;
  const parsed = ParamsSchema.safeParse(resolvedParams);
  if (!parsed.success) {
    notFound();
  }

  return (
    <div className="flex-1 overflow-y-auto p-8">
      <div className="mx-auto flex max-w-[1200px] flex-col gap-6">
      <WorkItemEdit id={parsed.data.id} />
      </div>
    </div>
  );
}
