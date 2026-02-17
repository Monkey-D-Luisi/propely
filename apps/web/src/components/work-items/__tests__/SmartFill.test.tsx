// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { SmartFill } from '@/components/work-items/SmartFill';

vi.mock('@/hooks/work-items', () => ({
  useParseWorkItem: vi.fn(),
}));

import { useParseWorkItem } from '@/hooks/work-items';

const mockUseParseWorkItem = vi.mocked(useParseWorkItem);

beforeEach(() => {
  vi.resetAllMocks();
  mockUseParseWorkItem.mockReturnValue({
    parse: vi.fn().mockResolvedValue(null),
    isLoading: false,
    error: null,
  });
});

describe('SmartFill', () => {
  it('renders Smart Fill section with textarea and button', () => {
    renderWithProviders(<SmartFill onFill={vi.fn()} />);

    expect(screen.getByText('AI Smart Fill')).toBeInTheDocument();
    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /smart fill/i })).toBeInTheDocument();
  });

  it('disables button when textarea is empty', () => {
    renderWithProviders(<SmartFill onFill={vi.fn()} />);

    const button = screen.getByRole('button', { name: /smart fill/i });
    expect(button).toBeDisabled();
  });

  it('enables button when textarea has text', async () => {
    const { user } = renderWithProviders(<SmartFill onFill={vi.fn()} />);

    const textarea = screen.getByRole('textbox');
    await user.type(textarea, 'Fix the login bug');

    const button = screen.getByRole('button', { name: /smart fill/i });
    expect(button).not.toBeDisabled();
  });

  it('shows loading state while parsing', () => {
    mockUseParseWorkItem.mockReturnValue({
      parse: vi.fn().mockReturnValue(new Promise(() => {})),
      isLoading: true,
      error: null,
    });

    renderWithProviders(<SmartFill onFill={vi.fn()} />);

    expect(screen.getByText('Parsing...')).toBeInTheDocument();
  });

  it('calls onFill with parsed data on success', async () => {
    const mockParse = vi.fn().mockResolvedValue({
      title: 'Login bug fix',
      description: 'Fix email validation',
      status: 'Active',
      priority: 'High',
      type: 'Bug',
      dueDateUtc: '2026-03-01T00:00:00Z',
      estimatedEffort: 'L',
      confidence: 0.9,
    });

    mockUseParseWorkItem.mockReturnValue({
      parse: mockParse,
      isLoading: false,
      error: null,
    });

    const onFill = vi.fn();
    const { user } = renderWithProviders(<SmartFill onFill={onFill} />);

    const textarea = screen.getByRole('textbox');
    await user.type(textarea, 'Fix the login bug');

    const button = screen.getByRole('button', { name: /smart fill/i });
    await user.click(button);

    await waitFor(() => {
      expect(onFill).toHaveBeenCalledWith({
        title: 'Login bug fix',
        description: 'Fix email validation',
        status: 'Active',
        priority: 'High',
        type: 'Bug',
        dueDate: '2026-03-01T00:00:00Z',
        estimatedEffort: 'L',
      });
    });
  });

  it('shows success message after successful parse', async () => {
    const mockParse = vi.fn().mockResolvedValue({
      title: 'Login bug fix',
      description: 'Fix email validation',
      status: 'Active',
      priority: 'High',
      type: 'Bug',
      dueDateUtc: null,
      estimatedEffort: null,
      confidence: 0.9,
    });

    mockUseParseWorkItem.mockReturnValue({
      parse: mockParse,
      isLoading: false,
      error: null,
    });

    const { user } = renderWithProviders(<SmartFill onFill={vi.fn()} />);

    await user.type(screen.getByRole('textbox'), 'Fix the login bug');
    await user.click(screen.getByRole('button', { name: /smart fill/i }));

    await waitFor(() => {
      expect(screen.getByText('Fields filled! Review and edit before submitting.')).toBeInTheDocument();
    });
  });

  it('shows error message when parse fails', async () => {
    const mockParse = vi.fn().mockResolvedValue(null);

    mockUseParseWorkItem.mockReturnValue({
      parse: mockParse,
      isLoading: false,
      error: null,
    });

    const { user } = renderWithProviders(<SmartFill onFill={vi.fn()} />);

    await user.type(screen.getByRole('textbox'), 'Fix the login bug');
    await user.click(screen.getByRole('button', { name: /smart fill/i }));

    await waitFor(() => {
      expect(screen.getByText('AI parsing failed. Please fill the form manually.')).toBeInTheDocument();
    });
  });

  it('renders hint text', () => {
    renderWithProviders(<SmartFill onFill={vi.fn()} />);

    expect(screen.getByText('AI will extract title, description, status, priority, type, due date, and effort from your text')).toBeInTheDocument();
  });
});
