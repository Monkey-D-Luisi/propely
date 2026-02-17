// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import React, { type SelectHTMLAttributes, forwardRef, useId } from 'react';
import { useFormContext, type FieldValues, type Path } from 'react-hook-form';

export interface FormSelectOption {
    label: string;
    value: string;
}

export interface FormSelectProps<T extends FieldValues>
    extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'name' | 'onChange' | 'onBlur'> {
    name: Path<T>;
    label: string;
    options: FormSelectOption[];
    placeholder?: string;
}

function FormSelectInner<T extends FieldValues>(
    { name, label, options, placeholder, className = '', ...props }: FormSelectProps<T>,
    ref: React.ForwardedRef<HTMLSelectElement>
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
            <label htmlFor={id} className="text-sm font-medium text-slate-700">
                {label}
            </label>
            <select
                id={id}
                {...props}
                {...registerProps}
                ref={(e) => {
                    registerRef(e);
                    if (typeof ref === 'function') {
                        ref(e);
                    } else if (ref) {
                        (ref as React.MutableRefObject<HTMLSelectElement | null>).current = e;
                    }
                }}
                className={`w-full rounded-lg border border-slate-200 px-4 py-2.5 text-sm transition-all focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600 ${errorMessage ? 'border-red-300' : ''} ${className}`}
                aria-invalid={errorMessage ? 'true' : 'false'}
                aria-describedby={errorMessage ? `${String(name)}-error` : undefined}
            >
                {placeholder ? (
                    <option value="" disabled>
                        {placeholder}
                    </option>
                ) : null}
                {options.map((option) => (
                    <option key={option.value} value={option.value}>
                        {option.label}
                    </option>
                ))}
            </select>
            {errorMessage ? (
                <span id={`${String(name)}-error`} className="text-sm text-red-600" role="alert">
                    {errorMessage}
                </span>
            ) : null}
        </div>
    );
}

export const FormSelect = forwardRef(FormSelectInner) as <T extends FieldValues>(
    props: FormSelectProps<T> & { ref?: React.ForwardedRef<HTMLSelectElement> }
) => React.ReactElement;

// displayName assignment
(FormSelect as React.FC).displayName = 'FormSelect';
