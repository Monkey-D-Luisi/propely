// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { notFound } from 'next/navigation';
import { z } from 'zod';
import { PropertyDetailPage } from '@/components/properties/detail/PropertyDetailPage';

const ParamsSchema = z.object({
  id: z.string().uuid(),
});

type DetailPageParams = z.infer<typeof ParamsSchema>;

interface DetailPageProps {
  params: DetailPageParams | Promise<DetailPageParams>;
}

export default async function PropertyDetailRoute({ params }: DetailPageProps) {
  const resolvedParams = await params;
  const parsed = ParamsSchema.safeParse(resolvedParams);
  if (!parsed.success) {
    notFound();
  }

  return <PropertyDetailPage id={parsed.data.id} />;
}
