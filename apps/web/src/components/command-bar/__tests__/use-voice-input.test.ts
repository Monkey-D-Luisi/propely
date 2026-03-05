// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act, waitFor } from '@testing-library/react';
import { useVoiceInput } from '@/hooks/use-voice-input';

// --- MediaRecorder mock ---

type MediaRecorderEventHandler = ((event: { data: Blob }) => void) | null;

class MockMediaRecorder {
  state: 'inactive' | 'recording' = 'inactive';
  mimeType = 'audio/webm';
  ondataavailable: MediaRecorderEventHandler = null;
  onstop: (() => void) | null = null;
  onerror: (() => void) | null = null;

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  constructor(_stream: MediaStream) {
    // Store for later reference
  }

  start() {
    this.state = 'recording';
  }

  stop() {
    this.state = 'inactive';
    // Simulate data available
    if (this.ondataavailable) {
      this.ondataavailable({ data: new Blob(['audio-data'], { type: 'audio/webm' }) });
    }
    if (this.onstop) {
      this.onstop();
    }
  }
}

// --- getUserMedia mock ---

function createMockStream(): MediaStream {
  return {
    getTracks: () => [{ stop: vi.fn() }],
  } as unknown as MediaStream;
}

let mockGetUserMedia: ReturnType<typeof vi.fn>;

beforeEach(() => {
  vi.useFakeTimers({ shouldAdvanceTime: true });

  mockGetUserMedia = vi.fn().mockResolvedValue(createMockStream());

  vi.stubGlobal('navigator', {
    mediaDevices: {
      getUserMedia: mockGetUserMedia,
    },
    vibrate: vi.fn(),
  });

  vi.stubGlobal('MediaRecorder', MockMediaRecorder);
});

afterEach(() => {
  vi.useRealTimers();
  vi.restoreAllMocks();
});

describe('useVoiceInput', () => {
  it('starts in idle state', () => {
    const { result } = renderHook(() => useVoiceInput());

    expect(result.current.state).toBe('idle');
    expect(result.current.audioBlob).toBeNull();
    expect(result.current.error).toBeNull();
    expect(result.current.permissionDenied).toBe(false);
    expect(result.current.mediaStream).toBeNull();
  });

  it('requests microphone permission on startRecording', async () => {
    const { result } = renderHook(() => useVoiceInput());

    await act(async () => {
      await result.current.startRecording();
    });

    expect(mockGetUserMedia).toHaveBeenCalledWith({ audio: true });
    expect(result.current.state).toBe('recording');
  });

  it('transitions to recording state after permission granted', async () => {
    const { result } = renderHook(() => useVoiceInput());

    await act(async () => {
      await result.current.startRecording();
    });

    expect(result.current.state).toBe('recording');
    expect(result.current.mediaStream).not.toBeNull();
  });

  it('sets permissionDenied when getUserMedia throws NotAllowedError', async () => {
    const permError = new DOMException('Permission denied', 'NotAllowedError');
    mockGetUserMedia.mockRejectedValue(permError);

    const { result } = renderHook(() => useVoiceInput());

    await act(async () => {
      await result.current.startRecording();
    });

    expect(result.current.permissionDenied).toBe(true);
    expect(result.current.error).toBe('permissionDenied');
    expect(result.current.state).toBe('idle');
  });

  it('sets error for non-permission getUserMedia failures', async () => {
    mockGetUserMedia.mockRejectedValue(new Error('No mic found'));

    const { result } = renderHook(() => useVoiceInput());

    await act(async () => {
      await result.current.startRecording();
    });

    expect(result.current.permissionDenied).toBe(false);
    expect(result.current.error).toBe('noAudioDetected');
    expect(result.current.state).toBe('idle');
  });

  it('produces audioBlob and transitions to processing after stopRecording', async () => {
    const { result } = renderHook(() => useVoiceInput());

    await act(async () => {
      await result.current.startRecording();
    });

    // Advance time past the 0.5s minimum
    vi.advanceTimersByTime(1000);

    act(() => {
      result.current.stopRecording();
    });

    await waitFor(() => {
      expect(result.current.state).toBe('processing');
      expect(result.current.audioBlob).toBeInstanceOf(Blob);
    });
  });

  it('rejects recordings shorter than 0.5 seconds', async () => {
    const { result } = renderHook(() => useVoiceInput());

    await act(async () => {
      await result.current.startRecording();
    });

    // Do NOT advance time, so it's < 500ms
    act(() => {
      result.current.stopRecording();
    });

    await waitFor(() => {
      expect(result.current.error).toBe('noAudioDetected');
      expect(result.current.state).toBe('idle');
      expect(result.current.audioBlob).toBeNull();
    });
  });

  it('clears previous error on new startRecording', async () => {
    // First: trigger an error
    mockGetUserMedia.mockRejectedValueOnce(new Error('fail'));

    const { result } = renderHook(() => useVoiceInput());

    await act(async () => {
      await result.current.startRecording();
    });

    expect(result.current.error).toBe('noAudioDetected');

    // Second: succeed
    mockGetUserMedia.mockResolvedValueOnce(createMockStream());

    await act(async () => {
      await result.current.startRecording();
    });

    expect(result.current.error).toBeNull();
    expect(result.current.state).toBe('recording');
  });

  it('stopRecording is a no-op when not recording', async () => {
    const { result } = renderHook(() => useVoiceInput());

    // Should not throw
    act(() => {
      result.current.stopRecording();
    });

    expect(result.current.state).toBe('idle');
  });
});
