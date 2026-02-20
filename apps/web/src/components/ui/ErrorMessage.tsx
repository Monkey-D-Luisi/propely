// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

interface ErrorMessageProps {
  message: string;
  className?: string;
}

/**
 * Reusable error display component with consistent styling.
 * Use for page-level or section-level error states.
 */
export function ErrorMessage({ message, className = '' }: ErrorMessageProps) {
  return (
    <div
      role="alert"
      className={`rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700 ${className}`.trim()}
    >
      {message}
    </div>
  );
}
