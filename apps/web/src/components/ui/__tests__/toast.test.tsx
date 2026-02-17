// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { useToast } from '@/components/ui/toast';
import { render, act } from '@testing-library/react';

// Test component that triggers toasts
function ToastTrigger({ variant, title, description }: { variant?: 'default' | 'destructive' | 'success'; title?: string; description?: string }) {
  const { toast } = useToast();
  return (
    <button
      type="button"
      onClick={() => toast({ title, description, variant })}
    >
      Show toast
    </button>
  );
}

beforeEach(() => {
  vi.useFakeTimers({ shouldAdvanceTime: true });
});

afterEach(() => {
  vi.useRealTimers();
});

describe('Toast', () => {
  it('shows a toast with title and description', async () => {
    const { user } = renderWithProviders(
      <ToastTrigger title="Success!" description="Operation completed." />,
    );

    await user.click(screen.getByRole('button', { name: 'Show toast' }));

    expect(screen.getByText('Success!')).toBeInTheDocument();
    expect(screen.getByText('Operation completed.')).toBeInTheDocument();
  });

  it('shows a destructive toast', async () => {
    const { user } = renderWithProviders(
      <ToastTrigger title="Error!" variant="destructive" />,
    );

    await user.click(screen.getByRole('button', { name: 'Show toast' }));

    const status = screen.getByRole('status');
    expect(status.className).toContain('bg-red-50');
  });

  it('shows a success toast', async () => {
    const { user } = renderWithProviders(
      <ToastTrigger title="Done!" variant="success" />,
    );

    await user.click(screen.getByRole('button', { name: 'Show toast' }));

    const status = screen.getByRole('status');
    expect(status.className).toContain('bg-emerald-50');
  });

  it('dismisses toast when close button is clicked', async () => {
    const { user } = renderWithProviders(
      <ToastTrigger title="Dismissable" />,
    );

    await user.click(screen.getByRole('button', { name: 'Show toast' }));
    expect(screen.getByText('Dismissable')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Close' }));

    await waitFor(() => {
      expect(screen.queryByText('Dismissable')).not.toBeInTheDocument();
    });
  });

  it('auto-dismisses toast after duration', async () => {
    const { user } = renderWithProviders(
      <ToastTrigger title="Auto dismiss" />,
    );

    await user.click(screen.getByRole('button', { name: 'Show toast' }));
    expect(screen.getByText('Auto dismiss')).toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(5000);
    });

    await waitFor(() => {
      expect(screen.queryByText('Auto dismiss')).not.toBeInTheDocument();
    });
  });

  it('renders ARIA live region for accessibility', async () => {
    const { user } = renderWithProviders(
      <ToastTrigger title="Accessible" />,
    );

    await user.click(screen.getByRole('button', { name: 'Show toast' }));

    const liveRegion = screen.getByText('Accessible').closest('[aria-live]');
    expect(liveRegion).toHaveAttribute('aria-live', 'polite');
  });

  it('throws error when useToast is used outside provider', () => {
    function BadComponent() {
      useToast();
      return null;
    }

    expect(() => {
      render(<BadComponent />);
    }).toThrow('useToast must be used within <ToastProvider>');
  });

  it('can show multiple toasts', async () => {
    function MultiToastTrigger() {
      const { toast } = useToast();
      return (
        <>
          <button type="button" onClick={() => toast({ title: 'First' })}>Toast 1</button>
          <button type="button" onClick={() => toast({ title: 'Second' })}>Toast 2</button>
        </>
      );
    }

    const { user } = renderWithProviders(<MultiToastTrigger />);

    await user.click(screen.getByRole('button', { name: 'Toast 1' }));
    await user.click(screen.getByRole('button', { name: 'Toast 2' }));

    expect(screen.getByText('First')).toBeInTheDocument();
    expect(screen.getByText('Second')).toBeInTheDocument();
  });
});
