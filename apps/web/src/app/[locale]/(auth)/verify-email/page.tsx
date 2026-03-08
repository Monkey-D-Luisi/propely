// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { Suspense } from 'react';
import { VerifyEmailContent } from '@/components/auth/VerifyEmailContent';

export default function VerifyEmailPage() {
  return (
    <Suspense
      fallback={
        <div className="mx-auto flex min-h-dvh w-full max-w-md flex-col justify-center px-6 py-16">
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <div className="h-8 w-48 animate-pulse rounded bg-slate-200" />
            <div className="mt-2 h-4 w-full animate-pulse rounded bg-slate-200" />
          </div>
        </div>
      }
    >
      <VerifyEmailContent />
    </Suspense>
  );
}
