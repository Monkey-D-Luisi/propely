// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { notFound } from 'next/navigation';
import { z } from 'zod';
import { PermissionMembersList } from '@/components/permissions/PermissionMembersList';

const ParamsSchema = z.object({
  orgId: z.string().uuid(),
});

type PermissionsPageParams = z.infer<typeof ParamsSchema>;

interface PermissionsPageProps {
  params: PermissionsPageParams | Promise<PermissionsPageParams>;
}

export default async function PermissionsPage({ params }: PermissionsPageProps) {
  const resolvedParams = await params;
  const parsed = ParamsSchema.safeParse(resolvedParams);
  if (!parsed.success) {
    notFound();
  }

  const { orgId } = parsed.data;

  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <PermissionMembersList orgId={orgId} />
    </div>
  );
}
