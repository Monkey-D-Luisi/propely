// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import React, { type InputHTMLAttributes, forwardRef, useId } from 'react';
import { useFormContext, type FieldValues, type Path } from 'react-hook-form';

export interface FormFieldProps<T extends FieldValues>
    extends Omit<InputHTMLAttributes<HTMLInputElement>, 'name' | 'onChange' | 'onBlur'> {
    name: Path<T>;
    label: string;
    /** Optional node rendered to the right of the label (e.g. "Forgot password?" link) */
    labelRight?: React.ReactNode;
}

function FormFieldInner<T extends FieldValues>(
    { name, label, labelRight, className = '', type = 'text', ...props }: FormFieldProps<T>,
    ref: React.ForwardedRef<HTMLInputElement>
) {
    const {
        register,
        formState: { errors },
    } = useFormContext<T>();

    const id = useId();
    const error = errors[name];
    const errorMessage = error?.message as string | undefined;

    const { ref: registerRef, ...registerProps } = register(name);

    return (
        <div className="flex flex-col gap-1.5">
            {labelRight ? (
                <div className="flex items-center justify-between">
                    <label htmlFor={id} className="text-sm font-medium text-slate-700">
                        {label}
                    </label>
                    {labelRight}
                </div>
            ) : (
                <label htmlFor={id} className="text-sm font-medium text-slate-700">
                    {label}
                </label>
            )}
            <input
                id={id}
                {...props}
                {...registerProps}
                ref={(e) => {
                    registerRef(e);
                    if (typeof ref === 'function') {
                        ref(e);
                    } else if (ref) {
                        (ref as React.MutableRefObject<HTMLInputElement | null>).current = e;
                    }
                }}
                type={type}
                className={`w-full rounded-lg border border-slate-200 px-4 py-3 text-sm transition-all focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600 ${errorMessage ? 'border-red-300' : ''} ${className}`}
                aria-invalid={errorMessage ? 'true' : 'false'}
                aria-describedby={errorMessage ? `${String(name)}-error` : undefined}
            />
            {errorMessage ? (
                <span id={`${String(name)}-error`} className="text-sm text-red-600" role="alert">
                    {errorMessage}
                </span>
            ) : null}
        </div>
    );
}

export const FormField = forwardRef(FormFieldInner) as <T extends FieldValues>(
    props: FormFieldProps<T> & { ref?: React.ForwardedRef<HTMLInputElement> }
) => React.ReactElement;
// displayName assignment
(FormField as React.FC).displayName = 'FormField';
