// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { type ButtonHTMLAttributes } from 'react';
import { useFormContext } from 'react-hook-form';
import { useTranslations } from 'next-intl';
import { Button } from '@/components/ui/button';

export interface FormSubmitButtonProps
    extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, 'type'> {
    loadingText?: string;
}

export function FormSubmitButton({
    children,
    loadingText,
    disabled,
    ...props
}: FormSubmitButtonProps) {
    const { formState } = useFormContext();
    const t = useTranslations('common');
    const isSubmitting = formState.isSubmitting;

    return (
        <Button type="submit" disabled={isSubmitting || disabled} {...props}>
            {isSubmitting ? (loadingText ?? t('submitting')) : children}
        </Button>
    );
}
