// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { CommandBar } from '@/components/command-bar/CommandBar';

// Mock the hooks
vi.mock('@/hooks/use-command-bar', () => ({
  useCommandBar: vi.fn(),
}));

vi.mock('@/hooks/use-execute-action', () => ({
  useExecuteAction: vi.fn(),
}));

vi.mock('@/hooks/use-execute-voice-action', () => ({
  useExecuteVoiceAction: vi.fn(),
}));

import { useCommandBar } from '@/hooks/use-command-bar';
import { useExecuteAction } from '@/hooks/use-execute-action';
import { useExecuteVoiceAction } from '@/hooks/use-execute-voice-action';

const mockUseCommandBar = vi.mocked(useCommandBar);
const mockUseExecuteAction = vi.mocked(useExecuteAction);
const mockUseExecuteVoiceAction = vi.mocked(useExecuteVoiceAction);

const defaultCommandBarState = {
  isOpen: true,
  open: vi.fn(),
  close: vi.fn(),
  toggle: vi.fn(),
  recentCommands: [] as string[],
  addRecentCommand: vi.fn(),
  openedWithKeyboard: { current: false },
};

const defaultExecuteActionState = {
  execute: vi.fn().mockResolvedValue(null),
  confirm: vi.fn().mockResolvedValue(null),
  isLoading: false,
  result: null,
  error: null,
  reset: vi.fn(),
};

const defaultExecuteVoiceActionState = {
  executeVoice: vi.fn().mockResolvedValue(null),
  isLoading: false,
};

beforeEach(() => {
  vi.resetAllMocks();
  mockUseCommandBar.mockReturnValue(defaultCommandBarState);
  mockUseExecuteAction.mockReturnValue(defaultExecuteActionState);
  mockUseExecuteVoiceAction.mockReturnValue(defaultExecuteVoiceActionState);
});

describe('CommandBar', () => {
  it('renders nothing when not open', () => {
    mockUseCommandBar.mockReturnValue({
      ...defaultCommandBarState,
      isOpen: false,
    });

    renderWithProviders(<CommandBar />);
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('renders dialog when open', () => {
    renderWithProviders(<CommandBar />);

    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });

  it('renders input with placeholder', () => {
    renderWithProviders(<CommandBar />);

    expect(screen.getByPlaceholderText('Type a command...')).toBeInTheDocument();
  });

  it('renders close button', () => {
    renderWithProviders(<CommandBar />);

    expect(screen.getByRole('button', { name: 'Close' })).toBeInTheDocument();
  });

  it('renders keyboard shortcut hint', () => {
    renderWithProviders(<CommandBar />);

    expect(screen.getByText('Ctrl+K')).toBeInTheDocument();
  });

  it('calls close when close button is clicked', async () => {
    const closeFn = vi.fn();
    mockUseCommandBar.mockReturnValue({
      ...defaultCommandBarState,
      close: closeFn,
    });

    const { user } = renderWithProviders(<CommandBar />);

    await user.click(screen.getByRole('button', { name: 'Close' }));
    expect(closeFn).toHaveBeenCalled();
  });

  it('executes command when submit is triggered', async () => {
    const executeFn = vi.fn().mockResolvedValue({ message: 'Done' });
    const addRecentCommand = vi.fn();
    mockUseCommandBar.mockReturnValue({
      ...defaultCommandBarState,
      addRecentCommand,
    });
    mockUseExecuteAction.mockReturnValue({
      ...defaultExecuteActionState,
      execute: executeFn,
    });

    const { user } = renderWithProviders(<CommandBar />);

    const input = screen.getByPlaceholderText('Type a command...');
    await user.type(input, 'create a property{Enter}');

    expect(executeFn).toHaveBeenCalledWith('create a property');
    expect(addRecentCommand).toHaveBeenCalledWith('create a property');
  });

  it('shows loading state while executing', () => {
    mockUseExecuteAction.mockReturnValue({
      ...defaultExecuteActionState,
      isLoading: true,
    });

    renderWithProviders(<CommandBar />);

    // Both CommandInput button and CommandResult show the loading text
    expect(screen.getAllByText('Processing...').length).toBeGreaterThanOrEqual(1);
    expect(screen.getByRole('status')).toBeInTheDocument();
  });

  it('shows success result after execution', () => {
    mockUseExecuteAction.mockReturnValue({
      ...defaultExecuteActionState,
      result: { message: 'Property created successfully' },
    });

    renderWithProviders(<CommandBar />);

    expect(screen.getByText('Property created successfully')).toBeInTheDocument();
    expect(screen.getByText('Done')).toBeInTheDocument();
  });

  it('shows error result with retry button', () => {
    mockUseExecuteAction.mockReturnValue({
      ...defaultExecuteActionState,
      error: 'Network failed',
    });

    renderWithProviders(<CommandBar />);

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('Network failed')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Try again' })).toBeInTheDocument();
  });

  it('shows recent commands when available and no result', () => {
    mockUseCommandBar.mockReturnValue({
      ...defaultCommandBarState,
      recentCommands: ['create property', 'list leads'],
    });

    renderWithProviders(<CommandBar />);

    expect(screen.getByText('Recent commands')).toBeInTheDocument();
    expect(screen.getByText('create property')).toBeInTheDocument();
    expect(screen.getByText('list leads')).toBeInTheDocument();
  });

  it('shows confirmation panel when result needs confirmation', () => {
    mockUseExecuteAction.mockReturnValue({
      ...defaultExecuteActionState,
      result: {
        message: 'Create property at 123 Main St?',
        needsConfirmation: true,
        confirmationId: 'conf-123',
        extractedParams: { address: '123 Main St', price: '500000' },
      },
    });

    renderWithProviders(<CommandBar />);

    expect(screen.getByText('Please confirm')).toBeInTheDocument();
    expect(screen.getByText('Create property at 123 Main St?')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Confirm' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  it('calls confirm when confirmation button is clicked', async () => {
    const confirmFn = vi.fn().mockResolvedValue(null);
    mockUseExecuteAction.mockReturnValue({
      ...defaultExecuteActionState,
      result: {
        message: 'Create property?',
        needsConfirmation: true,
        confirmationId: 'conf-123',
      },
      confirm: confirmFn,
    });

    const { user } = renderWithProviders(<CommandBar />);

    await user.click(screen.getByRole('button', { name: 'Confirm' }));
    expect(confirmFn).toHaveBeenCalledWith('conf-123');
  });

  it('calls reset when cancel is clicked during confirmation', async () => {
    const resetFn = vi.fn();
    mockUseExecuteAction.mockReturnValue({
      ...defaultExecuteActionState,
      result: {
        message: 'Create property?',
        needsConfirmation: true,
        confirmationId: 'conf-123',
      },
      reset: resetFn,
    });

    const { user } = renderWithProviders(<CommandBar />);

    await user.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(resetFn).toHaveBeenCalled();
  });
});
