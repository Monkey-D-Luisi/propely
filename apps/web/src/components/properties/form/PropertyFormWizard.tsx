// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import type { Property } from '@/lib/schemas';
import { usePropertyForm } from '@/hooks/usePropertyForm';
import { StepIndicator } from './StepIndicator';
import { useToast } from '@/components/ui/toast';
import { useChangePropertyStatus } from '@/hooks/properties';
import {
  BasicInfoStep,
  LocationStep,
  FeaturesStep,
  FinancialStep,
  DescriptionsStep,
  MediaStep,
} from './steps';

interface PropertyFormWizardProps {
  existingProperty?: Property | null;
}

const STEP_COMPONENTS = [
  BasicInfoStep,
  LocationStep,
  FeaturesStep,
  FinancialStep,
  DescriptionsStep,
  MediaStep,
];

export function PropertyFormWizard({ existingProperty }: PropertyFormWizardProps) {
  const t = useTranslations('properties.form');
  const router = useRouter();
  const { toast } = useToast();
  const changeStatus = useChangePropertyStatus();

  const {
    currentStep,
    completedSteps,
    formData,
    isEditMode,
    isSubmitting,
    lastSaved,
    totalSteps,
    updateFormData,
    nextStep,
    previousStep,
    saveDraft,
    publish,
  } = usePropertyForm(existingProperty);

  const handleSaveDraft = async () => {
    const result = await saveDraft();
    if (result) {
      toast({ title: t('saveDraft'), variant: 'success' });
      router.push(`/properties/${result.id}`);
    }
  };

  const handlePublish = async () => {
    const result = await publish();
    if (result) {
      if (result.status === 'Draft') {
        try {
          await changeStatus(result.id, 'Active');
        } catch {
          // Property created but status change failed - still navigate
        }
      }
      toast({ title: isEditMode ? t('save') : t('publish'), variant: 'success' });
      router.push(`/properties/${result.id}`);
    }
  };

  const StepComponent = STEP_COMPONENTS[currentStep];

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8">
      <StepIndicator currentStep={currentStep} completedSteps={completedSteps} />

      <div className="min-h-[300px]">
        {StepComponent && (
          <StepComponent data={formData} onChange={updateFormData} />
        )}
      </div>

      {/* Auto-save indicator */}
      {lastSaved && (
        <p className="mt-2 text-xs text-slate-400">
          {t('autoSaved')} {lastSaved.toLocaleTimeString()}
        </p>
      )}

      {/* Navigation buttons */}
      <div className="mt-8 flex items-center justify-between border-t border-slate-100 pt-6">
        <button
          type="button"
          onClick={previousStep}
          disabled={currentStep === 0}
          className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {t('previous')}
        </button>

        <div className="flex items-center gap-3">
          {!isEditMode && (
            <button
              type="button"
              onClick={() => void handleSaveDraft()}
              disabled={isSubmitting}
              className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50"
            >
              {isSubmitting ? t('saving') : t('saveDraft')}
            </button>
          )}

          {currentStep < totalSteps - 1 ? (
            <button
              type="button"
              onClick={nextStep}
              className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98]"
            >
              {t('next')}
            </button>
          ) : (
            <button
              type="button"
              onClick={() => void handlePublish()}
              disabled={isSubmitting}
              className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] disabled:opacity-50"
            >
              {isSubmitting ? t('saving') : isEditMode ? t('save') : t('publish')}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
