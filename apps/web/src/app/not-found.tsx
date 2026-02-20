// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import Link from 'next/link';

export default function RootNotFound() {
  return (
    <main className="mx-auto flex min-h-dvh w-full max-w-md flex-col items-center justify-center bg-surface px-6 py-16 text-center">
      <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-slate-100">
        <span className="text-2xl font-bold text-slate-400" aria-hidden="true">404</span>
      </div>
      <h1 className="mt-6 text-2xl font-bold text-slate-900">Page not found</h1>
      <p className="mt-2 text-sm text-slate-500">
        The page you are looking for does not exist or has been moved.
      </p>
      <Link
        href="/"
        className="mt-8 inline-flex h-10 items-center gap-2 rounded-lg bg-primary-600 px-5 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90"
      >
        Go to home page
      </Link>
    </main>
  );
}
