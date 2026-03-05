// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { CommandHistory } from '@/components/command-bar/CommandHistory';

describe('CommandHistory', () => {
  it('renders recent commands heading', () => {
    renderWithProviders(
      <CommandHistory commands={[]} onSelect={vi.fn()} />,
    );

    expect(screen.getByText('Recent commands')).toBeInTheDocument();
  });

  it('renders empty state message when no commands', () => {
    renderWithProviders(
      <CommandHistory commands={[]} onSelect={vi.fn()} />,
    );

    expect(screen.getByText('No recent commands')).toBeInTheDocument();
  });

  it('renders list of recent commands', () => {
    const commands = ['create property', 'list leads', 'schedule appointment'];
    renderWithProviders(
      <CommandHistory commands={commands} onSelect={vi.fn()} />,
    );

    expect(screen.getByText('create property')).toBeInTheDocument();
    expect(screen.getByText('list leads')).toBeInTheDocument();
    expect(screen.getByText('schedule appointment')).toBeInTheDocument();
  });

  it('calls onSelect when a command is clicked', async () => {
    const onSelect = vi.fn();
    const commands = ['create property', 'list leads'];
    const { user } = renderWithProviders(
      <CommandHistory commands={commands} onSelect={onSelect} />,
    );

    await user.click(screen.getByText('create property'));
    expect(onSelect).toHaveBeenCalledWith('create property');
  });

  it('renders a listbox with options', () => {
    const commands = ['create property'];
    renderWithProviders(
      <CommandHistory commands={commands} onSelect={vi.fn()} />,
    );

    expect(screen.getByRole('listbox')).toBeInTheDocument();
    expect(screen.getByRole('option')).toBeInTheDocument();
  });
});
