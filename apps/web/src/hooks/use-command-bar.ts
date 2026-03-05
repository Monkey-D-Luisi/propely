// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback, useRef } from 'react';

const MAX_RECENT_COMMANDS = 10;

export function useCommandBar() {
  const [isOpen, setIsOpen] = useState(false);
  const [recentCommands, setRecentCommands] = useState<string[]>([]);
  const openedWithKeyboard = useRef(false);

  const open = useCallback(() => {
    setIsOpen(true);
  }, []);

  const close = useCallback(() => {
    setIsOpen(false);
    openedWithKeyboard.current = false;
  }, []);

  const toggle = useCallback(() => {
    setIsOpen((prev) => !prev);
  }, []);

  const addRecentCommand = useCallback((command: string) => {
    setRecentCommands((prev) => {
      const filtered = prev.filter((c) => c !== command);
      return [command, ...filtered].slice(0, MAX_RECENT_COMMANDS);
    });
  }, []);

  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        openedWithKeyboard.current = true;
        toggle();
      }
    }

    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [toggle]);

  return {
    isOpen,
    open,
    close,
    toggle,
    recentCommands,
    addRecentCommand,
    openedWithKeyboard,
  };
}
