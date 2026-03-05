// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback, useEffect, useRef, useState } from 'react';

export interface UseAudioWaveformReturn {
  /** Frequency data array (0-255 values) updated per animation frame. */
  analyserData: Uint8Array | null;
  /** Whether the waveform is actively receiving audio data. */
  isActive: boolean;
}

/**
 * Hook that connects a MediaStream to the Web Audio API AnalyserNode
 * and returns live frequency data for visualization.
 *
 * @param mediaStream - The active MediaStream from getUserMedia (or null when not recording).
 */
export function useAudioWaveform(
  mediaStream: MediaStream | null,
): UseAudioWaveformReturn {
  const [analyserData, setAnalyserData] = useState<Uint8Array | null>(null);
  const [isActive, setIsActive] = useState(false);

  const audioContextRef = useRef<AudioContext | null>(null);
  const analyserRef = useRef<AnalyserNode | null>(null);
  const sourceRef = useRef<MediaStreamAudioSourceNode | null>(null);
  const rafIdRef = useRef<number | null>(null);

  const cleanup = useCallback(() => {
    if (rafIdRef.current !== null) {
      cancelAnimationFrame(rafIdRef.current);
      rafIdRef.current = null;
    }
    if (sourceRef.current) {
      sourceRef.current.disconnect();
      sourceRef.current = null;
    }
    if (analyserRef.current) {
      analyserRef.current.disconnect();
      analyserRef.current = null;
    }
    if (audioContextRef.current) {
      audioContextRef.current.close().catch(() => {
        // Ignore close errors
      });
      audioContextRef.current = null;
    }
    setAnalyserData(null);
    setIsActive(false);
  }, []);

  useEffect(() => {
    if (!mediaStream) {
      cleanup();
      return;
    }

    // Create AudioContext and AnalyserNode
    const audioContext = new AudioContext();
    audioContextRef.current = audioContext;

    const analyser = audioContext.createAnalyser();
    analyser.fftSize = 64;
    analyser.smoothingTimeConstant = 0.8;
    analyserRef.current = analyser;

    const source = audioContext.createMediaStreamSource(mediaStream);
    source.connect(analyser);
    sourceRef.current = source;

    const bufferLength = analyser.frequencyBinCount;
    const dataArray = new Uint8Array(bufferLength);

    setIsActive(true);

    const tick = () => {
      analyser.getByteFrequencyData(dataArray);
      // Create a new Uint8Array copy so React detects the state change
      setAnalyserData(new Uint8Array(dataArray));
      rafIdRef.current = requestAnimationFrame(tick);
    };

    rafIdRef.current = requestAnimationFrame(tick);

    return cleanup;
  }, [mediaStream, cleanup]);

  return { analyserData, isActive };
}
