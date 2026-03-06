// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { notFound } from 'next/navigation';
import { z } from 'zod';
import { ContactDetail } from '@/components/contacts/ContactDetail';

const ParamsSchema = z.object({
  id: z.string().uuid(),
});

type DetailPageParams = z.infer<typeof ParamsSchema>;

interface DetailPageProps {
  params: DetailPageParams | Promise<DetailPageParams>;
}

export default async function ContactDetailRoute({ params }: DetailPageProps) {
  const resolvedParams = await params;
  const parsed = ParamsSchema.safeParse(resolvedParams);
  if (!parsed.success) {
    notFound();
  }

  return <ContactDetail id={parsed.data.id} />;
}
