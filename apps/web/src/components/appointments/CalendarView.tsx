// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo, useCallback } from 'react';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import listPlugin from '@fullcalendar/list';
import type { EventInput, EventClickArg, DatesSetArg } from '@fullcalendar/core';
import type { DateClickArg } from '@fullcalendar/interaction';
import type { AppointmentListItem, AppointmentType, AppointmentStatus } from '@/hooks/useAppointments';

const typeColors: Record<AppointmentType, { bg: string; border: string; text: string }> = {
  PropertyViewing: { bg: '#dbeafe', border: '#3b82f6', text: '#1e40af' },
  OwnerMeeting: { bg: '#fef3c7', border: '#f59e0b', text: '#92400e' },
  Generic: { bg: '#f1f5f9', border: '#64748b', text: '#334155' },
};

const fadedStatuses: Set<AppointmentStatus> = new Set(['Completed', 'Cancelled']);

interface CalendarViewProps {
  appointments: AppointmentListItem[];
  isLoading: boolean;
  onEventClick: (appointment: AppointmentListItem) => void;
  onDateClick: (date: Date) => void;
  onDatesChange?: (start: Date, end: Date) => void;
}

export function CalendarView({
  appointments,
  isLoading,
  onEventClick,
  onDateClick,
  onDatesChange,
}: CalendarViewProps) {
  const events: EventInput[] = useMemo(() => {
    return appointments.map((apt) => {
      const colors = typeColors[apt.type] ?? typeColors.Generic;
      const isFaded = fadedStatuses.has(apt.status);

      return {
        id: apt.id,
        title: apt.title,
        start: apt.startTimeUtc,
        end: apt.endTimeUtc,
        allDay: apt.isAllDay,
        backgroundColor: isFaded ? '#f8fafc' : colors.bg,
        borderColor: isFaded ? '#cbd5e1' : colors.border,
        textColor: isFaded ? '#94a3b8' : colors.text,
        extendedProps: {
          appointment: apt,
          status: apt.status,
          type: apt.type,
        },
        classNames: isFaded ? ['opacity-60'] : [],
      };
    });
  }, [appointments]);

  const handleEventClick = useCallback(
    (info: EventClickArg) => {
      const apt = info.event.extendedProps.appointment as AppointmentListItem;
      if (apt) onEventClick(apt);
    },
    [onEventClick],
  );

  const handleDateClick = useCallback(
    (info: DateClickArg) => {
      onDateClick(info.date);
    },
    [onDateClick],
  );

  const handleDatesSet = useCallback(
    (info: DatesSetArg) => {
      onDatesChange?.(info.start, info.end);
    },
    [onDatesChange],
  );

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-4" data-testid="calendar-loading">
        <div className="h-10 w-full rounded-lg bg-slate-200" />
        <div className="h-[600px] rounded-xl bg-slate-100" />
      </div>
    );
  }

  return (
    <div data-testid="calendar-view" className="propely-calendar">
      <style>{`
        .propely-calendar .fc {
          --fc-border-color: #e2e8f0;
          --fc-button-bg-color: #fff;
          --fc-button-border-color: #e2e8f0;
          --fc-button-text-color: #334155;
          --fc-button-hover-bg-color: #f8fafc;
          --fc-button-hover-border-color: #cbd5e1;
          --fc-button-active-bg-color: #4f46e5;
          --fc-button-active-border-color: #4f46e5;
          --fc-button-active-text-color: #fff;
          --fc-today-bg-color: #f0f9ff;
          --fc-event-border-color: transparent;
          --fc-page-bg-color: #fff;
          --fc-neutral-bg-color: #f8fafc;
          font-family: inherit;
        }
        .propely-calendar .fc .fc-toolbar-title {
          font-size: 1.25rem;
          font-weight: 700;
          color: #0f172a;
        }
        .propely-calendar .fc .fc-button {
          border-radius: 0.5rem;
          font-size: 0.875rem;
          font-weight: 500;
          padding: 0.375rem 0.75rem;
          box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
          transition: all 150ms;
        }
        .propely-calendar .fc .fc-button:focus {
          box-shadow: 0 0 0 2px #fff, 0 0 0 4px #4f46e5;
        }
        .propely-calendar .fc .fc-button-group .fc-button {
          border-radius: 0;
        }
        .propely-calendar .fc .fc-button-group .fc-button:first-child {
          border-radius: 0.5rem 0 0 0.5rem;
        }
        .propely-calendar .fc .fc-button-group .fc-button:last-child {
          border-radius: 0 0.5rem 0.5rem 0;
        }
        .propely-calendar .fc .fc-daygrid-day-number,
        .propely-calendar .fc .fc-col-header-cell-cushion {
          font-size: 0.875rem;
          color: #475569;
          text-decoration: none;
        }
        .propely-calendar .fc .fc-event {
          border-radius: 0.375rem;
          padding: 2px 4px;
          font-size: 0.75rem;
          cursor: pointer;
          border-width: 0 0 0 3px;
        }
        .propely-calendar .fc .fc-timegrid-event {
          border-radius: 0.375rem;
          border-width: 0 0 0 3px;
        }
        .propely-calendar .fc .fc-daygrid-event-dot {
          display: none;
        }
        .propely-calendar .fc .fc-list-event-dot {
          border-radius: 9999px;
        }
        .propely-calendar .fc td,
        .propely-calendar .fc th {
          border-color: #e2e8f0;
        }
        @media (max-width: 640px) {
          .propely-calendar .fc .fc-toolbar {
            flex-direction: column;
            gap: 0.5rem;
          }
          .propely-calendar .fc .fc-toolbar-title {
            font-size: 1rem;
          }
        }
      `}</style>
      <FullCalendar
        plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin, listPlugin]}
        initialView="dayGridMonth"
        headerToolbar={{
          left: 'prev,next today',
          center: 'title',
          right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek',
        }}
        events={events}
        eventClick={handleEventClick}
        dateClick={handleDateClick}
        datesSet={handleDatesSet}
        editable={false}
        selectable={true}
        dayMaxEvents={3}
        nowIndicator={true}
        height="auto"
        eventDisplay="block"
        buttonText={{
          today: 'Today',
          month: 'Month',
          week: 'Week',
          day: 'Day',
          list: 'List',
        }}
      />
    </div>
  );
}
