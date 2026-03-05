// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback, useRef, useState } from 'react';

export type VoiceInputState =
  | 'idle'
  | 'requesting-permission'
  | 'recording'
  | 'processing';

export interface UseVoiceInputReturn {
  state: VoiceInputState;
  startRecording: () => Promise<void>;
  stopRecording: () => void;
  audioBlob: Blob | null;
  error: string | null;
  permissionDenied: boolean;
  mediaStream: MediaStream | null;
}

/** Minimum recording duration in milliseconds. */
const MIN_RECORDING_DURATION_MS = 500;

/**
 * Hook that manages MediaRecorder lifecycle, audio chunks, and permission state
 * for voice input recording.
 */
export function useVoiceInput(): UseVoiceInputReturn {
  const [state, setState] = useState<VoiceInputState>('idle');
  const [audioBlob, setAudioBlob] = useState<Blob | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [permissionDenied, setPermissionDenied] = useState(false);
  const [mediaStream, setMediaStream] = useState<MediaStream | null>(null);

  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const chunksRef = useRef<Blob[]>([]);
  const startTimeRef = useRef<number>(0);

  const cleanup = useCallback(() => {
    if (mediaRecorderRef.current) {
      // Only stop if still recording
      if (mediaRecorderRef.current.state === 'recording') {
        mediaRecorderRef.current.stop();
      }
      mediaRecorderRef.current = null;
    }
    chunksRef.current = [];
    startTimeRef.current = 0;
  }, []);

  const stopStream = useCallback((stream: MediaStream | null) => {
    if (stream) {
      stream.getTracks().forEach((track) => track.stop());
    }
  }, []);

  const startRecording = useCallback(async () => {
    // Reset previous state
    setError(null);
    setAudioBlob(null);
    setPermissionDenied(false);
    cleanup();

    setState('requesting-permission');

    let stream: MediaStream;
    try {
      stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    } catch (err) {
      const isDenied =
        err instanceof DOMException &&
        (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError');

      if (isDenied) {
        setPermissionDenied(true);
        setError('permissionDenied');
      } else {
        setError('noAudioDetected');
      }
      setState('idle');
      return;
    }

    setMediaStream(stream);

    const recorder = new MediaRecorder(stream);
    mediaRecorderRef.current = recorder;
    chunksRef.current = [];
    startTimeRef.current = Date.now();

    recorder.ondataavailable = (event) => {
      if (event.data.size > 0) {
        chunksRef.current.push(event.data);
      }
    };

    recorder.onstop = () => {
      const duration = Date.now() - startTimeRef.current;

      // Stop all tracks to release mic
      stopStream(stream);
      setMediaStream(null);

      if (duration < MIN_RECORDING_DURATION_MS) {
        setError('noAudioDetected');
        setState('idle');
        chunksRef.current = [];
        return;
      }

      if (chunksRef.current.length === 0) {
        setError('noAudioDetected');
        setState('idle');
        return;
      }

      const blob = new Blob(chunksRef.current, {
        type: recorder.mimeType || 'audio/webm',
      });
      setAudioBlob(blob);
      setState('processing');
      chunksRef.current = [];
    };

    recorder.onerror = () => {
      stopStream(stream);
      setMediaStream(null);
      setError('noAudioDetected');
      setState('idle');
    };

    recorder.start();
    setState('recording');
  }, [cleanup, stopStream]);

  const stopRecording = useCallback(() => {
    if (
      mediaRecorderRef.current &&
      mediaRecorderRef.current.state === 'recording'
    ) {
      mediaRecorderRef.current.stop();
    }
  }, []);

  return {
    state,
    startRecording,
    stopRecording,
    audioBlob,
    error,
    permissionDenied,
    mediaStream,
  };
}
