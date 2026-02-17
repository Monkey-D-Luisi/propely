// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import React, { type TextareaHTMLAttributes, forwardRef, useId } from 'react';
import { useFormContext, type FieldValues, type Path } from 'react-hook-form';

export interface FormTextareaProps<T extends FieldValues>
    extends Omit<TextareaHTMLAttributes<HTMLTextAreaElement>, 'name' | 'onChange' | 'onBlur'> {
    name: Path<T>;
    label: string;
}

function FormTextareaInner<T extends FieldValues>(
    { name, label, className = '', ...props }: FormTextareaProps<T>,
    ref: React.ForwardedRef<HTMLTextAreaElement>
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
            <textarea
                id={id}
                {...props}
                {...registerProps}
                ref={(e) => {
                    registerRef(e);
                    if (typeof ref === 'function') {
                        ref(e);
                    } else if (ref) {
                        (ref as React.MutableRefObject<HTMLTextAreaElement | null>).current = e;
                    }
                }}
                className={`w-full rounded-lg border border-slate-200 px-4 py-2.5 text-sm transition-all focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600 ${errorMessage ? 'border-red-300' : ''} ${className}`}
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

export const FormTextarea = forwardRef(FormTextareaInner) as <T extends FieldValues>(
    props: FormTextareaProps<T> & { ref?: React.ForwardedRef<HTMLTextAreaElement> }
) => React.ReactElement;
(FormTextarea as React.FC).displayName = 'FormTextarea';
