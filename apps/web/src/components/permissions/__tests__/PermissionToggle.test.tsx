// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { PermissionToggle } from '@/components/permissions/PermissionToggle';
import { renderWithProviders, screen } from '@test/utils';

describe('PermissionToggle', () => {
  it('renders a switch role element', () => {
    renderWithProviders(
      <PermissionToggle enabled={false} onChange={vi.fn()} disabled={false} isLoading={false} />,
    );

    expect(screen.getByRole('switch')).toBeInTheDocument();
  });

  it('reflects enabled state with aria-checked', () => {
    renderWithProviders(
      <PermissionToggle enabled={true} onChange={vi.fn()} disabled={false} isLoading={false} />,
    );

    expect(screen.getByRole('switch')).toHaveAttribute('aria-checked', 'true');
  });

  it('reflects disabled state with aria-checked false', () => {
    renderWithProviders(
      <PermissionToggle enabled={false} onChange={vi.fn()} disabled={false} isLoading={false} />,
    );

    expect(screen.getByRole('switch')).toHaveAttribute('aria-checked', 'false');
  });

  it('calls onChange with opposite value when clicked', async () => {
    const onChange = vi.fn();
    const { user } = renderWithProviders(
      <PermissionToggle enabled={false} onChange={onChange} disabled={false} isLoading={false} />,
    );

    await user.click(screen.getByRole('switch'));

    expect(onChange).toHaveBeenCalledWith(true);
  });

  it('calls onChange with false when enabled toggle is clicked', async () => {
    const onChange = vi.fn();
    const { user } = renderWithProviders(
      <PermissionToggle enabled={true} onChange={onChange} disabled={false} isLoading={false} />,
    );

    await user.click(screen.getByRole('switch'));

    expect(onChange).toHaveBeenCalledWith(false);
  });

  it('does not call onChange when disabled', async () => {
    const onChange = vi.fn();
    const { user } = renderWithProviders(
      <PermissionToggle enabled={false} onChange={onChange} disabled={true} isLoading={false} />,
    );

    await user.click(screen.getByRole('switch'));

    expect(onChange).not.toHaveBeenCalled();
  });

  it('does not call onChange when loading', async () => {
    const onChange = vi.fn();
    const { user } = renderWithProviders(
      <PermissionToggle enabled={false} onChange={onChange} disabled={false} isLoading={true} />,
    );

    await user.click(screen.getByRole('switch'));

    expect(onChange).not.toHaveBeenCalled();
  });

  it('applies primary-600 background when enabled', () => {
    renderWithProviders(
      <PermissionToggle enabled={true} onChange={vi.fn()} disabled={false} isLoading={false} />,
    );

    expect(screen.getByRole('switch').className).toContain('bg-primary-600');
  });

  it('applies slate-200 background when disabled', () => {
    renderWithProviders(
      <PermissionToggle enabled={false} onChange={vi.fn()} disabled={false} isLoading={false} />,
    );

    expect(screen.getByRole('switch').className).toContain('bg-slate-200');
  });

  it('applies reduced opacity when disabled', () => {
    renderWithProviders(
      <PermissionToggle enabled={false} onChange={vi.fn()} disabled={true} isLoading={false} />,
    );

    expect(screen.getByRole('switch').className).toContain('opacity-50');
  });
});
