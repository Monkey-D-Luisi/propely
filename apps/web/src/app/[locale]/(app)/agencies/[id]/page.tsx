// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { use } from 'react';
import { AgencyDashboard } from '@/components/agencies/AgencyDashboard';

export default function AgencyPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  return <AgencyDashboard agencyId={id} />;
}
