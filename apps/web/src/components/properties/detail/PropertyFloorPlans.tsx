// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useCallback, useEffect, useRef } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useFocusTrap } from '@/hooks/use-focus-trap';
import type { PropertyMedia } from '@/lib/schemas';

interface PropertyFloorPlansProps {
  media: PropertyMedia[];
}

export function PropertyFloorPlans({ media }: PropertyFloorPlansProps) {
  const t = useTranslations('properties.detail');
  const floorPlans = media.filter((m) => m.mediaType === 'FloorPlan');
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null);
  const lightboxRef = useRef<HTMLDivElement>(null);

  useFocusTrap(lightboxRef, lightboxIndex !== null);

  const handleLightboxKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (lightboxIndex === null) return;
      if (e.key === 'Escape') {
        setLightboxIndex(null);
      } else if (e.key === 'ArrowRight' && lightboxIndex < floorPlans.length - 1) {
        setLightboxIndex(lightboxIndex + 1);
      } else if (e.key === 'ArrowLeft' && lightboxIndex > 0) {
        setLightboxIndex(lightboxIndex - 1);
      }
    },
    [lightboxIndex, floorPlans.length],
  );

  useEffect(() => {
    if (lightboxIndex !== null) {
      document.addEventListener('keydown', handleLightboxKeyDown);
      return () => document.removeEventListener('keydown', handleLightboxKeyDown);
    }
  }, [lightboxIndex, handleLightboxKeyDown]);

  if (floorPlans.length === 0) {
    return (
      <div
        className="flex h-48 items-center justify-center rounded-xl bg-slate-100"
        data-testid="floor-plans-empty"
      >
        <div className="text-center">
          <span className="material-symbols-outlined text-4xl text-slate-300" aria-hidden="true">
            floor
          </span>
          <p className="mt-2 text-sm text-slate-400">{t('noFloorPlans')}</p>
        </div>
      </div>
    );
  }

  return (
    <>
      <div
        className="grid grid-cols-2 gap-3 sm:grid-cols-3"
        data-testid="floor-plans-grid"
      >
        {floorPlans.map((plan, index) => (
          <button
            key={plan.id}
            type="button"
            onClick={() => setLightboxIndex(index)}
            className="group relative aspect-[4/3] overflow-hidden rounded-lg bg-slate-100 focus:outline-none focus:ring-2 focus:ring-primary-600"
            data-testid={`floor-plan-${index}`}
          >
            {plan.thumbnailUrl || plan.url ? (
              <Image
                src={plan.thumbnailUrl ?? plan.url ?? ''}
                alt={plan.fileName}
                fill
                unoptimized
                className="object-cover transition group-hover:scale-105"
              />
            ) : (
              <div className="flex h-full w-full items-center justify-center">
                <span className="material-symbols-outlined text-2xl text-slate-300" aria-hidden="true">
                  floor
                </span>
              </div>
            )}
          </button>
        ))}
      </div>

      {/* Lightbox */}
      {lightboxIndex !== null && (
        <div
          ref={lightboxRef}
          role="dialog"
          aria-modal="true"
          aria-label="Floor plan lightbox"
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/90"
          onClick={() => setLightboxIndex(null)}
          data-testid="floor-plan-lightbox"
        >
          <button
            type="button"
            onClick={() => setLightboxIndex(null)}
            className="absolute right-4 top-4 rounded-full bg-white/10 p-2 text-white transition hover:bg-white/20"
            aria-label="Close"
          >
            <span className="material-symbols-outlined" aria-hidden="true">close</span>
          </button>

          {lightboxIndex > 0 && (
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                setLightboxIndex(lightboxIndex - 1);
              }}
              className="absolute left-4 rounded-full bg-white/10 p-2 text-white transition hover:bg-white/20"
              aria-label="Previous floor plan"
            >
              <span className="material-symbols-outlined" aria-hidden="true">chevron_left</span>
            </button>
          )}

          {lightboxIndex < floorPlans.length - 1 && (
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                setLightboxIndex(lightboxIndex + 1);
              }}
              className="absolute right-4 rounded-full bg-white/10 p-2 text-white transition hover:bg-white/20"
              aria-label="Next floor plan"
            >
              <span className="material-symbols-outlined" aria-hidden="true">chevron_right</span>
            </button>
          )}

          <div className="max-h-[90vh] max-w-[90vw]" onClick={(e) => e.stopPropagation()}>
            {floorPlans[lightboxIndex]?.url ? (
              <Image
                src={floorPlans[lightboxIndex].url!}
                alt={floorPlans[lightboxIndex].fileName}
                width={1920}
                height={1080}
                unoptimized
                className="max-h-[90vh] max-w-[90vw] object-contain"
              />
            ) : (
              <div className="flex h-96 w-96 items-center justify-center rounded-xl bg-slate-800">
                <span className="material-symbols-outlined text-4xl text-slate-500" aria-hidden="true">
                  floor
                </span>
              </div>
            )}
          </div>

          <div className="absolute bottom-4 text-center text-sm text-white/70">
            {lightboxIndex + 1} / {floorPlans.length}
          </div>
        </div>
      )}
    </>
  );
}
