// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import { useState } from "react";
import { useTranslations, useLocale } from "next-intl";
import { usePlans, useCreateCheckout } from "@/hooks/billing";
import { useCurrentUser, useMyOrgs } from "@/hooks/orgs";
import { useRouter } from "@/i18n/navigation";
import { PlanCard } from "./PlanCard";
import { useToast } from "@/components/ui/toast";
import { TrustedBySection } from "@/components/common/TrustedBySection";
import type { Plan } from "@/lib/schemas";

interface PlanMeta {
  price: string;
  tagline: string;
  ctaLabel: string;
  disabledFeatures?: string[];
}

export function PricingContent() {
  const t = useTranslations("billing");
  const locale = useLocale();
  const router = useRouter();
  const { plans: apiPlans, isLoading: plansLoading } = usePlans();
  const { user } = useCurrentUser();
  const { orgs } = useMyOrgs();
  const createCheckout = useCreateCheckout();
  const { toast } = useToast();

  const [loadingPlan, setLoadingPlan] = useState<string | null>(null);
  const [showOrgSelector, setShowOrgSelector] = useState<string | null>(null);

  const fallbackPlans: Plan[] = [
    {
      id: "starter",
      name: t("pricing.starterName"),
      features: [
        t("pricing.features.upTo3Projects"),
        t("pricing.features.basicAnalytics"),
        t("pricing.features.communitySupport"),
      ],
    },
    {
      id: "pro",
      name: t("pricing.proName"),
      features: [
        t("pricing.features.everythingInStarter"),
        t("pricing.features.unlimitedProjects"),
        t("pricing.features.advancedAnalytics"),
        t("pricing.features.customDomains"),
        t("pricing.features.prioritySupport"),
        t("pricing.features.teamCollaboration"),
      ],
    },
    {
      id: "enterprise",
      name: t("pricing.enterpriseName"),
      features: [
        t("pricing.features.everythingInPro"),
        t("pricing.features.sso"),
        t("pricing.features.auditLogs"),
        t("pricing.features.dedicatedManager"),
        t("pricing.features.uptimeSla"),
      ],
    },
  ];

  const plans = apiPlans.length > 0 ? apiPlans : fallbackPlans;

  const starterMeta: PlanMeta = {
    price: "$0",
    tagline: t("pricing.starterTagline"),
    ctaLabel: t("pricing.getStarted"),
    disabledFeatures: [t("pricing.features.customDomains")],
  };

  const planMeta: Record<string, PlanMeta> = {
    starter: starterMeta,
    free: starterMeta,
    pro: {
      price: "$29",
      tagline: t("pricing.proTagline"),
      ctaLabel: t("pricing.subscribe"),
    },
    enterprise: {
      price: "$99",
      tagline: t("pricing.enterpriseTagline"),
      ctaLabel: t("pricing.contactSales"),
    },
  };

  const faqs = [
    { q: t("pricing.faq1Question"), a: t("pricing.faq1Answer") },
    { q: t("pricing.faq2Question"), a: t("pricing.faq2Answer") },
    { q: t("pricing.faq3Question"), a: t("pricing.faq3Answer") },
    { q: t("pricing.faq4Question"), a: t("pricing.faq4Answer") },
  ];

  const handleSelectPlan = async (planId: string) => {
    if (!user) {
      router.push(`/login`);
      return;
    }

    if (planId === "free" || planId === "starter") {
      router.push(`/orgs/mine`);
      return;
    }

    if (planId === "enterprise") {
      window.location.href = "mailto:sales@propely.com";
      return;
    }

    const userOrgs = orgs ?? [];
    if (userOrgs.length === 0) {
      router.push(`/orgs/mine`);
      return;
    }

    if (userOrgs.length === 1) {
      await startCheckout(userOrgs[0].id, planId);
      return;
    }

    setShowOrgSelector(planId);
  };

  const startCheckout = async (orgId: string, planId: string) => {
    setLoadingPlan(planId);
    try {
      await createCheckout(orgId, planId, locale);
    } catch {
      toast({ title: t("errors.checkoutFailed"), variant: "destructive" });
    } finally {
      setLoadingPlan(null);
      setShowOrgSelector(null);
    }
  };

  if (plansLoading) {
    return (
      <div className="mx-auto w-full max-w-5xl px-4 py-16 sm:px-6 md:py-24 lg:px-8">
        <div className="mb-16 max-w-3xl mx-auto text-center">
          <div className="mx-auto mb-2 h-8 w-48 animate-pulse rounded bg-slate-200" />
          <div className="mx-auto h-5 w-72 animate-pulse rounded bg-slate-200" />
        </div>
        <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-80 animate-pulse rounded-xl border border-slate-200 bg-slate-100" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto w-full max-w-5xl px-4 py-16 sm:px-6 md:py-24 lg:px-8">
      {/* Header Section */}
      <div className="mb-16 max-w-3xl mx-auto text-center">
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 md:text-5xl mb-6">{t("pricing.title")}</h1>
        <p className="text-lg text-slate-600">{t("pricing.subtitle")}</p>
      </div>

      {/* Pricing Grid */}
      <div className="grid grid-cols-1 items-start gap-8 md:grid-cols-3">
        {plans.map((plan) => {
          const meta = planMeta[plan.id];
          return (
            <div key={plan.id}>
              <PlanCard
                plan={plan}
                isRecommended={plan.id === "pro"}
                isLoading={loadingPlan === plan.id}
                onSelect={() => handleSelectPlan(plan.id)}
                ctaLabel={meta?.ctaLabel ?? t("pricing.subscribe")}
                price={meta?.price}
                tagline={meta?.tagline}
                disabledFeatures={meta?.disabledFeatures}
              />

              {showOrgSelector === plan.id && (
                <div className="mt-3 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
                  <p className="mb-2 text-sm font-medium text-slate-700">
                    {t("pricing.selectOrg")}
                  </p>
                  <div className="space-y-1">
                    {(orgs ?? []).map((org) => (
                      <button
                        key={org.id}
                        onClick={() => {
                          startCheckout(org.id, plan.id);
                        }}
                        disabled={loadingPlan === plan.id}
                        className="w-full rounded px-3 py-2 text-left text-sm hover:bg-slate-50 disabled:opacity-50"
                      >
                        {org.name}
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {!user && (
        <p className="mt-10 text-center text-sm text-slate-500">
          {t("pricing.loginToSubscribe")}
        </p>
      )}

      {/* FAQ Section */}
      <div className="mt-24 border-t border-slate-200 pt-10">
        <h2 className="mb-8 text-center text-2xl font-bold text-slate-900">
          {t("pricing.faqTitle")}
        </h2>
        <div className="mx-auto grid max-w-4xl gap-x-12 gap-y-8 md:grid-cols-2">
          {faqs.map((faq) => (
            <div key={faq.q}>
              <h4 className="mb-2 font-medium text-slate-900">{faq.q}</h4>
              <p className="text-sm text-slate-600">{faq.a}</p>
            </div>
          ))}
        </div>
      </div>

      {/* Trusted By */}
      <TrustedBySection heading={t("pricing.trustedBy")} />
    </div>
  );
}
