// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { AudioWaveform } from '@/components/command-bar/AudioWaveform';

// Mock canvas context
const mockFillRect = vi.fn();
const mockClearRect = vi.fn();
const mockBeginPath = vi.fn();
const mockFill = vi.fn();
const mockScale = vi.fn();
const mockRoundRect = vi.fn();

beforeEach(() => {
  vi.resetAllMocks();

  // Mock HTMLCanvasElement.getContext
  HTMLCanvasElement.prototype.getContext = vi.fn().mockReturnValue({
    fillRect: mockFillRect,
    clearRect: mockClearRect,
    beginPath: mockBeginPath,
    fill: mockFill,
    scale: mockScale,
    roundRect: mockRoundRect,
    fillStyle: '',
  });
});

describe('AudioWaveform', () => {
  it('renders a canvas element', () => {
    const data = new Uint8Array([100, 150, 200, 50]);

    renderWithProviders(<AudioWaveform analyserData={data} />);

    const canvas = screen.getByTestId('audio-waveform');
    expect(canvas).toBeInTheDocument();
    expect(canvas.tagName).toBe('CANVAS');
  });

  it('has aria-hidden true for accessibility', () => {
    const data = new Uint8Array([100]);

    renderWithProviders(<AudioWaveform analyserData={data} />);

    const canvas = screen.getByTestId('audio-waveform');
    expect(canvas).toHaveAttribute('aria-hidden', 'true');
  });

  it('applies default dimensions', () => {
    const data = new Uint8Array([100]);

    renderWithProviders(<AudioWaveform analyserData={data} />);

    const canvas = screen.getByTestId('audio-waveform');
    expect(canvas.style.width).toBe('80px');
    expect(canvas.style.height).toBe('24px');
  });

  it('applies custom dimensions', () => {
    const data = new Uint8Array([100]);

    renderWithProviders(
      <AudioWaveform analyserData={data} width={120} height={32} />,
    );

    const canvas = screen.getByTestId('audio-waveform');
    expect(canvas.style.width).toBe('120px');
    expect(canvas.style.height).toBe('32px');
  });

  it('applies custom className', () => {
    const data = new Uint8Array([100]);

    renderWithProviders(
      <AudioWaveform analyserData={data} className="my-custom-class" />,
    );

    const canvas = screen.getByTestId('audio-waveform');
    expect(canvas.className).toContain('my-custom-class');
  });

  it('calls getContext on the canvas', () => {
    const data = new Uint8Array([100, 200]);

    renderWithProviders(<AudioWaveform analyserData={data} />);

    expect(HTMLCanvasElement.prototype.getContext).toHaveBeenCalledWith('2d');
  });

  it('clears the canvas before drawing', () => {
    const data = new Uint8Array([100]);

    renderWithProviders(<AudioWaveform analyserData={data} />);

    expect(mockClearRect).toHaveBeenCalled();
  });

  it('draws bars for each frequency data point', () => {
    const data = new Uint8Array([100, 150, 200, 50]);

    renderWithProviders(<AudioWaveform analyserData={data} />);

    // Should call beginPath and fill for each bar (up to 16, we have 4)
    expect(mockBeginPath).toHaveBeenCalledTimes(4);
    expect(mockFill).toHaveBeenCalledTimes(4);
  });
});
