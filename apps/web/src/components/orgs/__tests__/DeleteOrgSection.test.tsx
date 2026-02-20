// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { DeleteOrgSection } from '@/components/orgs/DeleteOrgSection';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { useRouter } from 'next/navigation';
import { ApiError } from '@/lib/api';

vi.mock('@/hooks/orgs', () => ({
  useDeleteOrg: vi.fn(),
}));

import { useDeleteOrg } from '@/hooks/orgs';

const mockUseDeleteOrg = vi.mocked(useDeleteOrg);
const mockRouter = vi.mocked(useRouter);

const ORG_ID = 'test-org-id';
const ORG_NAME = 'My Test Org';

beforeEach(() => {
  vi.resetAllMocks();
  mockUseDeleteOrg.mockReturnValue(vi.fn().mockResolvedValue(undefined));
  mockRouter.mockReturnValue({
    push: vi.fn(),
    replace: vi.fn(),
    refresh: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
    prefetch: vi.fn(),
  });
});

describe('DeleteOrgSection', () => {
  it('renders the danger zone section with delete button', () => {
    renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    expect(screen.getByText('Danger zone')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Delete organization' })).toBeInTheDocument();
  });

  it('opens confirmation dialog when delete button is clicked', async () => {
    const { user } = renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    await user.click(screen.getByRole('button', { name: 'Delete organization' }));
    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('Delete organization?')).toBeInTheDocument();
  });

  it('closes dialog when cancel is clicked', async () => {
    const { user } = renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    await user.click(screen.getByRole('button', { name: 'Delete organization' }));
    await user.click(screen.getByRole('button', { name: 'Cancel' }));
    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  it('disables confirm button when org name is not typed', async () => {
    const { user } = renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    await user.click(screen.getByRole('button', { name: 'Delete organization' }));

    const buttons = screen.getAllByRole('button', { name: 'Delete organization' });
    const confirmButton = buttons[buttons.length - 1];
    expect(confirmButton).toBeDisabled();
  });

  it('enables confirm button when org name matches', async () => {
    const { user } = renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    await user.click(screen.getByRole('button', { name: 'Delete organization' }));
    await user.type(screen.getByLabelText(/Type .* to confirm/), ORG_NAME);

    const buttons = screen.getAllByRole('button', { name: 'Delete organization' });
    const confirmButton = buttons[buttons.length - 1];
    expect(confirmButton).toBeEnabled();
  });

  it('calls deleteOrg and shows success toast on confirm', async () => {
    const deleteFn = vi.fn().mockResolvedValue(undefined);
    mockUseDeleteOrg.mockReturnValue(deleteFn);
    const pushFn = vi.fn();
    mockRouter.mockReturnValue({
      push: pushFn,
      replace: vi.fn(),
      refresh: vi.fn(),
      back: vi.fn(),
      forward: vi.fn(),
      prefetch: vi.fn(),
    });

    const { user } = renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    await user.click(screen.getByRole('button', { name: 'Delete organization' }));
    await user.type(screen.getByLabelText(/Type .* to confirm/), ORG_NAME);

    const buttons = screen.getAllByRole('button', { name: 'Delete organization' });
    await user.click(buttons[buttons.length - 1]);

    await waitFor(() => {
      expect(deleteFn).toHaveBeenCalled();
    });
    await waitFor(() => {
      expect(screen.getByText('Organization deleted')).toBeInTheDocument();
    });
    expect(pushFn).toHaveBeenCalledWith('/orgs/mine');
  });

  it('shows generic error toast on API failure', async () => {
    const deleteFn = vi.fn().mockRejectedValue(new Error('Network error'));
    mockUseDeleteOrg.mockReturnValue(deleteFn);

    const { user } = renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    await user.click(screen.getByRole('button', { name: 'Delete organization' }));
    await user.type(screen.getByLabelText(/Type .* to confirm/), ORG_NAME);

    const buttons = screen.getAllByRole('button', { name: 'Delete organization' });
    await user.click(buttons[buttons.length - 1]);

    await waitFor(() => {
      expect(screen.getByText("Couldn't delete organization")).toBeInTheDocument();
    });
    await waitFor(() => {
      expect(screen.getByText('Please try again in a few moments.')).toBeInTheDocument();
    });
  });

  it('shows forbidden error toast on 403 response', async () => {
    const deleteFn = vi.fn().mockRejectedValue(new ApiError('Forbidden', 403));
    mockUseDeleteOrg.mockReturnValue(deleteFn);

    const { user } = renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    await user.click(screen.getByRole('button', { name: 'Delete organization' }));
    await user.type(screen.getByLabelText(/Type .* to confirm/), ORG_NAME);

    const buttons = screen.getAllByRole('button', { name: 'Delete organization' });
    await user.click(buttons[buttons.length - 1]);

    await waitFor(() => {
      expect(screen.getByText('Only organization owners can delete the organization.')).toBeInTheDocument();
    });
  });

  it('resets confirm text when dialog is closed and reopened', async () => {
    const { user } = renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    await user.click(screen.getByRole('button', { name: 'Delete organization' }));
    await user.type(screen.getByLabelText(/Type .* to confirm/), 'partial');
    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Delete organization' }));
    const input = screen.getByLabelText(/Type .* to confirm/) as HTMLInputElement;
    expect(input.value).toBe('');
  });

  it('closes dialog after successful deletion', async () => {
    const deleteFn = vi.fn().mockResolvedValue(undefined);
    mockUseDeleteOrg.mockReturnValue(deleteFn);

    const { user } = renderWithProviders(
      <DeleteOrgSection orgId={ORG_ID} orgName={ORG_NAME} />,
    );
    await user.click(screen.getByRole('button', { name: 'Delete organization' }));
    await user.type(screen.getByLabelText(/Type .* to confirm/), ORG_NAME);

    const buttons = screen.getAllByRole('button', { name: 'Delete organization' });
    await user.click(buttons[buttons.length - 1]);

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });
});
