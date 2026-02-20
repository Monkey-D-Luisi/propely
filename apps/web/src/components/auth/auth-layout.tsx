// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { LayersIcon } from '@/components/ui/icons';

interface AuthLayoutProps {
  children: React.ReactNode;
}

/** Shared wrapper for all auth pages: centering + background gradient decorations. */
export function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <div className="flex min-h-dvh flex-col items-center justify-center bg-surface p-6">
      <div className="w-full max-w-md">
        {children}
      </div>
      {/* Stitch-style blurred gradient decorations */}
      <div className="pointer-events-none fixed inset-0 -z-10 overflow-hidden" aria-hidden="true">
        <div className="absolute -left-[10%] -top-[10%] h-[40%] w-[40%] rounded-full bg-primary-600/5 blur-[100px]" />
        <div className="absolute -bottom-[10%] -right-[10%] h-[40%] w-[40%] rounded-full bg-primary-600/10 blur-[100px]" />
      </div>
    </div>
  );
}

interface AuthBrandProps {
  icon?: React.ReactNode;
  children?: React.ReactNode;
}

/** Brand header with logo icon and optional heading/subtitle below. */
export function AuthBrand({ icon, children }: AuthBrandProps) {
  return (
    <div className="mb-8 flex flex-col items-center text-center">
      <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-primary-600 shadow-lg shadow-primary-600/30">
        {icon ?? <LayersIcon className="h-7 w-7 text-white" />}
      </div>
      {children}
    </div>
  );
}

interface AuthCardProps {
  children: React.ReactNode;
  className?: string;
}

/** White card shell with shadow and standard padding (no border in light mode per Stitch). */
export function AuthCard({ children, className }: AuthCardProps) {
  return (
    <div className={`rounded-2xl bg-white p-8 shadow-xl ${className ?? ''}`}>
      {children}
    </div>
  );
}

/** Terms / Privacy / Support links below auth cards. */
export function AuthFooterLinks() {
  const t = useTranslations('auth.footer');

  return (
    <div className="mt-8 flex justify-center gap-6 text-xs text-slate-400">
      <Link href="/privacy" className="transition-colors hover:text-slate-600">{t('privacy')}</Link>
      <Link href="/terms" className="transition-colors hover:text-slate-600">{t('terms')}</Link>
      <Link href="/support" className="transition-colors hover:text-slate-600">{t('support')}</Link>
    </div>
  );
}
