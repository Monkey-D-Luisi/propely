// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

export interface FormErrorProps {
    message: string | null | undefined;
}

export function FormError({ message }: FormErrorProps) {
    if (!message) return null;

    return (
        <div
            className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700"
            role="alert"
        >
            {message}
        </div>
    );
}
