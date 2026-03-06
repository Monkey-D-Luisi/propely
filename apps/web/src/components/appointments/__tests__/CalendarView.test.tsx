// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { CalendarView } from '../CalendarView';
import type { AppointmentListItem } from '@/hooks/useAppointments';

// FullCalendar is not JSDOM-compatible -- mock it to verify prop wiring.
vi.mock('@fullcalendar/react', () => ({
  default: (props: Record<string, unknown>) => (
    <div data-testid="fullcalendar-mock" data-view={props.initialView as string}>
      FullCalendar
    </div>
  ),
}));

vi.mock('@fullcalendar/daygrid', () => ({ default: {} }));
vi.mock('@fullcalendar/timegrid', () => ({ default: {} }));
vi.mock('@fullcalendar/interaction', () => ({ default: {} }));
vi.mock('@fullcalendar/list', () => ({ default: {} }));

const baseProps = {
  appointments: [] as AppointmentListItem[],
  isLoading: false,
  onEventClick: vi.fn(),
  onDateClick: vi.fn(),
};

describe('CalendarView', () => {
  it('renders the calendar container', () => {
    renderWithProviders(<CalendarView {...baseProps} />);
    expect(screen.getByTestId('calendar-view')).toBeInTheDocument();
  });

  it('renders loading skeleton when isLoading is true', () => {
    renderWithProviders(<CalendarView {...baseProps} isLoading={true} />);
    expect(screen.getByTestId('calendar-loading')).toBeInTheDocument();
    expect(screen.queryByTestId('calendar-view')).not.toBeInTheDocument();
  });

  it('renders the FullCalendar mock when not loading', () => {
    renderWithProviders(<CalendarView {...baseProps} isLoading={false} />);
    expect(screen.getByTestId('fullcalendar-mock')).toBeInTheDocument();
  });

  it('passes dayGridMonth as initial view', () => {
    renderWithProviders(<CalendarView {...baseProps} />);
    const fc = screen.getByTestId('fullcalendar-mock');
    expect(fc.getAttribute('data-view')).toBe('dayGridMonth');
  });
});
