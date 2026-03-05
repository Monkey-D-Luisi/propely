// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useRef, useState, useCallback } from 'react';
import { useTranslations } from 'next-intl';

interface SelectedFile {
  id: string;
  file: File;
  previewUrl: string;
  type: 'photo' | 'floorPlan';
}

interface MediaStepProps {
  data: Record<string, unknown>;
  onChange: (data: Record<string, unknown>) => void;
}

const MAX_FILE_SIZE_MB = 10;
const MAX_PHOTOS = 30;

export function MediaStep({ data, onChange }: MediaStepProps) {
  const t = useTranslations('properties.form');
  const photoInputRef = useRef<HTMLInputElement>(null);
  const floorPlanInputRef = useRef<HTMLInputElement>(null);
  const [dragActive, setDragActive] = useState(false);

  // Store selected files in data so the wizard can access them
  const selectedFiles = (data._selectedFiles as SelectedFile[]) ?? [];

  const photos = selectedFiles.filter((f) => f.type === 'photo');
  const floorPlans = selectedFiles.filter((f) => f.type === 'floorPlan');

  const addFiles = useCallback(
    (files: FileList | File[], type: 'photo' | 'floorPlan') => {
      const newFiles: SelectedFile[] = [];
      const validFiles = Array.from(files).filter(
        (f) => f.size <= MAX_FILE_SIZE_MB * 1024 * 1024 && f.type.startsWith('image/'),
      );

      for (const file of validFiles) {
        if (type === 'photo' && photos.length + newFiles.length >= MAX_PHOTOS) break;
        newFiles.push({
          id: crypto.randomUUID(),
          file,
          previewUrl: URL.createObjectURL(file),
          type,
        });
      }

      if (newFiles.length > 0) {
        onChange({
          ...data,
          _selectedFiles: [...selectedFiles, ...newFiles],
        });
      }
    },
    [data, onChange, photos.length, selectedFiles],
  );

  const removeFile = useCallback(
    (id: string) => {
      const file = selectedFiles.find((f) => f.id === id);
      if (file) {
        URL.revokeObjectURL(file.previewUrl);
      }
      onChange({
        ...data,
        _selectedFiles: selectedFiles.filter((f) => f.id !== id),
      });
    },
    [data, onChange, selectedFiles],
  );

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setDragActive(false);
      if (e.dataTransfer.files.length > 0) {
        addFiles(e.dataTransfer.files, 'photo');
      }
    },
    [addFiles],
  );

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setDragActive(true);
  };

  const handleDragLeave = () => {
    setDragActive(false);
  };

  return (
    <div className="space-y-8">
      {/* Photo Upload Area */}
      <div>
        <label className="mb-2 block text-sm font-medium text-slate-700">{t('uploadPhotos')}</label>

        {/* Drag & Drop Zone */}
        <div
          onDrop={handleDrop}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onClick={() => photoInputRef.current?.click()}
          role="button"
          tabIndex={0}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') photoInputRef.current?.click();
          }}
          className={`flex cursor-pointer flex-col items-center justify-center rounded-xl border-2 border-dashed p-8 transition ${
            dragActive
              ? 'border-primary-400 bg-primary-50'
              : 'border-slate-300 bg-slate-50 hover:border-slate-400 hover:bg-slate-100'
          }`}
          data-testid="photo-dropzone"
        >
          <span className="material-symbols-outlined mb-2 text-3xl text-slate-400" aria-hidden="true">
            cloud_upload
          </span>
          <p className="text-sm text-slate-600">{t('dragDrop')}</p>
          <p className="mt-1 text-xs text-slate-400">
            {t('maxFileSize', { size: MAX_FILE_SIZE_MB })} &middot; {t('maxPhotos', { count: MAX_PHOTOS })}
          </p>
        </div>

        <input
          ref={photoInputRef}
          type="file"
          accept="image/*"
          multiple
          className="hidden"
          onChange={(e) => {
            if (e.target.files) addFiles(e.target.files, 'photo');
            e.target.value = '';
          }}
        />

        {/* Photo Thumbnails */}
        {photos.length > 0 && (
          <div className="mt-4 grid grid-cols-3 gap-3 sm:grid-cols-4 md:grid-cols-6">
            {photos.map((photo) => (
              <div key={photo.id} className="group relative aspect-square overflow-hidden rounded-lg border border-slate-200">
                <img
                  src={photo.previewUrl}
                  alt={photo.file.name}
                  className="h-full w-full object-cover"
                />
                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    removeFile(photo.id);
                  }}
                  className="absolute right-1 top-1 flex h-6 w-6 items-center justify-center rounded-full bg-red-500 text-white opacity-0 transition group-hover:opacity-100"
                  aria-label={`Remove ${photo.file.name}`}
                >
                  <span className="material-symbols-outlined text-sm" aria-hidden="true">close</span>
                </button>
                <p className="absolute inset-x-0 bottom-0 truncate bg-black/50 px-1 py-0.5 text-[10px] text-white">
                  {photo.file.name}
                </p>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Floor Plan Upload */}
      <div>
        <label className="mb-2 block text-sm font-medium text-slate-700">{t('uploadFloorPlans')}</label>
        <button
          type="button"
          onClick={() => floorPlanInputRef.current?.click()}
          className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
        >
          <span className="material-symbols-outlined mr-1.5 align-middle text-base" aria-hidden="true">
            add
          </span>
          {t('uploadFloorPlans')}
        </button>

        <input
          ref={floorPlanInputRef}
          type="file"
          accept="image/*"
          multiple
          className="hidden"
          onChange={(e) => {
            if (e.target.files) addFiles(e.target.files, 'floorPlan');
            e.target.value = '';
          }}
        />

        {floorPlans.length > 0 && (
          <div className="mt-4 grid grid-cols-3 gap-3 sm:grid-cols-4">
            {floorPlans.map((fp) => (
              <div key={fp.id} className="group relative aspect-square overflow-hidden rounded-lg border border-slate-200">
                <img
                  src={fp.previewUrl}
                  alt={fp.file.name}
                  className="h-full w-full object-cover"
                />
                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    removeFile(fp.id);
                  }}
                  className="absolute right-1 top-1 flex h-6 w-6 items-center justify-center rounded-full bg-red-500 text-white opacity-0 transition group-hover:opacity-100"
                  aria-label={`Remove ${fp.file.name}`}
                >
                  <span className="material-symbols-outlined text-sm" aria-hidden="true">close</span>
                </button>
                <p className="absolute inset-x-0 bottom-0 truncate bg-black/50 px-1 py-0.5 text-[10px] text-white">
                  {fp.file.name}
                </p>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Virtual Tour / Video URLs */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="media-virtualTour" className="mb-1.5 block text-sm font-medium text-slate-700">
            Virtual Tour URL
          </label>
          <input
            id="media-virtualTour"
            type="url"
            value={(data.virtualTourUrl as string) ?? ''}
            onChange={(e) => onChange({ ...data, virtualTourUrl: e.target.value || undefined })}
            placeholder="https://..."
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="media-video" className="mb-1.5 block text-sm font-medium text-slate-700">
            Video URL
          </label>
          <input
            id="media-video"
            type="url"
            value={(data.videoUrl as string) ?? ''}
            onChange={(e) => onChange({ ...data, videoUrl: e.target.value || undefined })}
            placeholder="https://..."
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
      </div>
    </div>
  );
}
