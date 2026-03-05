// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { renderWithProviders, screen } from '@test/utils';
import { ViewToggle } from '../ViewToggle';

describe('ViewToggle', () => {
  it('renders both view buttons', () => {
    renderWithProviders(<ViewToggle view="table" onViewChange={vi.fn()} />);
    const buttons = screen.getAllByRole('radio');
    expect(buttons).toHaveLength(2);
  });

  it('marks table view as checked when active', () => {
    renderWithProviders(<ViewToggle view="table" onViewChange={vi.fn()} />);
    const buttons = screen.getAllByRole('radio');
    expect(buttons[0]).toHaveAttribute('aria-checked', 'true');
    expect(buttons[1]).toHaveAttribute('aria-checked', 'false');
  });

  it('marks card view as checked when active', () => {
    renderWithProviders(<ViewToggle view="card" onViewChange={vi.fn()} />);
    const buttons = screen.getAllByRole('radio');
    expect(buttons[0]).toHaveAttribute('aria-checked', 'false');
    expect(buttons[1]).toHaveAttribute('aria-checked', 'true');
  });

  it('calls onViewChange with "table" when table button clicked', async () => {
    const onChange = vi.fn();
    const { user } = renderWithProviders(<ViewToggle view="card" onViewChange={onChange} />);
    const buttons = screen.getAllByRole('radio');
    await user.click(buttons[0]);
    expect(onChange).toHaveBeenCalledWith('table');
  });

  it('calls onViewChange with "card" when card button clicked', async () => {
    const onChange = vi.fn();
    const { user } = renderWithProviders(<ViewToggle view="table" onViewChange={onChange} />);
    const buttons = screen.getAllByRole('radio');
    await user.click(buttons[1]);
    expect(onChange).toHaveBeenCalledWith('card');
  });

  it('applies active styles to selected view', () => {
    renderWithProviders(<ViewToggle view="table" onViewChange={vi.fn()} />);
    const buttons = screen.getAllByRole('radio');
    expect(buttons[0].className).toContain('bg-primary-600');
    expect(buttons[1].className).not.toContain('bg-primary-600');
  });
});
