// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import type { ReactNode } from 'react';

export default function AuthGroupLayout({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-dvh flex flex-col items-center justify-center relative overflow-hidden bg-surface">
      {/* Gradient mesh */}
      <div className="absolute top-0 left-0 w-full h-[600px] bg-gradient-to-b from-primary-600/10 to-transparent pointer-events-none z-0" />
      <div className="relative z-10 w-full max-w-[448px] px-4 sm:px-0 flex flex-col items-center">
        {children}
      </div>
    </div>
  );
}
