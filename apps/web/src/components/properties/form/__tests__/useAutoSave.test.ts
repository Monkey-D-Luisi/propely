// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { renderHook, act } from '@testing-library/react';
import { useAutoSave } from '@/hooks/useAutoSave';

describe('useAutoSave', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('saves data to localStorage', () => {
    const data = { title: 'Test Property' };
    const { result } = renderHook(() => useAutoSave(data));

    act(() => {
      result.current.save();
    });

    const stored = localStorage.getItem('propely-property-draft');
    expect(stored).toBe(JSON.stringify(data));
    expect(result.current.lastSaved).not.toBeNull();
  });

  it('loads data from localStorage', () => {
    const data = { title: 'Test Property' };
    localStorage.setItem('propely-property-draft', JSON.stringify(data));

    const { result } = renderHook(() => useAutoSave({}));

    const loaded = result.current.load();
    expect(loaded).toEqual(data);
  });

  it('clears data from localStorage', () => {
    localStorage.setItem('propely-property-draft', JSON.stringify({ title: 'Test' }));

    const { result } = renderHook(() => useAutoSave({}));

    act(() => {
      result.current.clear();
    });

    expect(localStorage.getItem('propely-property-draft')).toBeNull();
  });

  it('returns null when no saved data exists', () => {
    const { result } = renderHook(() => useAutoSave({}));
    expect(result.current.load()).toBeNull();
  });
});
