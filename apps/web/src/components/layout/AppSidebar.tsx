// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link, usePathname } from '@/i18n/navigation';
import { useCallback, useState } from 'react';
import { useCurrentUser } from '@/hooks/orgs';
import { useActiveOrg } from '@/hooks/use-active-org';
import { apiFetch } from '@/lib/api';
import { clearAccessToken } from '@/lib/token-store';
import { ensureCsrfToken } from '@/lib/csrf';
import { useToast } from '@/components/ui/toast';
import { LanguageSwitcher } from './LanguageSwitcher';

const navItems = [
  { href: '/dashboard', icon: 'dashboard', labelKey: 'dashboard' },
  { href: '/properties', icon: 'apartment', labelKey: 'properties' },
  { href: '/contacts', icon: 'group', labelKey: 'contacts' },
  { href: '/leads', icon: 'trending_up', labelKey: 'leads' },
  { href: '/appointments', icon: 'calendar_month', labelKey: 'appointments' },
  { href: '/work-items', icon: 'task_alt', labelKey: 'workItems' },
] as const;

export function AppSidebar() {
  const t = useTranslations('common');
  const tAuth = useTranslations('auth');
  const { toast } = useToast();
  const pathname = usePathname();
  const { user } = useCurrentUser();
  const { activeOrg } = useActiveOrg();
  const [isProcessing, setProcessing] = useState(false);

  const handleLogout = useCallback(async () => {
    setProcessing(true);
    try {
      const csrfToken = await ensureCsrfToken();
      if (!csrfToken) {
        toast({ title: tAuth('signOutError.title'), description: tAuth('signOutError.refreshAndRetry'), variant: 'destructive' });
        return;
      }
      await apiFetch('/auth/logout', { method: 'POST', headers: { 'x-csrf-token': csrfToken }, body: JSON.stringify({}) });
      clearAccessToken();
      window.location.href = '/login';
    } catch {
      toast({ title: tAuth('signOutError.title'), description: tAuth('signOutError.tryAgainLater'), variant: 'destructive' });
    } finally {
      setProcessing(false);
    }
  }, [toast, tAuth]);

  return (
    <aside className="w-64 bg-white border-r border-slate-200 flex flex-col h-full flex-shrink-0">
      <div className="p-4 flex flex-col h-full justify-between">
        <div className="flex flex-col gap-6">
          {/* Branding */}
          <div className="flex items-center gap-3 px-2">
            <Link href="/dashboard" className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-primary-600/20 flex items-center justify-center text-primary-600 font-bold">
                P
              </div>
              <div className="flex flex-col">
                <span className="text-base font-semibold leading-tight">{t('appName')}</span>
                {activeOrg && (
                  <p className="text-xs text-slate-500">{activeOrg.name}</p>
                )}
              </div>
            </Link>
          </div>

          {/* Navigation */}
          <nav className="flex flex-col gap-1">
            {navItems.map((item) => {
              const isActive = pathname.startsWith(item.href);
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={
                    isActive
                      ? 'flex items-center gap-3 px-3 py-2.5 rounded-lg bg-primary-600/10 text-primary-600 transition-colors'
                      : 'flex items-center gap-3 px-3 py-2.5 rounded-lg text-slate-600 hover:bg-slate-50 transition-colors'
                  }
                >
                  <span
                    className="material-symbols-outlined text-[20px]"
                    style={isActive ? { fontVariationSettings: "'FILL' 1" } : undefined}
                  >
                    {item.icon}
                  </span>
                  <span className={`text-sm ${isActive ? 'font-semibold' : 'font-medium'}`}>{t(item.labelKey)}</span>
                </Link>
              );
            })}
          </nav>
        </div>

        {/* Bottom Actions */}
        <div className="flex flex-col gap-4">
          <Link
            href="/properties/new"
            className="w-full bg-primary-600 hover:bg-primary-600/90 text-white rounded-lg h-10 px-4 text-sm font-medium transition-colors shadow-sm flex items-center justify-center gap-2"
          >
            <span className="material-symbols-outlined text-[18px]">add</span>
            {t('newProperty')}
          </Link>
          <div className="flex flex-col gap-1">
            {user && (
              <>
                <Link
                  href="/orgs/mine"
                  className={
                    pathname.startsWith('/orgs')
                      ? 'flex items-center gap-3 px-3 py-2 rounded-lg bg-primary-600/10 text-primary-600 transition-colors'
                      : 'flex items-center gap-3 px-3 py-2 rounded-lg text-slate-600 hover:bg-slate-50 transition-colors'
                  }
                >
                  <span className="material-symbols-outlined text-[20px]">business</span>
                  <span className="text-sm font-medium">{t('organizations')}</span>
                </Link>
                <Link
                  href="/profile"
                  className={
                    pathname.startsWith('/profile')
                      ? 'flex items-center gap-3 px-3 py-2 rounded-lg bg-primary-600/10 text-primary-600 transition-colors'
                      : 'flex items-center gap-3 px-3 py-2 rounded-lg text-slate-600 hover:bg-slate-50 transition-colors'
                  }
                >
                  <span className="material-symbols-outlined text-[20px]">person</span>
                  <span className="text-sm font-medium truncate">{user.name || user.email}</span>
                </Link>
                <div className="flex items-center gap-2 px-3 py-2">
                  <LanguageSwitcher />
                </div>
                <button
                  type="button"
                  onClick={() => void handleLogout()}
                  disabled={isProcessing}
                  className="flex items-center gap-3 px-3 py-2 rounded-lg text-red-600 hover:bg-red-50 transition-colors"
                >
                  <span className="material-symbols-outlined text-[20px]">logout</span>
                  <span className="text-sm font-medium">{isProcessing ? t('signingOut') : t('signOut')}</span>
                </button>
              </>
            )}
          </div>
        </div>
      </div>
    </aside>
  );
}
