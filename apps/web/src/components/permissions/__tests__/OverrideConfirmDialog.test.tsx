// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { OverrideConfirmDialog } from '@/components/permissions/OverrideConfirmDialog';
import { renderWithProviders, screen } from '@test/utils';

describe('OverrideConfirmDialog', () => {
  it('renders dialog with title', () => {
    renderWithProviders(
      <OverrideConfirmDialog
        permissionName="View all properties"
        userName="Juan Lopez"
        isProcessing={false}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    );

    expect(screen.getByRole('heading', { name: 'Revoke Permission' })).toBeInTheDocument();
  });

  it('renders warning message with permission and user name', () => {
    renderWithProviders(
      <OverrideConfirmDialog
        permissionName="View all properties"
        userName="Juan Lopez"
        isProcessing={false}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    );

    expect(
      screen.getByText(/View all properties.*Juan Lopez/),
    ).toBeInTheDocument();
  });

  it('calls onConfirm when confirm button is clicked', async () => {
    const onConfirm = vi.fn();
    const { user } = renderWithProviders(
      <OverrideConfirmDialog
        permissionName="View all properties"
        userName="Juan Lopez"
        isProcessing={false}
        onConfirm={onConfirm}
        onCancel={vi.fn()}
      />,
    );

    const buttons = screen.getAllByText('Revoke Permission');
    const confirmButton = buttons.find((el) => el.tagName === 'BUTTON')!;
    await user.click(confirmButton);

    expect(onConfirm).toHaveBeenCalledOnce();
  });

  it('calls onCancel when cancel button is clicked', async () => {
    const onCancel = vi.fn();
    const { user } = renderWithProviders(
      <OverrideConfirmDialog
        permissionName="View all properties"
        userName="Juan Lopez"
        isProcessing={false}
        onConfirm={vi.fn()}
        onCancel={onCancel}
      />,
    );

    await user.click(screen.getByText('Cancel'));

    expect(onCancel).toHaveBeenCalledOnce();
  });

  it('disables confirm button when processing', () => {
    renderWithProviders(
      <OverrideConfirmDialog
        permissionName="View all properties"
        userName="Juan Lopez"
        isProcessing={true}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    );

    const buttons = screen.getAllByText('Revoke Permission');
    const confirmButton = buttons.find((el) => el.tagName === 'BUTTON')!;
    expect(confirmButton).toBeDisabled();
  });

  it('has dialog role with aria-modal', () => {
    renderWithProviders(
      <OverrideConfirmDialog
        permissionName="View all properties"
        userName="Juan Lopez"
        isProcessing={false}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    );

    expect(screen.getByRole('dialog')).toHaveAttribute('aria-modal', 'true');
  });
});
