// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useCallback } from 'react';
import type { Property } from '@/lib/schemas';
import { useCreateProperty, useUpdateProperty } from '@/hooks/properties';
import { useAutoSave } from '@/hooks/useAutoSave';

const TOTAL_STEPS = 6;

const emptyFormData: Record<string, unknown> = {
  title: '',
  propertyType: 'Apartment',
  operationType: 'Sale',
  description: { es: '', en: '', pt: '' },
  address: {},
  features: {},
  financials: {},
  virtualTourUrl: '',
  videoUrl: '',
};

export function usePropertyForm(existingProperty?: Property | null) {
  const isEditMode = !!existingProperty;

  const [currentStep, setCurrentStep] = useState(0);
  const [completedSteps, setCompletedSteps] = useState<Set<number>>(new Set());
  const [formData, setFormData] = useState<Record<string, unknown>>(() => {
    if (existingProperty) {
      return {
        title: existingProperty.title,
        propertyType: existingProperty.propertyType,
        operationType: existingProperty.operationType,
        description: existingProperty.description ?? { es: '', en: '', pt: '' },
        address: existingProperty.address ?? {},
        features: existingProperty.features ?? {},
        financials: existingProperty.financials ?? {},
        virtualTourUrl: existingProperty.virtualTourUrl ?? '',
        videoUrl: existingProperty.videoUrl ?? '',
      };
    }
    return { ...emptyFormData };
  });
  const [isSubmitting, setSubmitting] = useState(false);
  const [error, setError] = useState<unknown>(null);

  const createProperty = useCreateProperty();
  const updateProperty = useUpdateProperty();
  const { save, load, clear, lastSaved } = useAutoSave(formData, !isEditMode);

  const updateFormData = useCallback((data: Record<string, unknown>) => {
    setFormData(data);
  }, []);

  const goToStep = useCallback((step: number) => {
    if (step >= 0 && step < TOTAL_STEPS) {
      setCompletedSteps((prev) => {
        const next = new Set(prev);
        next.add(currentStep);
        return next;
      });
      setCurrentStep(step);
    }
  }, [currentStep]);

  const nextStep = useCallback(() => {
    goToStep(currentStep + 1);
  }, [currentStep, goToStep]);

  const previousStep = useCallback(() => {
    goToStep(currentStep - 1);
  }, [currentStep, goToStep]);

  const loadDraft = useCallback(() => {
    const draft = load();
    if (draft) {
      setFormData(draft);
    }
    return draft;
  }, [load]);

  const saveDraft = useCallback(async (): Promise<Property | null> => {
    setSubmitting(true);
    setError(null);
    try {
      const result = isEditMode && existingProperty
        ? await updateProperty(existingProperty.id, formData)
        : await createProperty(formData);
      clear();
      return result;
    } catch (e) {
      setError(e);
      return null;
    } finally {
      setSubmitting(false);
    }
  }, [isEditMode, existingProperty, formData, createProperty, updateProperty, clear]);

  const publish = useCallback(async (): Promise<Property | null> => {
    setSubmitting(true);
    setError(null);
    try {
      const result = isEditMode && existingProperty
        ? await updateProperty(existingProperty.id, formData)
        : await createProperty(formData);
      clear();
      return result;
    } catch (e) {
      setError(e);
      return null;
    } finally {
      setSubmitting(false);
    }
  }, [isEditMode, existingProperty, formData, createProperty, updateProperty, clear]);

  return {
    currentStep,
    completedSteps,
    formData,
    isEditMode,
    isSubmitting,
    error,
    lastSaved,
    totalSteps: TOTAL_STEPS,
    updateFormData,
    goToStep,
    nextStep,
    previousStep,
    loadDraft,
    saveDraft,
    publish,
    save,
    clear,
  };
}
