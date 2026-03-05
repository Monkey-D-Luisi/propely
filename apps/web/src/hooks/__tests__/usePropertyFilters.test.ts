// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { usePropertyFilters } from '@/hooks/usePropertyFilters';

const mockReplace = vi.fn();
let mockSearchParams = new URLSearchParams();

vi.mock('next/navigation', () => ({
  useSearchParams: () => mockSearchParams,
  useRouter: () => ({ replace: mockReplace }),
  usePathname: () => '/en/properties',
}));

beforeEach(() => {
  vi.resetAllMocks();
  mockSearchParams = new URLSearchParams();
});

describe('usePropertyFilters', () => {
  it('initializes with empty filters when no URL params', () => {
    const { result } = renderHook(() => usePropertyFilters());

    expect(result.current.filters).toEqual({});
    expect(result.current.page).toBe(1);
    expect(result.current.activeFilterCount).toBe(0);
  });

  it('parses URL search params on init', () => {
    mockSearchParams = new URLSearchParams('type=Apartment&minPrice=100000&hasPool=true&page=2');
    const { result } = renderHook(() => usePropertyFilters());

    expect(result.current.filters.type).toBe('Apartment');
    expect(result.current.filters.minPrice).toBe(100000);
    expect(result.current.filters.hasPool).toBe(true);
    expect(result.current.page).toBe(2);
  });

  it('setFilters updates filters and resets page to 1', () => {
    mockSearchParams = new URLSearchParams('page=3');
    const { result } = renderHook(() => usePropertyFilters());

    act(() => {
      result.current.setFilters({ type: 'Villa' });
    });

    expect(result.current.filters.type).toBe('Villa');
    expect(result.current.page).toBe(1);
  });

  it('clearAll resets everything', () => {
    mockSearchParams = new URLSearchParams('type=Apartment&minPrice=100000&page=2');
    const { result } = renderHook(() => usePropertyFilters());

    act(() => {
      result.current.clearAll();
    });

    expect(result.current.filters).toEqual({});
    expect(result.current.page).toBe(1);
  });

  it('activeFilterCount correctly counts active filters', () => {
    mockSearchParams = new URLSearchParams('type=Apartment&minPrice=100000&hasPool=true&search=beach');
    const { result } = renderHook(() => usePropertyFilters());

    expect(result.current.activeFilterCount).toBe(4);
  });

  it('syncs filters to URL via router.replace', async () => {
    const { result } = renderHook(() => usePropertyFilters());

    act(() => {
      result.current.setFilters({ type: 'House', minBedrooms: 3 });
    });

    // Wait for the useEffect to fire
    await vi.waitFor(() => {
      expect(mockReplace).toHaveBeenCalled();
    });

    const lastCall = mockReplace.mock.calls[mockReplace.mock.calls.length - 1];
    const url = lastCall[0] as string;
    expect(url).toContain('type=House');
    expect(url).toContain('minBedrooms=3');
  });

  it('setPage updates page number', () => {
    const { result } = renderHook(() => usePropertyFilters());

    act(() => {
      result.current.setPage(5);
    });

    expect(result.current.page).toBe(5);
  });

  it('parses boolean amenity filters from URL', () => {
    mockSearchParams = new URLSearchParams('hasGarden=true&hasGarage=true&hasElevator=true&hasTerrace=true');
    const { result } = renderHook(() => usePropertyFilters());

    expect(result.current.filters.hasGarden).toBe(true);
    expect(result.current.filters.hasGarage).toBe(true);
    expect(result.current.filters.hasElevator).toBe(true);
    expect(result.current.filters.hasTerrace).toBe(true);
    expect(result.current.activeFilterCount).toBe(4);
  });

  it('parses numeric filters from URL', () => {
    mockSearchParams = new URLSearchParams('minBedrooms=2&minBathrooms=1&minArea=80&maxArea=200');
    const { result } = renderHook(() => usePropertyFilters());

    expect(result.current.filters.minBedrooms).toBe(2);
    expect(result.current.filters.minBathrooms).toBe(1);
    expect(result.current.filters.minArea).toBe(80);
    expect(result.current.filters.maxArea).toBe(200);
  });
});
