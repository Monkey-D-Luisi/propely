// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { useVoiceInput, type VoiceInputState } from '@/hooks/use-voice-input';
import { useAudioWaveform } from '@/hooks/use-audio-waveform';
import { AudioWaveform } from './AudioWaveform';

export interface VoiceMicButtonProps {
  /** Called when recording stops and audio blob is ready. */
  onAudioReady?: (blob: Blob) => void;
  /** Whether voice processing is in progress (controlled externally). */
  isProcessing?: boolean;
  /** Additional CSS classes. */
  className?: string;
}

/**
 * Microphone button component for voice input in the command bar.
 *
 * States:
 * - Idle: microphone icon
 * - Recording: pulsing red ring animation + waveform
 * - Processing: loading spinner
 * - Permission denied: error message
 */
export function VoiceMicButton({
  onAudioReady,
  isProcessing = false,
  className = '',
}: VoiceMicButtonProps) {
  const t = useTranslations('commandBar.voice');
  const {
    state,
    startRecording,
    stopRecording,
    audioBlob,
    error,
    permissionDenied,
    mediaStream,
  } = useVoiceInput();
  const { analyserData, isActive: waveformActive } =
    useAudioWaveform(mediaStream);

  const effectiveState: VoiceInputState =
    isProcessing ? 'processing' : state;

  // Notify parent when audio blob is ready
  useEffect(() => {
    if (audioBlob && onAudioReady && state === 'processing' && !isProcessing) {
      onAudioReady(audioBlob);
    }
  }, [audioBlob, onAudioReady, state, isProcessing]);

  const handleClick = async () => {
    if (effectiveState === 'recording') {
      stopRecording();
      // Optional haptic feedback
      if (typeof navigator !== 'undefined' && navigator.vibrate) {
        navigator.vibrate(50);
      }
    } else if (effectiveState === 'idle') {
      await startRecording();
      // Optional haptic feedback
      if (typeof navigator !== 'undefined' && navigator.vibrate) {
        navigator.vibrate(50);
      }
    }
  };

  const getAriaLabel = (): string => {
    switch (effectiveState) {
      case 'recording':
        return t('stop');
      case 'processing':
        return t('processing');
      case 'requesting-permission':
        return t('processing');
      default:
        return t('record');
    }
  };

  const isDisabled =
    effectiveState === 'processing' ||
    effectiveState === 'requesting-permission';

  return (
    <div className={`relative inline-flex flex-col items-center ${className}`}>
      <button
        type="button"
        onClick={handleClick}
        disabled={isDisabled}
        aria-label={getAriaLabel()}
        className={`
          relative flex h-10 w-10 min-h-[48px] min-w-[48px] items-center justify-center
          rounded-full transition-all duration-200
          focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2
          disabled:opacity-50 disabled:cursor-not-allowed
          ${effectiveState === 'recording'
            ? 'bg-red-50 text-red-600 ring-2 ring-red-500 animate-pulse'
            : 'bg-slate-100 text-slate-600 hover:bg-slate-200 hover:text-slate-800'
          }
        `}
      >
        {effectiveState === 'processing' ? (
          <LoadingSpinner />
        ) : (
          <MicrophoneIcon isRecording={effectiveState === 'recording'} />
        )}
      </button>

      {/* Waveform visualization during recording */}
      {effectiveState === 'recording' && waveformActive && analyserData && (
        <div className="mt-1">
          <AudioWaveform analyserData={analyserData} />
        </div>
      )}

      {/* Permission denied message */}
      {permissionDenied && (
        <div
          className="absolute top-full mt-2 w-64 rounded-lg bg-red-50 border border-red-200 p-3 text-sm text-red-700 shadow-lg z-50"
          role="alert"
        >
          <p className="font-medium">{t('permissionDenied')}</p>
          <p className="mt-1 text-red-600">{t('permissionGuide')}</p>
        </div>
      )}

      {/* Error message (non-permission) */}
      {error && !permissionDenied && (
        <div
          className="absolute top-full mt-2 w-56 rounded-lg bg-amber-50 border border-amber-200 p-3 text-sm text-amber-700 shadow-lg z-50"
          role="alert"
        >
          <p>{t(error as 'permissionDenied' | 'noAudioDetected')}</p>
        </div>
      )}
    </div>
  );
}

function MicrophoneIcon({ isRecording }: { isRecording: boolean }) {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 24 24"
      fill="currentColor"
      className={`h-5 w-5 ${isRecording ? 'text-red-600' : ''}`}
      aria-hidden="true"
    >
      <path d="M12 14a3 3 0 0 0 3-3V5a3 3 0 0 0-6 0v6a3 3 0 0 0 3 3Z" />
      <path d="M19 11a1 1 0 1 0-2 0 5 5 0 0 1-10 0 1 1 0 1 0-2 0 7 7 0 0 0 6 6.92V20H8a1 1 0 1 0 0 2h8a1 1 0 1 0 0-2h-3v-2.08A7 7 0 0 0 19 11Z" />
    </svg>
  );
}

function LoadingSpinner() {
  return (
    <svg
      className="h-5 w-5 animate-spin text-primary-600"
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
      aria-hidden="true"
    >
      <circle
        className="opacity-25"
        cx="12"
        cy="12"
        r="10"
        stroke="currentColor"
        strokeWidth="4"
      />
      <path
        className="opacity-75"
        fill="currentColor"
        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
      />
    </svg>
  );
}
