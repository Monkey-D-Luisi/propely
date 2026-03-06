// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import Link from 'next/link';

export default function RootNotFound() {
  return (
    <main className="min-h-dvh flex flex-col items-center justify-center bg-surface px-4 py-12 relative overflow-x-hidden">
      {/* Decorative background glow */}
      <div className="fixed inset-0 pointer-events-none z-0 flex items-center justify-center">
        <div className="w-[300px] h-[300px] md:w-[500px] md:h-[500px] bg-primary-600/10 rounded-full blur-[100px] md:blur-[150px]" />
      </div>

      <div className="relative z-10 w-full max-w-[960px] mx-auto text-center flex flex-col items-center">
        {/* Large 404 */}
        <p className="text-[140px] md:text-[240px] font-black text-slate-200 leading-none tracking-tighter select-none mb-0" aria-hidden="true">
          404
        </p>

        <h1 className="text-[28px] md:text-[36px] font-bold tracking-tight text-slate-900 mb-4 px-4">
          Page not found
        </h1>
        <p className="text-slate-500 text-base md:text-lg max-w-[480px] mx-auto mb-10 px-4 leading-relaxed">
          The page you are looking for does not exist or has been moved.
        </p>

        <div className="flex flex-col sm:flex-row gap-4 w-full max-w-[480px] px-4 justify-center">
          <Link
            href="/"
            className="flex-1 min-w-[140px] flex items-center justify-center gap-2 h-12 px-6 rounded-xl bg-primary-600 text-white font-semibold text-sm hover:bg-primary-600/90 transition"
          >
            Go to home page
          </Link>
        </div>
      </div>
    </main>
  );
}
