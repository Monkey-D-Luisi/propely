// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { notFound } from 'next/navigation';
import { z } from 'zod';
import { MembersManager } from '@/components/orgs/MembersManager';

const ParamsSchema = z.object({
  orgId: z.string().uuid()
});

type MembersPageParams = z.infer<typeof ParamsSchema>;

interface MembersPageProps {
  params: MembersPageParams | Promise<MembersPageParams>;
}

export default async function MembersPage({ params }: MembersPageProps) {
  const resolvedParams = await params;
  const parsed = ParamsSchema.safeParse(resolvedParams);
  if (!parsed.success) {
    notFound();
  }

  const { orgId } = parsed.data;

  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <MembersManager orgId={orgId} />
    </div>
  );
}