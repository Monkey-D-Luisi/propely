// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import type React from 'react';
import { render } from '@testing-library/react';
import { renderWithProviders, screen } from '@test/utils';
import { NextIntlClientProvider } from 'next-intl';
import { ToastProvider } from '@/components/ui/toast';
import messages from '../../../../messages/en.json';
import { VoiceMicButton } from '@/components/command-bar/VoiceMicButton';

// Mock the hooks
vi.mock('@/hooks/use-voice-input', () => ({
  useVoiceInput: vi.fn(),
}));

vi.mock('@/hooks/use-audio-waveform', () => ({
  useAudioWaveform: vi.fn().mockReturnValue({
    analyserData: null,
    isActive: false,
  }),
}));

import { useVoiceInput } from '@/hooks/use-voice-input';
import { useAudioWaveform } from '@/hooks/use-audio-waveform';

const mockUseVoiceInput = vi.mocked(useVoiceInput);
const mockUseAudioWaveform = vi.mocked(useAudioWaveform);

const defaultVoiceInput = {
  state: 'idle' as const,
  startRecording: vi.fn(),
  stopRecording: vi.fn(),
  resetVoice: vi.fn(),
  audioBlob: null,
  error: null,
  permissionDenied: false,
  mediaStream: null,
};

beforeEach(() => {
  vi.resetAllMocks();
  mockUseVoiceInput.mockReturnValue({ ...defaultVoiceInput });
  mockUseAudioWaveform.mockReturnValue({ analyserData: null, isActive: false });
});

describe('VoiceMicButton', () => {
  it('renders microphone button in idle state', () => {
    renderWithProviders(<VoiceMicButton />);

    const button = screen.getByRole('button', { name: /start recording/i });
    expect(button).toBeInTheDocument();
    expect(button).not.toBeDisabled();
  });

  it('calls startRecording when clicked in idle state', async () => {
    const startRecording = vi.fn();
    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      startRecording,
    });

    const { user } = renderWithProviders(<VoiceMicButton />);

    const button = screen.getByRole('button', { name: /start recording/i });
    await user.click(button);

    expect(startRecording).toHaveBeenCalled();
  });

  it('shows stop recording label when recording', () => {
    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      state: 'recording',
    });

    renderWithProviders(<VoiceMicButton />);

    expect(screen.getByRole('button', { name: /stop recording/i })).toBeInTheDocument();
  });

  it('calls stopRecording when clicked while recording', async () => {
    const stopRecording = vi.fn();
    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      state: 'recording',
      stopRecording,
    });

    const { user } = renderWithProviders(<VoiceMicButton />);

    const button = screen.getByRole('button', { name: /stop recording/i });
    await user.click(button);

    expect(stopRecording).toHaveBeenCalled();
  });

  it('disables button when processing', () => {
    renderWithProviders(<VoiceMicButton isProcessing />);

    const button = screen.getByRole('button', { name: /processing voice/i });
    expect(button).toBeDisabled();
  });

  it('shows permission denied alert when microphone access is denied', () => {
    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      permissionDenied: true,
      error: 'permissionDenied',
    });

    renderWithProviders(<VoiceMicButton />);

    expect(screen.getByText('Microphone access required')).toBeInTheDocument();
    expect(
      screen.getByText('Please allow microphone access in your browser settings'),
    ).toBeInTheDocument();
  });

  it('shows error alert for non-permission errors', () => {
    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      error: 'noAudioDetected',
      permissionDenied: false,
    });

    renderWithProviders(<VoiceMicButton />);

    expect(screen.getByText(/no audio detected/i)).toBeInTheDocument();
  });

  it('shows waveform visualization when recording and waveform is active', () => {
    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      state: 'recording',
      mediaStream: {} as MediaStream,
    });
    mockUseAudioWaveform.mockReturnValue({
      analyserData: new Uint8Array([100, 150, 200]),
      isActive: true,
    });

    renderWithProviders(<VoiceMicButton />);

    expect(screen.getByTestId('audio-waveform')).toBeInTheDocument();
  });

  it('does not show waveform when idle', () => {
    renderWithProviders(<VoiceMicButton />);

    expect(screen.queryByTestId('audio-waveform')).not.toBeInTheDocument();
  });

  it('has minimum 48x48 tap target for mobile', () => {
    renderWithProviders(<VoiceMicButton />);

    const button = screen.getByRole('button', { name: /start recording/i });
    expect(button.className).toContain('min-h-[48px]');
    expect(button.className).toContain('min-w-[48px]');
  });

  it('applies pulsing animation when recording', () => {
    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      state: 'recording',
    });

    renderWithProviders(<VoiceMicButton />);

    const button = screen.getByRole('button', { name: /stop recording/i });
    expect(button.className).toContain('animate-pulse');
    expect(button.className).toContain('ring-red-500');
  });

  it('calls onAudioReady exactly once when audioBlob becomes available and state is processing', () => {
    const onAudioReady = vi.fn();
    const resetVoice = vi.fn();
    const blob = new Blob(['audio'], { type: 'audio/webm' });

    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      state: 'processing',
      audioBlob: blob,
      resetVoice,
    });

    renderWithProviders(
      <VoiceMicButton onAudioReady={onAudioReady} />,
    );

    expect(onAudioReady).toHaveBeenCalledTimes(1);
    expect(onAudioReady).toHaveBeenCalledWith(blob);
    expect(resetVoice).toHaveBeenCalledTimes(1);
  });

  it('does NOT call onAudioReady when isProcessing is true (parent already processing)', () => {
    const onAudioReady = vi.fn();
    const blob = new Blob(['audio'], { type: 'audio/webm' });

    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      state: 'processing',
      audioBlob: blob,
    });

    renderWithProviders(
      <VoiceMicButton onAudioReady={onAudioReady} isProcessing />,
    );

    expect(onAudioReady).not.toHaveBeenCalled();
  });

  it('does NOT re-call onAudioReady after isProcessing toggles if blob was cleared', () => {
    const onAudioReady = vi.fn();
    const resetVoice = vi.fn();
    const blob = new Blob(['audio'], { type: 'audio/webm' });

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <NextIntlClientProvider locale="en" messages={messages}>
        <ToastProvider>{children}</ToastProvider>
      </NextIntlClientProvider>
    );

    // Initial render: blob ready, not processing → should hand off
    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      state: 'processing',
      audioBlob: blob,
      resetVoice,
    });

    const { rerender } = render(
      <VoiceMicButton onAudioReady={onAudioReady} isProcessing={false} />,
      { wrapper },
    );

    expect(onAudioReady).toHaveBeenCalledTimes(1);
    onAudioReady.mockClear();

    // Parent starts processing: isProcessing becomes true, blob cleared by resetVoice
    mockUseVoiceInput.mockReturnValue({
      ...defaultVoiceInput,
      state: 'idle',
      audioBlob: null,
      resetVoice,
    });

    rerender(
      <VoiceMicButton onAudioReady={onAudioReady} isProcessing />,
    );

    expect(onAudioReady).not.toHaveBeenCalled();

    // Parent finishes processing: isProcessing goes back to false
    rerender(
      <VoiceMicButton onAudioReady={onAudioReady} isProcessing={false} />,
    );

    expect(onAudioReady).not.toHaveBeenCalled();
  });
});
