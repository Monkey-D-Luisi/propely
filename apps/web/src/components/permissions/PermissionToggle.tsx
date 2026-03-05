// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { SpinnerIcon } from '@/components/ui/icons';

interface PermissionToggleProps {
  enabled: boolean;
  onChange: (enabled: boolean) => void;
  disabled: boolean;
  isLoading: boolean;
}

export function PermissionToggle({
  enabled,
  onChange,
  disabled,
  isLoading,
}: PermissionToggleProps) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={enabled}
      disabled={disabled || isLoading}
      onClick={() => onChange(!enabled)}
      className={`relative inline-flex h-6 w-11 shrink-0 cursor-pointer items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 ${
        enabled ? 'bg-primary-600' : 'bg-slate-200'
      } ${disabled || isLoading ? 'cursor-not-allowed opacity-50' : ''}`}
    >
      <span className="sr-only">Toggle permission</span>
      {isLoading ? (
        <span
          className={`pointer-events-none inline-flex h-5 w-5 transform items-center justify-center rounded-full bg-white shadow ring-0 transition-transform ${
            enabled ? 'translate-x-5' : 'translate-x-0.5'
          }`}
        >
          <SpinnerIcon className="h-3 w-3 text-slate-400" />
        </span>
      ) : (
        <span
          className={`pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition-transform ${
            enabled ? 'translate-x-5' : 'translate-x-0.5'
          }`}
        />
      )}
    </button>
  );
}
