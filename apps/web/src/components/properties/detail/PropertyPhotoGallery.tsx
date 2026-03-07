// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useCallback, useEffect, useRef } from 'react';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { useFocusTrap } from '@/hooks/use-focus-trap';
import type { PropertyMedia } from '@/lib/schemas';

interface PropertyPhotoGalleryProps {
  media: PropertyMedia[];
}

export function PropertyPhotoGallery({ media }: PropertyPhotoGalleryProps) {
  const t = useTranslations('properties.detail');
  const photos = media.filter((m) => m.mediaType === 'Photo');
  const [heroIndex, setHeroIndex] = useState(0);
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null);
  const lightboxRef = useRef<HTMLDivElement>(null);

  useFocusTrap(lightboxRef, lightboxIndex !== null);

  // Close lightbox on Escape, navigate with arrows
  const handleLightboxKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (lightboxIndex === null) return;
      if (e.key === 'Escape') {
        setLightboxIndex(null);
      } else if (e.key === 'ArrowRight' && lightboxIndex < photos.length - 1) {
        setLightboxIndex(lightboxIndex + 1);
      } else if (e.key === 'ArrowLeft' && lightboxIndex > 0) {
        setLightboxIndex(lightboxIndex - 1);
      }
    },
    [lightboxIndex, photos.length],
  );

  useEffect(() => {
    if (lightboxIndex !== null) {
      document.addEventListener('keydown', handleLightboxKeyDown);
      return () => document.removeEventListener('keydown', handleLightboxKeyDown);
    }
  }, [lightboxIndex, handleLightboxKeyDown]);

  if (photos.length === 0) {
    return (
      <div
        className="flex h-64 items-center justify-center rounded-xl bg-slate-100"
        data-testid="photo-gallery-empty"
      >
        <div className="text-center">
          <span className="material-symbols-outlined text-4xl text-slate-300" aria-hidden="true">
            photo_library
          </span>
          <p className="mt-2 text-sm text-slate-400">{t('noPhotos')}</p>
        </div>
      </div>
    );
  }

  const heroPhoto = photos[heroIndex];

  return (
    <>
      <div data-testid="photo-gallery">
        {/* Hero photo */}
        <button
          type="button"
          onClick={() => setLightboxIndex(heroIndex)}
          className="group relative w-full overflow-hidden rounded-xl bg-slate-100 focus:outline-none focus:ring-2 focus:ring-primary-600"
          data-testid="hero-photo"
        >
          <div className="relative aspect-video w-full">
            {heroPhoto?.url || heroPhoto?.thumbnailUrl ? (
              <Image
                src={heroPhoto.url ?? heroPhoto.thumbnailUrl ?? ''}
                alt={heroPhoto.fileName}
                fill
                unoptimized
                className="object-cover transition group-hover:scale-[1.02]"
              />
            ) : (
              <div className="flex h-full w-full items-center justify-center">
                <span className="material-symbols-outlined text-4xl text-slate-300" aria-hidden="true">
                  image
                </span>
              </div>
            )}
          </div>
        </button>

        {/* Thumbnail row */}
        {photos.length > 1 && (
          <div className="mt-3 flex gap-2 overflow-x-auto" data-testid="thumbnail-row">
            {photos.slice(0, 5).map((photo, index) => (
              <button
                key={photo.id}
                type="button"
                onClick={() => setHeroIndex(index)}
                className={`relative h-16 w-16 flex-shrink-0 overflow-hidden rounded-lg bg-slate-100 transition focus:outline-none focus:ring-2 focus:ring-primary-600 ${
                  index === heroIndex
                    ? 'ring-2 ring-primary-600'
                    : 'opacity-70 hover:opacity-100'
                }`}
                data-testid={`thumbnail-${index}`}
              >
                {photo.thumbnailUrl ? (
                  <Image
                    src={photo.thumbnailUrl}
                    alt={photo.fileName}
                    fill
                    unoptimized
                    className="object-cover"
                  />
                ) : (
                  <div className="flex h-full w-full items-center justify-center">
                    <span className="material-symbols-outlined text-sm text-slate-300" aria-hidden="true">
                      image
                    </span>
                  </div>
                )}
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Lightbox */}
      {lightboxIndex !== null && (
        <div
          ref={lightboxRef}
          role="dialog"
          aria-modal="true"
          aria-label="Photo gallery lightbox"
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/90"
          onClick={() => setLightboxIndex(null)}
          data-testid="lightbox"
        >
          {/* Close button */}
          <button
            type="button"
            onClick={() => setLightboxIndex(null)}
            className="absolute right-4 top-4 rounded-full bg-white/10 p-2 text-white transition hover:bg-white/20"
            aria-label="Close"
            data-testid="lightbox-close"
          >
            <span className="material-symbols-outlined" aria-hidden="true">close</span>
          </button>

          {/* Previous button */}
          {lightboxIndex > 0 && (
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                setLightboxIndex(lightboxIndex - 1);
              }}
              className="absolute left-4 rounded-full bg-white/10 p-2 text-white transition hover:bg-white/20"
              aria-label="Previous photo"
              data-testid="lightbox-prev"
            >
              <span className="material-symbols-outlined" aria-hidden="true">chevron_left</span>
            </button>
          )}

          {/* Next button */}
          {lightboxIndex < photos.length - 1 && (
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                setLightboxIndex(lightboxIndex + 1);
              }}
              className="absolute right-4 rounded-full bg-white/10 p-2 text-white transition hover:bg-white/20"
              aria-label="Next photo"
              data-testid="lightbox-next"
            >
              <span className="material-symbols-outlined" aria-hidden="true">chevron_right</span>
            </button>
          )}

          {/* Current photo */}
          <div className="max-h-[90vh] max-w-[90vw]" onClick={(e) => e.stopPropagation()}>
            {photos[lightboxIndex]?.url ? (
              <Image
                src={photos[lightboxIndex].url!}
                alt={photos[lightboxIndex].fileName}
                width={1920}
                height={1080}
                unoptimized
                className="max-h-[90vh] max-w-[90vw] object-contain"
                data-testid="lightbox-image"
              />
            ) : (
              <div className="flex h-96 w-96 items-center justify-center rounded-xl bg-slate-800">
                <span className="material-symbols-outlined text-4xl text-slate-500" aria-hidden="true">
                  image
                </span>
              </div>
            )}
          </div>

          {/* Counter */}
          <div className="absolute bottom-4 text-center text-sm text-white/70" data-testid="lightbox-counter">
            {lightboxIndex + 1} / {photos.length}
          </div>
        </div>
      )}
    </>
  );
}
