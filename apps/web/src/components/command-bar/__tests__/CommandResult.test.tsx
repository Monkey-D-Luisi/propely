// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { CommandResult } from '@/components/command-bar/CommandResult';

describe('CommandResult', () => {
  it('renders loading state with spinner', () => {
    renderWithProviders(
      <CommandResult result={null} error={null} isLoading={true} onRetry={vi.fn()} />,
    );

    expect(screen.getByRole('status')).toBeInTheDocument();
    expect(screen.getByText('Processing...')).toBeInTheDocument();
  });

  it('renders error state with retry button', () => {
    renderWithProviders(
      <CommandResult
        result={null}
        error="Network error"
        isLoading={false}
        onRetry={vi.fn()}
      />,
    );

    expect(screen.getByRole('alert')).toBeInTheDocument();
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('Network error')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Try again' })).toBeInTheDocument();
  });

  it('calls onRetry when retry button is clicked', async () => {
    const onRetry = vi.fn();
    const { user } = renderWithProviders(
      <CommandResult
        result={null}
        error="Network error"
        isLoading={false}
        onRetry={onRetry}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Try again' }));
    expect(onRetry).toHaveBeenCalledOnce();
  });

  it('renders success state with message', () => {
    renderWithProviders(
      <CommandResult
        result={{ message: 'Property created successfully' }}
        error={null}
        isLoading={false}
        onRetry={vi.fn()}
      />,
    );

    expect(screen.getByText('Done')).toBeInTheDocument();
    expect(screen.getByText('Property created successfully')).toBeInTheDocument();
  });

  it('renders nothing when no result, no error, and not loading', () => {
    renderWithProviders(
      <CommandResult result={null} error={null} isLoading={false} onRetry={vi.fn()} />,
    );

    expect(screen.queryByRole('status')).not.toBeInTheDocument();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
    expect(screen.queryByText('Done')).not.toBeInTheDocument();
  });

  it('does not render success for needsConfirmation results', () => {
    renderWithProviders(
      <CommandResult
        result={{
          message: 'Please confirm',
          needsConfirmation: true,
          confirmationId: '123',
        }}
        error={null}
        isLoading={false}
        onRetry={vi.fn()}
      />,
    );

    expect(screen.queryByRole('status')).not.toBeInTheDocument();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
    expect(screen.queryByText('Done')).not.toBeInTheDocument();
  });
});
