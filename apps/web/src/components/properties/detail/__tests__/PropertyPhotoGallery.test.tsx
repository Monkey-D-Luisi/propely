// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { PropertyPhotoGallery } from '../PropertyPhotoGallery';
import type { PropertyMedia } from '@/lib/schemas';

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [key: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
  useRouter: () => ({ push: vi.fn() }),
}));

const mockPhotos: PropertyMedia[] = [
  {
    id: '00000000-0000-0000-0000-000000000001',
    propertyId: '00000000-0000-0000-0000-000000000010',
    mediaType: 'Photo',
    storagePath: '/photos/1.jpg',
    fileName: 'living-room.jpg',
    contentType: 'image/jpeg',
    sizeBytes: 500000,
    width: 1920,
    height: 1080,
    displayOrder: 0,
    uploadedAtUtc: '2026-01-15T10:00:00Z',
    url: 'https://example.com/photos/1.jpg',
    thumbnailUrl: 'https://example.com/photos/1_thumb.jpg',
  },
  {
    id: '00000000-0000-0000-0000-000000000002',
    propertyId: '00000000-0000-0000-0000-000000000010',
    mediaType: 'Photo',
    storagePath: '/photos/2.jpg',
    fileName: 'bedroom.jpg',
    contentType: 'image/jpeg',
    sizeBytes: 400000,
    width: 1920,
    height: 1080,
    displayOrder: 1,
    uploadedAtUtc: '2026-01-15T10:01:00Z',
    url: 'https://example.com/photos/2.jpg',
    thumbnailUrl: 'https://example.com/photos/2_thumb.jpg',
  },
  {
    id: '00000000-0000-0000-0000-000000000003',
    propertyId: '00000000-0000-0000-0000-000000000010',
    mediaType: 'Photo',
    storagePath: '/photos/3.jpg',
    fileName: 'kitchen.jpg',
    contentType: 'image/jpeg',
    sizeBytes: 350000,
    width: 1920,
    height: 1080,
    displayOrder: 2,
    uploadedAtUtc: '2026-01-15T10:02:00Z',
    url: 'https://example.com/photos/3.jpg',
    thumbnailUrl: 'https://example.com/photos/3_thumb.jpg',
  },
];

describe('PropertyPhotoGallery', () => {
  it('shows empty state when no photos', () => {
    renderWithProviders(<PropertyPhotoGallery media={[]} />);
    expect(screen.getByTestId('photo-gallery-empty')).toBeInTheDocument();
    expect(screen.getByText('No photos uploaded yet')).toBeInTheDocument();
  });

  it('renders hero photo and thumbnail row', () => {
    renderWithProviders(<PropertyPhotoGallery media={mockPhotos} />);
    expect(screen.getByTestId('photo-gallery')).toBeInTheDocument();
    expect(screen.getByTestId('hero-photo')).toBeInTheDocument();
    expect(screen.getByTestId('thumbnail-row')).toBeInTheDocument();
  });

  it('renders thumbnails for each photo', () => {
    renderWithProviders(<PropertyPhotoGallery media={mockPhotos} />);
    expect(screen.getByTestId('thumbnail-0')).toBeInTheDocument();
    expect(screen.getByTestId('thumbnail-1')).toBeInTheDocument();
    expect(screen.getByTestId('thumbnail-2')).toBeInTheDocument();
  });

  it('clicking thumbnail changes the hero image', async () => {
    const { user } = renderWithProviders(<PropertyPhotoGallery media={mockPhotos} />);
    // Click second thumbnail
    await user.click(screen.getByTestId('thumbnail-1'));
    // The hero image should now show the second photo
    const heroPhoto = screen.getByTestId('hero-photo');
    const heroImg = heroPhoto.querySelector('img');
    expect(heroImg).toHaveAttribute('alt', 'bedroom.jpg');
  });

  it('clicking hero opens lightbox', async () => {
    const { user } = renderWithProviders(<PropertyPhotoGallery media={mockPhotos} />);
    await user.click(screen.getByTestId('hero-photo'));
    expect(screen.getByTestId('lightbox')).toBeInTheDocument();
    expect(screen.getByTestId('lightbox-counter')).toHaveTextContent('1 / 3');
  });

  it('filters out non-photo media', () => {
    const mixed: PropertyMedia[] = [
      ...mockPhotos,
      {
        ...mockPhotos[0],
        id: '00000000-0000-0000-0000-000000000004',
        mediaType: 'FloorPlan',
        fileName: 'plan.pdf',
      },
    ];
    renderWithProviders(<PropertyPhotoGallery media={mixed} />);
    // Should only show 3 thumbnails (not the floor plan)
    expect(screen.getByTestId('thumbnail-0')).toBeInTheDocument();
    expect(screen.getByTestId('thumbnail-1')).toBeInTheDocument();
    expect(screen.getByTestId('thumbnail-2')).toBeInTheDocument();
    expect(screen.queryByTestId('thumbnail-3')).not.toBeInTheDocument();
  });

  it('does not render thumbnail row when only one photo', () => {
    renderWithProviders(<PropertyPhotoGallery media={[mockPhotos[0]]} />);
    expect(screen.getByTestId('hero-photo')).toBeInTheDocument();
    expect(screen.queryByTestId('thumbnail-row')).not.toBeInTheDocument();
  });
});
