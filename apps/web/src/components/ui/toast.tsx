// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { useTranslations } from "next-intl";

export type ToastVariant = "default" | "destructive" | "success";

export type Toast = {
  id: string;
  title?: string;
  description?: string;
  variant?: ToastVariant;
  durationMs?: number;
};

type ToastContextValue = {
  toasts: Toast[];
  toast: (t: Omit<Toast, "id">) => void;
  dismiss: (id: string) => void;
};

const ToastContext = createContext<ToastContextValue | null>(null);

export function ToastProvider({ children }: { children: ReactNode }) {
  const tCommon = useTranslations("common");
  const [toasts, setToasts] = useState<Toast[]>([]);

  const dismiss = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const toast = useCallback(
    (t: Omit<Toast, "id">) => {
      const id = crypto.randomUUID();
      const toast: Toast = {
        id,
        durationMs: 4500,
        variant: "default",
        ...t,
      };
      setToasts((prev) => [toast, ...prev]);
      if (toast.durationMs && toast.durationMs > 0) {
        setTimeout(() => dismiss(id), toast.durationMs);
      }
    },
    [dismiss],
  );

  const value = useMemo(
    () => ({ toasts, toast, dismiss }),
    [toasts, toast, dismiss],
  );

  return (
    <ToastContext.Provider value={value}>
      {children}
      {/* UI container — ARIA live region so screen readers announce new toasts */}
      <div
        className="pointer-events-none fixed inset-x-0 bottom-4 z-50 mx-auto flex w-full max-w-xl flex-col gap-2 px-4"
        aria-live="polite"
        aria-relevant="additions"
      >
        {toasts.map((t) => (
          <div
            key={t.id}
            role="status"
            className={[
              "pointer-events-auto rounded-md border px-4 py-3 shadow-md",
              t.variant === "destructive" &&
                "border-red-200 bg-red-50 text-red-800",
              t.variant === "success" &&
                "border-emerald-200 bg-emerald-50 text-emerald-900",
              (!t.variant || t.variant === "default") &&
                "border-slate-200 bg-white text-slate-900",
            ]
              .filter(Boolean)
              .join(" ")}
          >
            <div className="flex items-start gap-3">
              <div className="flex-1">
                {t.title ? (
                  <div className="text-sm font-semibold">{t.title}</div>
                ) : null}
                {t.description ? (
                  <div className="text-sm opacity-80">{t.description}</div>
                ) : null}
              </div>
              <button
                className="ml-2 text-xs text-slate-500 hover:text-slate-700"
                onClick={() => dismiss(t.id)}
              >
                {tCommon("close")}
              </button>
            </div>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToast must be used within <ToastProvider>");
  return ctx;
}