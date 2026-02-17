// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import { useState } from "react";
import { useTranslations, useLocale } from "next-intl";
import { useSubscription, useCreateCustomerPortal } from "@/hooks/billing";
import { useOrg } from "@/hooks/orgs";
import { Link } from "@/i18n/navigation";
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/ui/toast";
import { PaymentHistory } from "@/components/billing/PaymentHistory";
import { getStatusColor, subscriptionStatusColors } from "@/lib/status-colors";

interface BillingManagementProps {
  orgId: string;
}

export function BillingManagement({ orgId }: BillingManagementProps) {
  const t = useTranslations("billing");
  const locale = useLocale();
  const { subscription, isLoading: subLoading } = useSubscription(orgId);
  const { org } = useOrg(orgId);
  const createPortal = useCreateCustomerPortal();
  const { toast } = useToast();
  const [portalLoading, setPortalLoading] = useState(false);

  const canManage =
    org?.role === "owner" || org?.role === "admin";

  const handleManageBilling = async () => {
    setPortalLoading(true);
    try {
      await createPortal(orgId, locale);
    } catch {
      toast({ title: t("errors.portalFailed"), variant: "destructive" });
    } finally {
      setPortalLoading(false);
    }
  };

  const getStatusLabel = (status: string) => {
    const statusMap: Record<string, string> = {
      active: t("management.active"),
      trialing: t("management.trialing"),
      free: t("management.free"),
      pastdue: t("management.pastDue"),
      cancelled: t("management.cancelled"),
    };
    return statusMap[status] ?? status;
  };

  if (subLoading) {
    return (
      <div className="space-y-6">
        <div>
          <div className="h-8 w-32 animate-pulse rounded bg-slate-200" />
          <div className="mt-1 h-5 w-64 animate-pulse rounded bg-slate-200" />
        </div>
        <div className="h-48 animate-pulse rounded-xl border border-slate-200 bg-slate-100" />
      </div>
    );
  }

  if (!subscription) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">
            {t("management.title")}
          </h1>
          <p className="mt-1 text-sm text-slate-600">{t("management.subtitle")}</p>
        </div>
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <p className="text-slate-600">{t("management.noSubscription")}</p>
          <Link
            href="/pricing"
            className="mt-4 inline-block text-sm font-medium text-slate-900 underline"
          >
            {t("management.changePlan")}
          </Link>
        </div>
        <PaymentHistory orgId={orgId} />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">
          {t("management.title")}
        </h1>
        <p className="mt-1 text-sm text-slate-600">{t("management.subtitle")}</p>
      </div>

      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-6 md:flex-row md:items-start md:justify-between">
          <div className="space-y-4">
            <div>
              <p className="text-sm text-slate-500">{t("management.currentPlan")}</p>
              <p className="text-xl font-semibold text-slate-900">
                {subscription.planName}
              </p>
            </div>

            <div>
              <p className="text-sm text-slate-500">{t("management.status")}</p>
              <span
                className={`inline-block rounded-full px-3 py-1 text-xs font-medium ${getStatusColor(subscription.status, subscriptionStatusColors)}`}
              >
                {getStatusLabel(subscription.status)}
              </span>
            </div>

            {subscription.currentPeriodEnd && (
              <div>
                <p className="text-sm text-slate-500">{t("management.renewsOn")}</p>
                <p className="text-sm text-slate-900">
                  {new Date(subscription.currentPeriodEnd).toLocaleDateString(
                    locale,
                    { year: "numeric", month: "long", day: "numeric" },
                  )}
                </p>
              </div>
            )}

            <div>
              <p className="mb-2 text-sm text-slate-500">{t("management.features")}</p>
              <ul className="space-y-1">
                {subscription.features.map((feature) => (
                  <li
                    key={feature}
                    className="flex items-center gap-2 text-sm text-slate-600"
                  >
                    <svg
                      className="h-4 w-4 flex-shrink-0 text-emerald-500"
                      fill="none"
                      viewBox="0 0 24 24"
                      strokeWidth={2}
                      stroke="currentColor"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M4.5 12.75l6 6 9-13.5"
                      />
                    </svg>
                    {feature}
                  </li>
                ))}
              </ul>
            </div>
          </div>

          <div className="flex flex-col gap-3">
            {canManage && subscription.status !== "free" && (
              <Button onClick={handleManageBilling} disabled={portalLoading}>
                {portalLoading ? "..." : t("management.manageBilling")}
              </Button>
            )}
            <Link
              href="/pricing"
              className="text-center text-sm font-medium text-slate-600 hover:text-slate-900"
            >
              {t("management.changePlan")}
            </Link>
            {!canManage && (
              <p className="text-xs text-slate-500">
                {t("management.ownersOnly")}
              </p>
            )}
          </div>
        </div>
      </div>

      <div>
        <h2 className="mb-4 text-lg font-semibold text-slate-900">
          {t("payments.title")}
        </h2>
        <PaymentHistory orgId={orgId} />
      </div>
    </div>
  );
}
