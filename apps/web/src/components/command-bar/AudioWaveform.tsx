// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useRef } from 'react';

export interface AudioWaveformProps {
  /** Frequency data from AnalyserNode (Uint8Array of 0-255 values). */
  analyserData: Uint8Array;
  /** Width of the canvas in pixels. */
  width?: number;
  /** Height of the canvas in pixels. */
  height?: number;
  /** Color of the waveform bars. */
  barColor?: string;
  /** Additional CSS classes. */
  className?: string;
}

/**
 * Canvas-based audio waveform visualization component.
 * Renders frequency data as vertical bars in a compact inline display.
 */
export function AudioWaveform({
  analyserData,
  width = 80,
  height = 24,
  barColor = '#dc2626', // red-600
  className = '',
}: AudioWaveformProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const dpr = typeof window !== 'undefined' ? window.devicePixelRatio || 1 : 1;
    canvas.width = width * dpr;
    canvas.height = height * dpr;
    ctx.scale(dpr, dpr);

    // Clear
    ctx.clearRect(0, 0, width, height);

    const barCount = Math.min(analyserData.length, 16);
    const gap = 2;
    const barWidth = (width - gap * (barCount - 1)) / barCount;
    const minBarHeight = 2;

    ctx.fillStyle = barColor;

    for (let i = 0; i < barCount; i++) {
      const value = analyserData[i] / 255;
      const barHeight = Math.max(value * height, minBarHeight);
      const x = i * (barWidth + gap);
      const y = (height - barHeight) / 2;

      // Rounded bar caps
      const radius = Math.min(barWidth / 2, 2);
      ctx.beginPath();
      ctx.roundRect(x, y, barWidth, barHeight, radius);
      ctx.fill();
    }
  }, [analyserData, width, height, barColor]);

  return (
    <canvas
      ref={canvasRef}
      width={width}
      height={height}
      className={`block ${className}`}
      style={{ width: `${width}px`, height: `${height}px` }}
      aria-hidden="true"
      data-testid="audio-waveform"
    />
  );
}
