// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import { useTranslations } from "next-intl";
import type { Plan } from "@/lib/schemas";
import { Button } from "@/components/ui/button";

interface PlanCardProps {
  plan: Plan;
  isCurrent?: boolean;
  isRecommended?: boolean;
  isLoading?: boolean;
  onSelect?: () => void;
  ctaLabel?: string;
  price?: string;
  tagline?: string;
  disabledFeatures?: string[];
}

export function PlanCard({
  plan,
  isCurrent = false,
  isRecommended = false,
  isLoading = false,
  onSelect,
  ctaLabel,
  price,
  tagline,
  disabledFeatures,
}: PlanCardProps) {
  const t = useTranslations("billing.pricing");

  return (
    <div
      className={`relative flex flex-col rounded-xl bg-white p-8 ${
        isRecommended
          ? "z-10 border-2 border-primary-600 shadow-lg md:-translate-y-4"
          : isCurrent
            ? "border-2 border-primary-600 shadow-lg"
            : "border border-slate-200 shadow-sm transition-shadow duration-300 hover:shadow-md"
      }`}
    >
      {isRecommended && (
        <div className="absolute left-1/2 top-0 -translate-x-1/2 -translate-y-1/2">
          <span className="rounded-full bg-primary-600 px-3 py-1 text-xs font-bold uppercase tracking-wide text-white shadow-sm">
            {t("recommended")}
          </span>
        </div>
      )}

      <div className="mb-6">
        <h3 className={`text-lg font-semibold ${isRecommended ? "text-primary-600" : "text-slate-900"}`}>{plan.name}</h3>
        {tagline && (
          <p className="mt-2 text-sm text-slate-500">{tagline}</p>
        )}
      </div>

      {price !== undefined && (
        <div className="mb-6 flex items-baseline">
          <span className="text-4xl font-bold text-slate-900">{price}</span>
          <span className="ml-2 text-slate-500">{t("perMonth")}</span>
        </div>
      )}

      <ul className="mb-8 flex-1 space-y-4">
        {plan.features.map((feature) => (
          <li key={feature} className="flex items-start">
            <span className="material-symbols-outlined mr-2 text-xl text-emerald-500">check_circle</span>
            <span className="text-sm text-slate-600">{feature}</span>
          </li>
        ))}
        {disabledFeatures?.map((feature) => (
          <li key={feature} className="flex items-start opacity-50">
            <span className="material-symbols-outlined mr-2 text-xl text-slate-300">cancel</span>
            <span className="text-sm text-slate-400">{feature}</span>
          </li>
        ))}
      </ul>

      {isCurrent ? (
        <div className="rounded-lg border border-slate-200 bg-slate-50 py-2.5 text-center text-sm font-medium text-slate-600">
          {t("currentPlan")}
        </div>
      ) : isRecommended ? (
        <Button
          onClick={onSelect}
          disabled={isLoading}
          className="w-full py-2.5 font-semibold shadow-md shadow-primary-600/30"
        >
          {isLoading ? "..." : ctaLabel}
        </Button>
      ) : (
        <button
          onClick={onSelect}
          disabled={isLoading}
          className="w-full rounded-lg border border-slate-300 py-2.5 px-4 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isLoading ? "..." : ctaLabel}
        </button>
      )}
    </div>
  );
}
