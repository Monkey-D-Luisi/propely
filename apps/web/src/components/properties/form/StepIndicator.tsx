// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';

const steps = ['basicInfo', 'location', 'features', 'financial', 'descriptions', 'media'] as const;

interface StepIndicatorProps {
  currentStep: number;
  completedSteps: Set<number>;
}

export function StepIndicator({ currentStep, completedSteps }: StepIndicatorProps) {
  const t = useTranslations('properties.form.step');

  return (
    <nav aria-label="Form steps" className="mb-8">
      <ol className="flex items-center gap-2">
        {steps.map((step, index) => {
          const isActive = index === currentStep;
          const isCompleted = completedSteps.has(index);

          return (
            <li key={step} className="flex items-center gap-2">
              {index > 0 && (
                <div className={`h-px w-6 sm:w-10 ${isCompleted || isActive ? 'bg-primary-600' : 'bg-slate-200'}`} />
              )}
              <div className="flex items-center gap-1.5">
                <span
                  className={`flex h-7 w-7 items-center justify-center rounded-full text-xs font-semibold ${
                    isActive
                      ? 'bg-primary-600 text-white'
                      : isCompleted
                      ? 'bg-primary-100 text-primary-700'
                      : 'bg-slate-100 text-slate-400'
                  }`}
                >
                  {isCompleted ? (
                    <span className="material-symbols-outlined text-sm" aria-hidden="true">check</span>
                  ) : (
                    index + 1
                  )}
                </span>
                <span
                  className={`hidden text-xs font-medium sm:inline ${
                    isActive ? 'text-primary-700' : isCompleted ? 'text-slate-700' : 'text-slate-400'
                  }`}
                >
                  {t(step)}
                </span>
              </div>
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
