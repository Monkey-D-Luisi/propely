// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useCallback, useEffect, useRef, useState } from 'react';
import { Button } from '@/components/ui/button';
import { useCurrentUser } from '@/hooks/orgs';
import { apiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { useToast } from '@/components/ui/toast';
import { LanguageSwitcher } from './LanguageSwitcher';
import { NotificationBell } from './NotificationBell';
import { BranchSwitcher } from '@/components/agencies/BranchSwitcher';
import { VerificationBanner } from '@/components/auth/VerificationBanner';

export function AppHeader() {
  const t = useTranslations('common');
  const tAuth = useTranslations('auth');
  const { toast } = useToast();
  const { user, isLoading } = useCurrentUser();
  const [isProcessing, setProcessing] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const mobileMenuRef = useRef<HTMLDivElement>(null);

  const handleLogout = useCallback(async () => {
    setProcessing(true);
    try {
      const csrfToken = await ensureCsrfToken();
      if (!csrfToken) {
        toast({
          title: tAuth('signOutError.title'),
          description: tAuth('signOutError.refreshAndRetry'),
          variant: 'destructive',
        });
        return;
      }

      await apiFetch('/auth/logout', {
        method: 'POST',
        headers: {
          'x-csrf-token': csrfToken,
        },
        body: JSON.stringify({}),
      });

      window.location.href = '/login';
    } catch {
      toast({
        title: tAuth('signOutError.title'),
        description: tAuth('signOutError.tryAgainLater'),
        variant: 'destructive',
      });
    } finally {
      setProcessing(false);
    }
  }, [toast, tAuth]);

  // Close mobile menu on outside click or Escape key
  useEffect(() => {
    if (!mobileMenuOpen) return;

    const handleClickOutside = (event: MouseEvent) => {
      if (mobileMenuRef.current && !mobileMenuRef.current.contains(event.target as Node)) {
        setMobileMenuOpen(false);
      }
    };

    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setMobileMenuOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    document.addEventListener('keydown', handleEscape);

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('keydown', handleEscape);
    };
  }, [mobileMenuOpen]);

  return (
    <header ref={mobileMenuRef} className="sticky top-0 z-50 border-b border-slate-200 bg-white/80 backdrop-blur-md">
      <div className="mx-auto flex h-16 w-full max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <Link href="/" className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary-600 text-white">
            <span className="material-symbols-outlined text-lg" aria-hidden="true">layers</span>
          </div>
          <span className="text-lg font-bold tracking-tight text-slate-900">{t('appName')}</span>
        </Link>
        <div className="flex items-center gap-3">
          <Link
            href="/pricing"
            className="hidden text-sm text-slate-600 transition hover:text-slate-900 sm:inline"
          >
            {t('pricing')}
          </Link>
          <LanguageSwitcher />
          {isLoading ? null : user ? (
            <>
              {user.isSystemAdmin && (
                <>
                  <Link
                    href="/admin/feature-flags"
                    className="hidden text-sm text-slate-600 transition hover:text-slate-900 sm:inline"
                  >
                    {t('featureFlags')}
                  </Link>
                  <Link
                    href="/admin/audit-logs"
                    className="hidden text-sm text-slate-600 transition hover:text-slate-900 sm:inline"
                  >
                    {t('auditLogs')}
                  </Link>
                  <Link
                    href="/admin/version"
                    className="hidden text-sm text-slate-600 transition hover:text-slate-900 sm:inline"
                  >
                    {t('version')}
                  </Link>
                </>
              )}
              <Link
                href="/work-items"
                className="hidden text-sm text-slate-600 transition hover:text-slate-900 sm:inline"
              >
                {t('workItems')}
              </Link>
              <BranchSwitcher />
              <NotificationBell />
              <Link
                href="/profile"
                className="hidden text-sm text-slate-600 transition hover:text-slate-900 sm:inline"
              >
                {user.name || user.email}
              </Link>
              <Button
                type="button"
                onClick={() => void handleLogout()}
                disabled={isProcessing}
                className="hidden items-center justify-center sm:inline-flex"
              >
                {isProcessing ? t('signingOut') : t('signOut')}
              </Button>
              {/* Mobile hamburger menu button */}
              <button
                type="button"
                onClick={() => setMobileMenuOpen((prev) => !prev)}
                aria-expanded={mobileMenuOpen}
                aria-label={mobileMenuOpen ? t('closeMenu') : t('openMenu')}
                className="inline-flex items-center justify-center rounded-lg p-2 text-slate-500 transition hover:bg-slate-100 hover:text-slate-700 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 sm:hidden"
              >
                {mobileMenuOpen ? (
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="h-6 w-6" aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M6 18 18 6M6 6l12 12" />
                  </svg>
                ) : (
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="h-6 w-6" aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
                  </svg>
                )}
              </button>
            </>
          ) : (
            <Link
              href="/login"
              className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
            >
              {t('signIn')}
            </Link>
          )}
        </div>
      </div>

      {/* Mobile navigation panel */}
      {mobileMenuOpen && user && (
        <nav className="border-t border-slate-200 bg-white sm:hidden" aria-label={t('openMenu')}>
          <div className="space-y-1 px-4 py-3">
            <Link
              href="/pricing"
              onClick={() => setMobileMenuOpen(false)}
              className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
            >
              {t('pricing')}
            </Link>
            {user.isSystemAdmin && (
              <>
                <Link
                  href="/admin/feature-flags"
                  onClick={() => setMobileMenuOpen(false)}
                  className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
                >
                  {t('featureFlags')}
                </Link>
                <Link
                  href="/admin/audit-logs"
                  onClick={() => setMobileMenuOpen(false)}
                  className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
                >
                  {t('auditLogs')}
                </Link>
                <Link
                  href="/admin/version"
                  onClick={() => setMobileMenuOpen(false)}
                  className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
                >
                  {t('version')}
                </Link>
              </>
            )}
            <Link
              href="/work-items"
              onClick={() => setMobileMenuOpen(false)}
              className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
            >
              {t('workItems')}
            </Link>
            <Link
              href="/agencies/new"
              onClick={() => setMobileMenuOpen(false)}
              className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
            >
              {t('agencies')}
            </Link>
            <Link
              href="/profile"
              onClick={() => setMobileMenuOpen(false)}
              className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
            >
              {user.name || user.email}
            </Link>
            <div className="border-t border-slate-100 pt-2">
              <Button
                type="button"
                onClick={() => {
                  setMobileMenuOpen(false);
                  void handleLogout();
                }}
                disabled={isProcessing}
                className="w-full justify-center"
              >
                {isProcessing ? t('signingOut') : t('signOut')}
              </Button>
            </div>
          </div>
        </nav>
      )}

      <VerificationBanner show={!isLoading && !!user && !user.emailVerified} />
    </header>
  );
}
