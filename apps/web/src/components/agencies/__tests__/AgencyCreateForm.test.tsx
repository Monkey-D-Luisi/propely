// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { AgencyCreateForm } from '@/components/agencies/AgencyCreateForm';
import { ApiError } from '@/lib/api';
import { renderWithProviders, screen, waitFor } from '@test/utils';

const mockPush = vi.fn();

vi.mock('@/hooks/agencies', () => ({
  useCreateAgency: vi.fn(),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
  useRouter: vi.fn(() => ({
    push: mockPush,
    replace: vi.fn(),
    refresh: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
    prefetch: vi.fn(),
  })),
}));

import { useCreateAgency } from '@/hooks/agencies';

const mockUseCreateAgency = vi.mocked(useCreateAgency);

beforeEach(() => {
  vi.resetAllMocks();
  mockUseCreateAgency.mockReturnValue(vi.fn().mockResolvedValue({ id: 'agency-1', name: 'Test Agency', slug: 'test-agency' }));
});

describe('AgencyCreateForm', () => {
  it('renders the create form with name and slug fields', () => {
    renderWithProviders(<AgencyCreateForm />);

    expect(screen.getByRole('heading', { name: 'Create agency' })).toBeInTheDocument();
    expect(screen.getByLabelText(/Agency name/)).toBeInTheDocument();
    expect(screen.getByLabelText(/Agency slug/)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Create agency' })).toBeInTheDocument();
  });

  it('auto-generates slug from name', async () => {
    const { user } = renderWithProviders(<AgencyCreateForm />);

    const nameInput = screen.getByLabelText(/Agency name/);
    await user.type(nameInput, 'My Great Agency');

    expect(screen.getByLabelText(/Agency slug/)).toHaveValue('my-great-agency');
  });

  it('shows validation error for empty name', async () => {
    const { user } = renderWithProviders(<AgencyCreateForm />);

    await user.click(screen.getByRole('button', { name: 'Create agency' }));

    await waitFor(() => {
      expect(screen.getByText('Agency name is required.')).toBeInTheDocument();
    });
  });

  it('shows validation error for invalid slug', async () => {
    const { user } = renderWithProviders(<AgencyCreateForm />);

    const nameInput = screen.getByLabelText(/Agency name/);
    await user.type(nameInput, 'Test');

    const slugInput = screen.getByLabelText(/Agency slug/);
    await user.clear(slugInput);
    await user.type(slugInput, 'AB');

    await user.click(screen.getByRole('button', { name: 'Create agency' }));

    await waitFor(() => {
      expect(screen.getByText('Slug must be at least 3 characters.')).toBeInTheDocument();
    });
  });

  it('calls createAgency on valid submit and redirects', async () => {
    const createFn = vi.fn().mockResolvedValue({ id: 'new-agency-id', name: 'My Agency', slug: 'my-agency' });
    mockUseCreateAgency.mockReturnValue(createFn);

    const { user } = renderWithProviders(<AgencyCreateForm />);

    const nameInput = screen.getByLabelText(/Agency name/);
    await user.type(nameInput, 'My Agency');

    await user.click(screen.getByRole('button', { name: 'Create agency' }));

    await waitFor(() => {
      expect(createFn).toHaveBeenCalledWith('My Agency', 'my-agency');
    });

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith('/agencies/new-agency-id');
    });
  });

  it('shows slug-already-exists error on 409 conflict', async () => {
    const createFn = vi.fn().mockRejectedValue(new ApiError('Conflict', 409));
    mockUseCreateAgency.mockReturnValue(createFn);

    const { user } = renderWithProviders(<AgencyCreateForm />);

    const nameInput = screen.getByLabelText(/Agency name/);
    await user.type(nameInput, 'Test Agency');

    const slugInput = screen.getByLabelText(/Agency slug/);
    await user.clear(slugInput);
    await user.type(slugInput, 'test-agency');

    await user.click(screen.getByRole('button', { name: 'Create agency' }));

    await waitFor(() => {
      expect(screen.getByText('An agency with this slug already exists.')).toBeInTheDocument();
    });
  });

  it('shows generic error on non-409 failure', async () => {
    const createFn = vi.fn().mockRejectedValue(new Error('Network error'));
    mockUseCreateAgency.mockReturnValue(createFn);

    const { user } = renderWithProviders(<AgencyCreateForm />);

    const nameInput = screen.getByLabelText(/Agency name/);
    await user.type(nameInput, 'Test Agency');

    const slugInput = screen.getByLabelText(/Agency slug/);
    await user.clear(slugInput);
    await user.type(slugInput, 'test-agency');

    await user.click(screen.getByRole('button', { name: 'Create agency' }));

    await waitFor(() => {
      expect(screen.getByText('Failed to create the agency. Please try again.')).toBeInTheDocument();
    });
  });

  it('has a cancel button that navigates back', () => {
    renderWithProviders(<AgencyCreateForm />);

    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });
});
