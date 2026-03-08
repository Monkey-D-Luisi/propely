// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useFetch } from '@/hooks/use-fetch';
import { aiApiFetch } from '@/lib/api';

export type Suggestion = {
  type: string;
  priority: string;
  message: string;
  actionUrl?: string;
  actionLabel?: string;
};

export type SuggestionsData = {
  suggestions: Suggestion[];
  isLoading: boolean;
  error: unknown;
};

export function useSuggestions(): SuggestionsData {
  const { data, isLoading, error } = useFetch<Suggestion[]>(
    '/v1/suggestions',
    { fetcher: aiApiFetch },
  );

  return {
    suggestions: data ?? [],
    isLoading,
    error,
  };
}
