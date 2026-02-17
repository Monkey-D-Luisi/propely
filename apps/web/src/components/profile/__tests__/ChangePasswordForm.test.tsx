// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ChangePasswordForm } from '@/components/profile/ChangePasswordForm';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/orgs', () => ({
  useChangePassword: vi.fn(),
}));
vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn(),
}));

import { useChangePassword } from '@/hooks/orgs';
import { ensureCsrfToken } from '@/lib/csrf';

const mockUseChangePassword = vi.mocked(useChangePassword);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);

beforeEach(() => {
  vi.resetAllMocks();
  mockEnsureCsrfToken.mockResolvedValue('test-csrf-token');
  mockUseChangePassword.mockReturnValue(vi.fn().mockResolvedValue({ ok: true }));
});

describe('ChangePasswordForm', () => {
  it('renders password form fields', async () => {
    renderWithProviders(<ChangePasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Current password')).toBeInTheDocument();
    });
    expect(screen.getByLabelText('New password')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Update password' })).toBeInTheDocument();
  });

  it('shows change password title', () => {
    renderWithProviders(<ChangePasswordForm />);

    expect(screen.getByRole('heading', { name: 'Change password' })).toBeInTheDocument();
  });

  it('shows validation error for empty current password', async () => {
    const { user } = renderWithProviders(<ChangePasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('New password')).toBeInTheDocument();
    });

    const newPasswordInput = screen.getByLabelText('New password');
    await user.type(newPasswordInput, 'NewPassword123!');
    await user.click(screen.getByRole('button', { name: 'Update password' }));

    await waitFor(() => {
      expect(screen.getByText('Current password is required.')).toBeInTheDocument();
    });
  });

  it('shows validation error for short new password', async () => {
    const { user } = renderWithProviders(<ChangePasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Current password')).toBeInTheDocument();
    });

    const currentInput = screen.getByLabelText('Current password');
    const newInput = screen.getByLabelText('New password');
    await user.type(currentInput, 'OldPassword123!');
    await user.type(newInput, 'short');
    await user.click(screen.getByRole('button', { name: 'Update password' }));

    await waitFor(() => {
      expect(screen.getByText('New password must be at least 8 characters.')).toBeInTheDocument();
    });
  });

  it('submits successfully and shows success toast', async () => {
    const changeFn = vi.fn().mockResolvedValue({ ok: true });
    mockUseChangePassword.mockReturnValue(changeFn);

    const { user } = renderWithProviders(<ChangePasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Current password')).toBeInTheDocument();
    });

    const currentInput = screen.getByLabelText('Current password');
    const newInput = screen.getByLabelText('New password');
    await user.type(currentInput, 'OldPassword123!');
    await user.type(newInput, 'NewPassword456!');
    await user.click(screen.getByRole('button', { name: 'Update password' }));

    await waitFor(() => {
      expect(changeFn).toHaveBeenCalledWith(
        { currentPassword: 'OldPassword123!', newPassword: 'NewPassword456!' },
        'test-csrf-token',
      );
    });

    await waitFor(() => {
      expect(screen.getByText('Password updated')).toBeInTheDocument();
    });
  });

  it('shows error toast on failure', async () => {
    const changeFn = vi.fn().mockRejectedValue(new Error('Wrong password'));
    mockUseChangePassword.mockReturnValue(changeFn);

    const { user } = renderWithProviders(<ChangePasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Current password')).toBeInTheDocument();
    });

    const currentInput = screen.getByLabelText('Current password');
    const newInput = screen.getByLabelText('New password');
    await user.type(currentInput, 'WrongPassword!');
    await user.type(newInput, 'NewPassword456!');
    await user.click(screen.getByRole('button', { name: 'Update password' }));

    await waitFor(() => {
      expect(screen.getByText("Couldn't update password")).toBeInTheDocument();
    });
  });
});
