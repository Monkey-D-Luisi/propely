// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import { useTranslations, useLocale } from "next-intl";
import { usePayments } from "@/hooks/billing";
import { formatDateShort } from "@/lib/format";
import { getStatusColor, paymentStatusColors } from "@/lib/status-colors";

interface PaymentHistoryProps {
  orgId: string;
}

export function PaymentHistory({ orgId }: PaymentHistoryProps) {
  const t = useTranslations("billing");
  const locale = useLocale();
  const { payments, isLoading } = usePayments(orgId);

  if (isLoading) {
    return (
      <div className="h-32 animate-pulse rounded-xl border border-slate-200 bg-slate-100" />
    );
  }

  if (payments.length === 0) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-6">
        <p className="text-slate-600">{t("payments.noPayments")}</p>
      </div>
    );
  }

  const formatAmount = (amount: number, currency: string) => {
    return new Intl.NumberFormat(locale, {
      style: "currency",
      currency: currency.toUpperCase(),
    }).format(amount / 100);
  };

  const getStatusLabel = (status: string) => {
    const statusMap: Record<string, string> = {
      succeeded: t("payments.succeeded"),
      pending: t("payments.pending"),
      failed: t("payments.failed"),
    };
    return statusMap[status] ?? status;
  };

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-slate-200 bg-slate-50/50">
          <tr>
            <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
              {t("payments.date")}
            </th>
            <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
              {t("payments.description")}
            </th>
            <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
              {t("payments.amount")}
            </th>
            <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
              {t("payments.status")}
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {payments.map((payment) => (
            <tr key={payment.id} className="group hover:bg-slate-50 transition-colors">
              <td className="px-6 py-5 text-slate-900">
                {formatDateShort(payment.createdAtUtc, locale)}
              </td>
              <td className="px-6 py-5 text-slate-600">
                {payment.description}
              </td>
              <td className="px-6 py-5 font-medium text-slate-900">
                {formatAmount(payment.amount, payment.currency)}
              </td>
              <td className="px-6 py-5">
                <span
                  className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusColor(payment.status, paymentStatusColors)}`}
                >
                  {getStatusLabel(payment.status)}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
