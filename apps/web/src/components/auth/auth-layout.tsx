// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';

interface AuthLayoutProps {
  children: React.ReactNode;
}

/**
 * Shared wrapper for all auth pages.
 * NOTE: The (auth) route group layout handles centering + gradient mesh.
 * This component is now a pass-through for backward compatibility.
 */
export function AuthLayout({ children }: AuthLayoutProps) {
  return <>{children}</>;
}

interface AuthBrandProps {
  children?: React.ReactNode;
}

/** Brand header with diamond icon and "Propely" text, matching Stitch login.html. */
export function AuthBrand({ children }: AuthBrandProps) {
  return (
    <div className="flex flex-col items-center mb-8">
      <div className="flex items-center gap-2 mb-3">
        <span
          className="material-symbols-outlined text-primary-600 text-4xl"
          style={{ fontVariationSettings: "'FILL' 1" }}
        >
          diamond
        </span>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">Propely</h1>
      </div>
      {children}
    </div>
  );
}

interface AuthCardProps {
  children: React.ReactNode;
  className?: string;
}

/** White card shell matching Stitch login.html: rounded-xl, border, shadow-sm. */
export function AuthCard({ children, className }: AuthCardProps) {
  return (
    <div className={`w-full bg-white rounded-xl border border-slate-200 shadow-sm p-8 sm:p-10 flex flex-col gap-6 ${className ?? ''}`}>
      {children}
    </div>
  );
}

/** Terms / Privacy / Support links below auth cards, matching Stitch login.html. */
export function AuthFooterLinks() {
  const t = useTranslations('auth.footer');

  return (
    <div className="mt-12 flex items-center justify-center gap-6 text-xs text-slate-500">
      <Link href="/privacy" className="hover:text-slate-800 transition-colors">{t('privacy')}</Link>
      <Link href="/terms" className="hover:text-slate-800 transition-colors">{t('terms')}</Link>
      <Link href="/support" className="hover:text-slate-800 transition-colors">{t('support')}</Link>
    </div>
  );
}
