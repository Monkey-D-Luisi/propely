// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { ContactsTable } from '../ContactsTable';
import type { ContactListItem } from '@/hooks/useContacts';

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; className?: string }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

const mockItems: ContactListItem[] = [
  {
    id: '00000000-0000-0000-0000-000000000001',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john@example.com',
    phone: '+34 600 000 001',
    company: 'ACME Corp',
    roles: ['Buyer', 'Seller'],
    createdAtUtc: '2026-01-15T10:00:00Z',
  },
  {
    id: '00000000-0000-0000-0000-000000000002',
    firstName: 'Jane',
    lastName: 'Smith',
    email: 'jane@example.com',
    phone: null,
    company: null,
    roles: ['Tenant'],
    createdAtUtc: '2026-02-20T14:30:00Z',
  },
];

describe('ContactsTable', () => {
  it('renders table headers', () => {
    renderWithProviders(
      <ContactsTable items={mockItems} isLoading={false} onDelete={vi.fn()} />,
    );
    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Email')).toBeInTheDocument();
    expect(screen.getByText('Phone')).toBeInTheDocument();
    expect(screen.getByText('Roles')).toBeInTheDocument();
    expect(screen.getByText('Created')).toBeInTheDocument();
  });

  it('renders contact rows with data', () => {
    renderWithProviders(
      <ContactsTable items={mockItems} isLoading={false} onDelete={vi.fn()} />,
    );
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('john@example.com')).toBeInTheDocument();
    expect(screen.getByText('+34 600 000 001')).toBeInTheDocument();
    expect(screen.getByText('ACME Corp')).toBeInTheDocument();
    expect(screen.getByText('Jane Smith')).toBeInTheDocument();
  });

  it('renders role badges for each contact', () => {
    renderWithProviders(
      <ContactsTable items={mockItems} isLoading={false} onDelete={vi.fn()} />,
    );
    expect(screen.getByTestId('role-badge-Buyer')).toBeInTheDocument();
    expect(screen.getByTestId('role-badge-Seller')).toBeInTheDocument();
    expect(screen.getByTestId('role-badge-Tenant')).toBeInTheDocument();
  });

  it('calls onDelete when delete button is clicked', async () => {
    const onDelete = vi.fn();
    const { user } = renderWithProviders(
      <ContactsTable items={mockItems} isLoading={false} onDelete={onDelete} />,
    );
    const deleteButtons = screen.getAllByLabelText('Delete');
    await user.click(deleteButtons[0]);
    expect(onDelete).toHaveBeenCalledWith('00000000-0000-0000-0000-000000000001');
  });

  it('renders skeleton when loading', () => {
    renderWithProviders(
      <ContactsTable items={[]} isLoading={true} onDelete={vi.fn()} />,
    );
    expect(screen.queryByText('John Doe')).not.toBeInTheDocument();
  });
});
