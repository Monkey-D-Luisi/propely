// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
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
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <WorkItemEdit id={parsed.data.id} />
    </div>
  );
}
