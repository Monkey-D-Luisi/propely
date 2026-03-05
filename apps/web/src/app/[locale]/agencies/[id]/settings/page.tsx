// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { use } from 'react';
import { AgencySettingsForm } from '@/components/agencies/AgencySettingsForm';

export default function AgencySettingsPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  return <AgencySettingsForm agencyId={id} />;
}
