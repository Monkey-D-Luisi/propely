// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useCallback, useEffect, useRef, useState } from 'react';
import { useCurrentUser } from '@/hooks/orgs';
import { apiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { useToast } from '@/components/ui/toast';
import { NotificationBell } from './NotificationBell';
import { VerificationBanner } from '@/components/auth/VerificationBanner';

const DiamondLogo = () => (
  <svg fill="none" viewBox="0 0 48 48" xmlns="http://www.w3.org/2000/svg" className="size-6">
    <g clipPath="url(#clip0_logo)">
      <path clipRule="evenodd" d="M47.2426 24L24 47.2426L0.757355 24L24 0.757355L47.2426 24ZM12.2426 21H35.7574L24 9.24264L12.2426 21Z" fill="currentColor" fillRule="evenodd" />
    </g>
    <defs>
      <clipPath id="clip0_logo"><rect fill="white" height="48" width="48" /></clipPath>
    </defs>
  </svg>
);

export function AppTopHeader() {
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
        toast({ title: tAuth('signOutError.title'), description: tAuth('signOutError.refreshAndRetry'), variant: 'destructive' });
        return;
      }
      await apiFetch('/auth/logout', { method: 'POST', headers: { 'x-csrf-token': csrfToken }, body: JSON.stringify({}) });
      window.location.href = '/login';
    } catch {
      toast({ title: tAuth('signOutError.title'), description: tAuth('signOutError.tryAgainLater'), variant: 'destructive' });
    } finally {
      setProcessing(false);
    }
  }, [toast, tAuth]);

  useEffect(() => {
    if (!mobileMenuOpen) return;
    const handleClickOutside = (event: MouseEvent) => {
      if (mobileMenuRef.current && !mobileMenuRef.current.contains(event.target as Node)) setMobileMenuOpen(false);
    };
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setMobileMenuOpen(false);
    };
    document.addEventListener('mousedown', handleClickOutside);
    document.addEventListener('keydown', handleEscape);
    return () => { document.removeEventListener('mousedown', handleClickOutside); document.removeEventListener('keydown', handleEscape); };
  }, [mobileMenuOpen]);

  return (
    <header ref={mobileMenuRef} className="flex items-center justify-between whitespace-nowrap border-b border-slate-200 bg-white px-10 py-3 sticky top-0 z-10">
      <div className="flex items-center gap-8">
        <Link href="/" className="flex items-center gap-4 text-primary-600">
          <DiamondLogo />
          <h2 className="text-xl font-bold leading-tight tracking-tight text-slate-900">{t('appName')}</h2>
        </Link>
        {/* Search bar - hidden on mobile */}
        <label className="flex flex-col min-w-40 !h-10 max-w-64 hidden md:flex">
          <div className="flex w-full flex-1 items-stretch rounded-xl h-full">
            <div className="text-slate-500 flex border-none bg-slate-100 items-center justify-center pl-4 rounded-l-xl border-r-0">
              <span className="material-symbols-outlined">search</span>
            </div>
            <input
              className="flex w-full min-w-0 flex-1 resize-none overflow-hidden rounded-xl focus:outline-0 focus:ring-0 border-none bg-slate-100 h-full placeholder:text-slate-500 px-4 rounded-l-none border-l-0 pl-2 text-sm font-normal leading-normal text-slate-900"
              placeholder={t('search')}
            />
          </div>
        </label>
      </div>

      <div className="flex flex-1 justify-end gap-8">
        {/* Desktop nav links */}
        <nav className="flex items-center gap-9 hidden lg:flex">
          <Link href="/orgs/mine" className="text-slate-600 hover:text-primary-600 text-sm font-medium leading-normal transition-colors">{t('dashboard')}</Link>
          <Link href="/properties" className="text-slate-600 hover:text-primary-600 text-sm font-medium leading-normal transition-colors">{t('properties')}</Link>
          <Link href="/contacts" className="text-slate-600 hover:text-primary-600 text-sm font-medium leading-normal transition-colors">{t('contacts')}</Link>
        </nav>

        <div className="flex items-center gap-4">
          {isLoading ? null : user ? (
            <>
              <Link
                href="/properties/new"
                className="flex cursor-pointer items-center justify-center overflow-hidden rounded-xl h-10 px-4 bg-primary-600 hover:bg-primary-600/90 text-white text-sm font-bold leading-normal tracking-wide transition-colors hidden sm:flex"
              >
                <span className="truncate">{t('addProperty')}</span>
              </Link>
              <NotificationBell />
              <Link
                href="/profile"
                className="bg-primary-600 text-white rounded-full size-10 flex items-center justify-center text-sm font-bold"
              >
                {(user.name || user.email || '?').charAt(0).toUpperCase()}
              </Link>
              {/* Mobile hamburger */}
              <button
                type="button"
                onClick={() => setMobileMenuOpen((prev) => !prev)}
                aria-expanded={mobileMenuOpen}
                aria-label={mobileMenuOpen ? t('closeMenu') : t('openMenu')}
                className="inline-flex items-center justify-center rounded-lg p-2 text-slate-500 transition hover:bg-slate-100 hover:text-slate-700 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 sm:hidden"
              >
                {mobileMenuOpen ? (
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="h-6 w-6" aria-hidden="true"><path strokeLinecap="round" strokeLinejoin="round" d="M6 18 18 6M6 6l12 12" /></svg>
                ) : (
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="h-6 w-6" aria-hidden="true"><path strokeLinecap="round" strokeLinejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" /></svg>
                )}
              </button>
            </>
          ) : (
            <Link href="/login" className="flex min-w-[84px] cursor-pointer items-center justify-center overflow-hidden rounded-lg h-10 px-4 bg-primary-600 hover:bg-primary-600/90 text-white text-sm font-bold leading-normal tracking-wide transition-colors">
              <span className="truncate">{t('signIn')}</span>
            </Link>
          )}
        </div>
      </div>

      {/* Mobile navigation panel */}
      {mobileMenuOpen && user && (
        <nav className="absolute top-full left-0 right-0 border-t border-slate-200 bg-white sm:hidden z-50" aria-label={t('openMenu')}>
          <div className="space-y-1 px-4 py-3">
            <Link href="/orgs/mine" onClick={() => setMobileMenuOpen(false)} className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900">{t('dashboard')}</Link>
            <Link href="/properties" onClick={() => setMobileMenuOpen(false)} className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900">{t('properties')}</Link>
            <Link href="/contacts" onClick={() => setMobileMenuOpen(false)} className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900">{t('contacts')}</Link>
            <Link href="/leads" onClick={() => setMobileMenuOpen(false)} className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900">{t('leads')}</Link>
            <Link href="/appointments" onClick={() => setMobileMenuOpen(false)} className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900">{t('appointments')}</Link>
            <Link href="/work-items" onClick={() => setMobileMenuOpen(false)} className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900">{t('workItems')}</Link>
            <Link href="/profile" onClick={() => setMobileMenuOpen(false)} className="block rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900">{user.name || user.email}</Link>
            <div className="border-t border-slate-100 pt-2">
              <button
                type="button"
                onClick={() => { setMobileMenuOpen(false); void handleLogout(); }}
                disabled={isProcessing}
                className="w-full rounded-lg bg-primary-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-primary-600/90"
              >
                {isProcessing ? t('signingOut') : t('signOut')}
              </button>
            </div>
          </div>
        </nav>
      )}

      <VerificationBanner show={!isLoading && !!user && !user.emailVerified} />
    </header>
  );
}
