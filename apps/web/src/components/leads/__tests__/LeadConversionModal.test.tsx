// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { LeadConversionModal } from '../LeadConversionModal';

vi.mock('@/components/ui/dialog-overlay', () => ({
  DialogOverlay: ({ children }: { children: React.ReactNode }) => <div data-testid="dialog-overlay">{children}</div>,
}));

describe('LeadConversionModal', () => {
  it('renders with title and description', () => {
    renderWithProviders(
      <LeadConversionModal leadId="lead-1" onConvert={vi.fn()} onClose={vi.fn()} />,
    );
    expect(screen.getByText('Convert Lead to Contact')).toBeInTheDocument();
    expect(screen.getByText(/This will create a contact/)).toBeInTheDocument();
  });

  it('renders role selector with all roles', () => {
    renderWithProviders(
      <LeadConversionModal leadId="lead-1" onConvert={vi.fn()} onClose={vi.fn()} />,
    );
    const select = screen.getByLabelText('Contact Role');
    expect(select).toBeInTheDocument();
    expect(select).toHaveValue('Buyer');
  });

  it('renders notes textarea', () => {
    renderWithProviders(
      <LeadConversionModal leadId="lead-1" onConvert={vi.fn()} onClose={vi.fn()} />,
    );
    expect(screen.getByLabelText('Notes')).toBeInTheDocument();
  });

  it('renders cancel and convert buttons', () => {
    renderWithProviders(
      <LeadConversionModal leadId="lead-1" onConvert={vi.fn()} onClose={vi.fn()} />,
    );
    expect(screen.getByText('Cancel')).toBeInTheDocument();
    expect(screen.getByText('Convert')).toBeInTheDocument();
  });

  it('calls onClose when cancel is clicked', async () => {
    const onClose = vi.fn();
    const { user } = renderWithProviders(
      <LeadConversionModal leadId="lead-1" onConvert={vi.fn()} onClose={onClose} />,
    );
    await user.click(screen.getByText('Cancel'));
    expect(onClose).toHaveBeenCalled();
  });
});
