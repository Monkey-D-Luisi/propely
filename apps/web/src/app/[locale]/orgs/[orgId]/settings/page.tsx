// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { notFound } from 'next/navigation';
import { z } from 'zod';
import { OrgSettingsForm } from '@/components/orgs/OrgSettingsForm';

const ParamsSchema = z.object({
  orgId: z.string().uuid()
});

type SettingsPageParams = z.infer<typeof ParamsSchema>;

interface SettingsPageProps {
  params: SettingsPageParams | Promise<SettingsPageParams>;
}

export default async function SettingsPage({ params }: SettingsPageProps) {
  const resolvedParams = await params;
  const parsed = ParamsSchema.safeParse(resolvedParams);
  if (!parsed.success) {
    notFound();
  }

  const { orgId } = parsed.data;

  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-8 p-6 md:p-10">
      <OrgSettingsForm orgId={orgId} />
    </div>
  );
}
