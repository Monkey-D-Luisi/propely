// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { CommandInput } from '@/components/command-bar/CommandInput';

describe('CommandInput', () => {
  let onSubmit: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    onSubmit = vi.fn();
  });

  it('renders input with placeholder text', () => {
    renderWithProviders(<CommandInput onSubmit={onSubmit} isLoading={false} />);

    expect(screen.getByPlaceholderText('Type a command...')).toBeInTheDocument();
  });

  it('renders submit button with Execute label', () => {
    renderWithProviders(<CommandInput onSubmit={onSubmit} isLoading={false} />);

    expect(screen.getByRole('button', { name: 'Execute' })).toBeInTheDocument();
  });

  it('disables submit button when input is empty', () => {
    renderWithProviders(<CommandInput onSubmit={onSubmit} isLoading={false} />);

    const button = screen.getByRole('button', { name: 'Execute' });
    expect(button).toBeDisabled();
  });

  it('enables submit button when input has text', async () => {
    const { user } = renderWithProviders(
      <CommandInput onSubmit={onSubmit} isLoading={false} />,
    );

    const input = screen.getByPlaceholderText('Type a command...');
    await user.type(input, 'create a property');

    const button = screen.getByRole('button', { name: 'Execute' });
    expect(button).not.toBeDisabled();
  });

  it('calls onSubmit when clicking the button', async () => {
    const { user } = renderWithProviders(
      <CommandInput onSubmit={onSubmit} isLoading={false} />,
    );

    const input = screen.getByPlaceholderText('Type a command...');
    await user.type(input, 'create a property');
    await user.click(screen.getByRole('button', { name: 'Execute' }));

    expect(onSubmit).toHaveBeenCalledWith('create a property');
  });

  it('calls onSubmit when pressing Enter', async () => {
    const { user } = renderWithProviders(
      <CommandInput onSubmit={onSubmit} isLoading={false} />,
    );

    const input = screen.getByPlaceholderText('Type a command...');
    await user.type(input, 'create a property{Enter}');

    expect(onSubmit).toHaveBeenCalledWith('create a property');
  });

  it('shows loading text when isLoading is true', () => {
    renderWithProviders(<CommandInput onSubmit={onSubmit} isLoading={true} />);

    expect(screen.getByRole('button', { name: 'Processing...' })).toBeInTheDocument();
  });

  it('disables input when isLoading is true', () => {
    renderWithProviders(<CommandInput onSubmit={onSubmit} isLoading={true} />);

    const input = screen.getByPlaceholderText('Type a command...');
    expect(input).toBeDisabled();
  });

  it('does not call onSubmit for whitespace-only input', async () => {
    const { user } = renderWithProviders(
      <CommandInput onSubmit={onSubmit} isLoading={false} />,
    );

    const input = screen.getByPlaceholderText('Type a command...');
    await user.type(input, '   {Enter}');

    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('disables input when disabled prop is true', () => {
    renderWithProviders(
      <CommandInput onSubmit={onSubmit} isLoading={false} disabled />,
    );

    const input = screen.getByPlaceholderText('Type a command...');
    expect(input).toBeDisabled();
  });
});
