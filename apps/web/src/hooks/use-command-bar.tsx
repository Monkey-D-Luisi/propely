// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { createContext, useContext, useState, useEffect, useCallback, useRef, type ReactNode } from 'react';

const MAX_RECENT_COMMANDS = 10;

interface CommandBarContextValue {
  isOpen: boolean;
  open: () => void;
  close: () => void;
  toggle: () => void;
  recentCommands: string[];
  addRecentCommand: (command: string) => void;
  openedWithKeyboard: React.RefObject<boolean>;
  sessionId: string | null;
}

const CommandBarContext = createContext<CommandBarContextValue | null>(null);

export function CommandBarProvider({ children }: { children: ReactNode }) {
  const [isOpen, setIsOpen] = useState(false);
  const [recentCommands, setRecentCommands] = useState<string[]>([]);
  const openedWithKeyboard = useRef(false);
  const [sessionId, setSessionId] = useState<string | null>(null);

  const open = useCallback(() => {
    setIsOpen(true);
    setSessionId((prev) => prev ?? crypto.randomUUID());
  }, []);

  const close = useCallback(() => {
    setIsOpen(false);
    openedWithKeyboard.current = false;
    setSessionId(null);
  }, []);

  const toggle = useCallback(() => {
    setIsOpen((prev) => {
      if (!prev) {
        setSessionId((s) => s ?? crypto.randomUUID());
      }
      return !prev;
    });
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

  return (
    <CommandBarContext.Provider value={{
      isOpen,
      open,
      close,
      toggle,
      recentCommands,
      addRecentCommand,
      openedWithKeyboard,
      sessionId,
    }}>
      {children}
    </CommandBarContext.Provider>
  );
}

export function useCommandBar() {
  const ctx = useContext(CommandBarContext);
  if (!ctx) {
    throw new Error('useCommandBar must be used within a CommandBarProvider');
  }
  return ctx;
}
