// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { SuggestionFeed } from '@/components/suggestions/SuggestionFeed';

vi.mock('next-intl', async () => {
  const actual = await vi.importActual('next-intl');
  return {
    ...actual,
    useTranslations: () => (key: string) => {
      const translations: Record<string, string> = {
        title: 'Suggestions',
        allCaughtUp: 'All caught up! No suggestions right now.',
      };
      return translations[key] ?? key;
    },
  };
});

vi.mock('@/hooks/use-suggestions', () => ({
  useSuggestions: vi.fn(),
}));

import { useSuggestions } from '@/hooks/use-suggestions';

const mockUseSuggestions = vi.mocked(useSuggestions);

beforeEach(() => {
  vi.resetAllMocks();
});

describe('SuggestionFeed', () => {
  it('renders suggestions when available', () => {
    mockUseSuggestions.mockReturnValue({
      suggestions: [
        {
          type: 'StaleLeads',
          priority: 'High',
          message: 'You have 8 new leads waiting.',
          actionUrl: '/leads?status=New',
          actionLabel: 'View new leads',
        },
        {
          type: 'EmptyCalendar',
          priority: 'Medium',
          message: 'No appointments scheduled.',
          actionUrl: '/appointments',
          actionLabel: 'View calendar',
        },
      ],
      isLoading: false,
      error: null,
    });

    renderWithProviders(<SuggestionFeed />);

    expect(screen.getAllByTestId('suggestion-card')).toHaveLength(2);
    expect(screen.getByText('You have 8 new leads waiting.')).toBeInTheDocument();
    expect(screen.getByText('No appointments scheduled.')).toBeInTheDocument();
  });

  it('renders loading skeleton when loading', () => {
    mockUseSuggestions.mockReturnValue({
      suggestions: [],
      isLoading: true,
      error: null,
    });

    renderWithProviders(<SuggestionFeed />);

    expect(screen.getByTestId('suggestions-loading')).toBeInTheDocument();
  });

  it('renders empty state when no suggestions', () => {
    mockUseSuggestions.mockReturnValue({
      suggestions: [],
      isLoading: false,
      error: null,
    });

    renderWithProviders(<SuggestionFeed />);

    expect(screen.getByTestId('suggestions-empty')).toBeInTheDocument();
  });

  it('renders action links', () => {
    mockUseSuggestions.mockReturnValue({
      suggestions: [
        {
          type: 'DraftProperty',
          priority: 'Medium',
          message: '5 properties in Draft status.',
          actionUrl: '/properties?status=Draft',
          actionLabel: 'View draft properties',
        },
      ],
      isLoading: false,
      error: null,
    });

    renderWithProviders(<SuggestionFeed />);

    const link = screen.getByText('View draft properties');
    expect(link).toBeInTheDocument();
    expect(link.closest('a')).toHaveAttribute('href', '/properties?status=Draft');
  });
});
