// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { Command } from 'cmdk';
import { useTranslations } from 'next-intl';
import { useCommandBar } from '@/hooks/use-command-bar';
import { useExecuteAction } from '@/hooks/use-execute-action';
import { useExecuteVoiceAction } from '@/hooks/use-execute-voice-action';
import { CommandInput } from './CommandInput';
import { CommandResult } from './CommandResult';
import { CommandHistory } from './CommandHistory';
import { ConfirmationPanel } from './ConfirmationPanel';
import { VoiceMicButton } from './VoiceMicButton';

export function CommandBar() {
  const t = useTranslations('commandBar');
  const {
    isOpen,
    close,
    recentCommands,
    addRecentCommand,
    sessionId,
  } = useCommandBar();
  const { execute, confirm, isLoading, result, error, reset } = useExecuteAction();
  const { executeVoice, isLoading: isVoiceLoading } = useExecuteVoiceAction();
  const dialogRef = useRef<HTMLDivElement>(null);
  const lastCommandRef = useRef<string>('');
  const [transcribedText, setTranscribedText] = useState<string | undefined>(undefined);

  const handleSubmit = useCallback(
    async (text: string) => {
      lastCommandRef.current = text;
      addRecentCommand(text);
      await execute(text, sessionId ?? undefined);
    },
    [execute, addRecentCommand, sessionId],
  );

  const handleRetry = useCallback(() => {
    if (lastCommandRef.current) {
      void execute(lastCommandRef.current, sessionId ?? undefined);
    }
  }, [execute, sessionId]);

  const handleConfirm = useCallback(() => {
    if (result?.confirmationId) {
      void confirm(result.confirmationId);
    }
  }, [confirm, result]);

  const handleCancel = useCallback(() => {
    reset();
  }, [reset]);

  const handleClose = useCallback(() => {
    close();
    // Defer reset to allow closing animation
    setTimeout(() => {
      reset();
      lastCommandRef.current = '';
    }, 150);
  }, [close, reset]);

  const handleSelectHistory = useCallback(
    (command: string) => {
      void handleSubmit(command);
    },
    [handleSubmit],
  );

  const handleAudioReady = useCallback(
    async (blob: Blob) => {
      const voiceResult = await executeVoice(blob, undefined, sessionId ?? undefined);
      if (voiceResult?.transcribedText) {
        setTranscribedText(voiceResult.transcribedText);
        // Auto-submit the transcribed text as a command
        addRecentCommand(voiceResult.transcribedText);
        lastCommandRef.current = voiceResult.transcribedText;
        await execute(voiceResult.transcribedText, sessionId ?? undefined);
      }
    },
    [executeVoice, execute, addRecentCommand, sessionId],
  );

  // Close on Escape
  useEffect(() => {
    if (!isOpen) return;

    function handleKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape') {
        e.preventDefault();
        handleClose();
      }
    }

    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen, handleClose]);

  if (!isOpen) return null;

  const showConfirmation = result?.needsConfirmation && !error;
  const showResult = (result && !result.needsConfirmation) || error || isLoading;
  const showHistory = !result && !error && !isLoading && recentCommands.length > 0;

  return (
    <div
      className="fixed inset-0 z-50 flex items-start justify-center bg-slate-900/40 p-4 pt-[15vh] sm:pt-[20vh]"
      onClick={(e) => {
        if (e.target === e.currentTarget) handleClose();
      }}
      role="presentation"
    >
      <div
        ref={dialogRef}
        className="w-full max-w-2xl overflow-hidden rounded-xl border border-slate-200 bg-white shadow-2xl sm:max-w-2xl"
        role="dialog"
        aria-label={t('placeholder')}
        aria-modal="true"
      >
        <Command label={t('placeholder')} shouldFilter={false}>
          <CommandInput
            onSubmit={handleSubmit}
            isLoading={isLoading || isVoiceLoading}
            disabled={!!showConfirmation}
            externalValue={transcribedText}
            trailingSlot={
              <VoiceMicButton
                onAudioReady={handleAudioReady}
                isProcessing={isVoiceLoading}
              />
            }
          />

          <Command.List className="max-h-80 overflow-y-auto">
            {showResult && (
              <CommandResult
                result={result}
                error={error}
                isLoading={isLoading}
                onRetry={handleRetry}
              />
            )}

            {showConfirmation && result && (
              <ConfirmationPanel
                result={result}
                onConfirm={handleConfirm}
                onCancel={handleCancel}
                isLoading={isLoading}
              />
            )}

            {showHistory && (
              <Command.Group>
                <CommandHistory
                  commands={recentCommands}
                  onSelect={handleSelectHistory}
                />
              </Command.Group>
            )}
          </Command.List>

          <div className="flex items-center justify-between border-t border-slate-200 px-4 py-2">
            <span className="text-xs text-slate-400">{t('openShortcut')}</span>
            <button
              type="button"
              onClick={handleClose}
              className="rounded-lg px-2 py-1 text-xs text-slate-500 transition-colors hover:bg-slate-100 hover:text-slate-700 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
            >
              {t('close')}
            </button>
          </div>
        </Command>
      </div>
    </div>
  );
}
