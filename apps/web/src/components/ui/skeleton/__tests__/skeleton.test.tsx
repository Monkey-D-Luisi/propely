// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import { Skeleton, SkeletonText, SkeletonTable } from '@/components/ui/skeleton';

describe('Skeleton', () => {
  it('renders a div with animate-pulse class', () => {
    const { container } = render(<Skeleton />);
    const el = container.firstChild as HTMLElement;
    expect(el.tagName).toBe('DIV');
    expect(el.className).toContain('animate-pulse');
  });

  it('applies custom className', () => {
    const { container } = render(<Skeleton className="h-10 w-full" />);
    const el = container.firstChild as HTMLElement;
    expect(el.className).toContain('h-10');
    expect(el.className).toContain('w-full');
  });
});

describe('SkeletonText', () => {
  it('renders single line by default', () => {
    const { container } = render(<SkeletonText />);
    const lines = container.querySelectorAll('.animate-pulse');
    expect(lines).toHaveLength(1);
  });

  it('renders multiple lines', () => {
    const { container } = render(<SkeletonText lines={3} />);
    const lines = container.querySelectorAll('.animate-pulse');
    expect(lines).toHaveLength(3);
  });

  it('applies custom widths to lines', () => {
    const { container } = render(<SkeletonText lines={2} widths={['w-1/2', 'w-1/3']} />);
    const lines = container.querySelectorAll('.animate-pulse');
    expect(lines[0].className).toContain('w-1/2');
    expect(lines[1].className).toContain('w-1/3');
  });

  it('applies custom className', () => {
    const { container } = render(<SkeletonText className="my-4" />);
    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper.className).toContain('my-4');
  });
});

describe('SkeletonTable', () => {
  it('renders default 3 rows and 4 columns', () => {
    const { container } = render(<SkeletonTable />);
    // Header row with 4 columns
    const headerCols = container.querySelector('.bg-slate-50')?.querySelectorAll('.animate-pulse');
    expect(headerCols).toHaveLength(4);

    // Body rows
    const bodyRows = container.querySelectorAll('.divide-y > div');
    expect(bodyRows).toHaveLength(3);

    // Each body row has 4 columns
    bodyRows.forEach(row => {
      const cols = row.querySelectorAll('.animate-pulse');
      expect(cols).toHaveLength(4);
    });
  });

  it('renders custom number of rows and columns', () => {
    const { container } = render(<SkeletonTable rows={2} columns={3} />);
    const headerCols = container.querySelector('.bg-slate-50')?.querySelectorAll('.animate-pulse');
    expect(headerCols).toHaveLength(3);

    const bodyRows = container.querySelectorAll('.divide-y > div');
    expect(bodyRows).toHaveLength(2);
  });

  it('applies custom className', () => {
    const { container } = render(<SkeletonTable className="mt-4" />);
    const el = container.firstChild as HTMLElement;
    expect(el.className).toContain('mt-4');
  });
});
