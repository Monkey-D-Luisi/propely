// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useCallback, useEffect, useRef } from 'react';

const AUTOSAVE_KEY = 'propely-property-draft';
const AUTOSAVE_INTERVAL = 30000;

export function useAutoSave(data: Record<string, unknown>, enabled = true) {
  const [lastSaved, setLastSaved] = useState<Date | null>(null);
  const dataRef = useRef(data);

  useEffect(() => {
    dataRef.current = data;
  });

  const save = useCallback(() => {
    try {
      localStorage.setItem(AUTOSAVE_KEY, JSON.stringify(dataRef.current));
      setLastSaved(new Date());
    } catch {
      // Silently fail if localStorage is full
    }
  }, []);

  const load = useCallback((): Record<string, unknown> | null => {
    try {
      const saved = localStorage.getItem(AUTOSAVE_KEY);
      return saved ? JSON.parse(saved) : null;
    } catch {
      return null;
    }
  }, []);

  const clear = useCallback(() => {
    try {
      localStorage.removeItem(AUTOSAVE_KEY);
      setLastSaved(null);
    } catch {
      // Silently fail
    }
  }, []);

  useEffect(() => {
    if (!enabled) return;
    const timer = setInterval(save, AUTOSAVE_INTERVAL);
    return () => clearInterval(timer);
  }, [enabled, save]);

  return { save, load, clear, lastSaved };
}
