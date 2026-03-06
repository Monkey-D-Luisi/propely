// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { notFound } from "next/navigation";
import { z } from "zod";
import { BillingManagement } from "@/components/billing/BillingManagement";

const ParamsSchema = z.object({
  orgId: z.string().uuid(),
});

type BillingPageParams = z.infer<typeof ParamsSchema>;

interface BillingPageProps {
  params: BillingPageParams | Promise<BillingPageParams>;
}

export default async function BillingPage({ params }: BillingPageProps) {
  const resolvedParams = await params;
  const parsed = ParamsSchema.safeParse(resolvedParams);
  if (!parsed.success) {
    notFound();
  }

  const { orgId } = parsed.data;

  return (
    <BillingManagement orgId={orgId} />
  );
}
