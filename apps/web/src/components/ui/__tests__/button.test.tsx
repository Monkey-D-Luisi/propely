// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi } from 'vitest';
import { Button } from '@/components/ui/button';
import { renderWithProviders, screen } from '@test/utils';

describe('Button', () => {
  it('renders children text', () => {
    renderWithProviders(<Button>Click me</Button>);
    expect(screen.getByRole('button', { name: 'Click me' })).toBeInTheDocument();
  });

  it('applies custom className', () => {
    renderWithProviders(<Button className="bg-red-500">Styled</Button>);
    const button = screen.getByRole('button', { name: 'Styled' });
    expect(button.className).toContain('bg-red-500');
  });

  it('forwards disabled attribute', () => {
    renderWithProviders(<Button disabled>Disabled</Button>);
    expect(screen.getByRole('button', { name: 'Disabled' })).toBeDisabled();
  });

  it('calls onClick handler', async () => {
    const handleClick = vi.fn();
    const { user } = renderWithProviders(
      <Button onClick={handleClick}>Press</Button>,
    );
    await user.click(screen.getByRole('button', { name: 'Press' }));
    expect(handleClick).toHaveBeenCalledOnce();
  });

  it('does not call onClick when disabled', async () => {
    const handleClick = vi.fn();
    const { user } = renderWithProviders(
      <Button disabled onClick={handleClick}>No click</Button>,
    );
    await user.click(screen.getByRole('button', { name: 'No click' }));
    expect(handleClick).not.toHaveBeenCalled();
  });
});
