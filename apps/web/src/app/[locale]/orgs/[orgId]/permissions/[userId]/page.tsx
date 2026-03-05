// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { notFound } from 'next/navigation';
import { z } from 'zod';
import { PermissionDetailPageClient } from './PermissionDetailPageClient';

const ParamsSchema = z.object({
  orgId: z.string().uuid(),
  userId: z.string().uuid(),
});

type PermissionDetailParams = z.infer<typeof ParamsSchema>;

interface PermissionDetailPageProps {
  params: PermissionDetailParams | Promise<PermissionDetailParams>;
}

export default async function PermissionDetailPage({ params }: PermissionDetailPageProps) {
  const resolvedParams = await params;
  const parsed = ParamsSchema.safeParse(resolvedParams);
  if (!parsed.success) {
    notFound();
  }

  const { orgId, userId } = parsed.data;

  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <PermissionDetailPageClient orgId={orgId} userId={userId} />
    </div>
  );
}
