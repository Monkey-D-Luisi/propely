// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { AlertTriangleIcon } from '@/components/ui/icons';

interface GlobalErrorProps {
  error: Error & { digest?: string };
  reset: () => void;
}

export default function GlobalError({ error: _error, reset }: GlobalErrorProps) {
  return (
    <html lang="en">
      <body className="min-h-dvh bg-surface text-slate-950 antialiased">
        <main className="mx-auto flex min-h-dvh w-full max-w-md flex-col items-center justify-center px-6 py-16 text-center">
          <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-red-100">
            <AlertTriangleIcon className="h-7 w-7 text-red-600" />
          </div>
          <h1 className="mt-6 text-2xl font-bold text-slate-900">Something went wrong</h1>
          <p className="mt-2 text-sm text-slate-500">A critical error occurred. Please try refreshing the page.</p>
          <button
            type="button"
            onClick={reset}
            className="mt-8 inline-flex h-10 items-center rounded-lg bg-primary-600 px-5 text-sm font-medium text-white shadow-sm transition hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-300 focus:ring-offset-2"
          >
            Refresh page
          </button>
        </main>
      </body>
    </html>
  );
}
