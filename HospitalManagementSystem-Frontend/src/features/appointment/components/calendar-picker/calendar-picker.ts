import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal, effect } from '@angular/core';

export interface CalendarDate {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  isSelected: boolean;
  isDisabled: boolean;
  isAvailable: boolean;
  isFullyBooked: boolean;
}

@Component({
  selector: 'app-calendar-picker',
  imports: [CommonModule],
  templateUrl: './calendar-picker.html',
  styleUrl: './calendar-picker.css',
})
export class CalendarPicker {
  @Input() selectedDate: Date | null = null;
  @Input() disabledDates: Date[] = []; // Dates to disable (past dates, fully booked, etc.)
  @Input() availableDates: Date[] = []; // Dates that are available
  @Input() fullyBookedDates: Date[] = []; // Dates that are fully booked
  @Input() minDate: Date = new Date(); // Minimum selectable date
  @Input() maxDate: Date = new Date(); // Maximum selectable date (3 months ahead)
  
  @Output() dateSelected = new EventEmitter<Date>();

  currentMonth = signal<Date>(new Date());
  calendarDays = signal<CalendarDate[]>([]);
  monthNames = ['January', 'February', 'March', 'April', 'May', 'June', 
                'July', 'August', 'September', 'October', 'November', 'December'];
  dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  constructor() {
    // Initialize max date to 3 months ahead
    const max = new Date();
    max.setMonth(max.getMonth() + 3);
    this.maxDate = max;

    // Generate calendar when month changes
    effect(() => {
      this.generateCalendar();
    });
  }

  ngOnInit() {
    if (this.selectedDate) {
      this.currentMonth.set(new Date(this.selectedDate.getFullYear(), this.selectedDate.getMonth(), 1));
    }
    this.generateCalendar();
  }

  generateCalendar() {
    const year = this.currentMonth().getFullYear();
    const month = this.currentMonth().getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();

    const days: CalendarDate[] = [];

    // Previous month's trailing days
    const prevMonth = new Date(year, month, 0);
    const daysInPrevMonth = prevMonth.getDate();
    for (let i = startingDayOfWeek - 1; i >= 0; i--) {
      const date = new Date(year, month - 1, daysInPrevMonth - i);
      days.push(this.createCalendarDate(date, false));
    }

    // Current month's days
    for (let day = 1; day <= daysInMonth; day++) {
      const date = new Date(year, month, day);
      days.push(this.createCalendarDate(date, true));
    }

    // Next month's leading days (to fill the grid)
    const totalCells = 42; // 6 weeks * 7 days
    const remainingCells = totalCells - days.length;
    for (let day = 1; day <= remainingCells; day++) {
      const date = new Date(year, month + 1, day);
      days.push(this.createCalendarDate(date, false));
    }

    this.calendarDays.set(days);
  }

  createCalendarDate(date: Date, isCurrentMonth: boolean): CalendarDate {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const dateOnly = new Date(date);
    dateOnly.setHours(0, 0, 0, 0);

    const isToday = dateOnly.getTime() === today.getTime();
    const isSelected = this.selectedDate && 
      dateOnly.getTime() === new Date(this.selectedDate).setHours(0, 0, 0, 0);
    
    // Check if date is disabled
    const isPast = dateOnly < today;
    const isBeforeMin = dateOnly < this.minDate;
    const isAfterMax = dateOnly > this.maxDate;
    const isInDisabledList = this.disabledDates.some(d => 
      new Date(d).setHours(0, 0, 0, 0) === dateOnly.getTime()
    );
    
    const isDisabled = isPast || isBeforeMin || isAfterMax || isInDisabledList;
    
    // Check availability
    const isAvailable = this.availableDates.some(d => 
      new Date(d).setHours(0, 0, 0, 0) === dateOnly.getTime()
    );
    const isFullyBooked = this.fullyBookedDates.some(d => 
      new Date(d).setHours(0, 0, 0, 0) === dateOnly.getTime()
    );

    return {
      date,
      isCurrentMonth,
      isToday,
      isSelected: !!isSelected,
      isDisabled,
      isAvailable,
      isFullyBooked
    };
  }

  selectDate(day: CalendarDate) {
    if (day.isDisabled || !day.isCurrentMonth) return;
    
    this.selectedDate = day.date;
    this.dateSelected.emit(day.date);
    this.generateCalendar(); // Refresh to update selected state
  }

  previousMonth() {
    const newDate = new Date(this.currentMonth());
    newDate.setMonth(newDate.getMonth() - 1);
    this.currentMonth.set(newDate);
  }

  nextMonth() {
    const newDate = new Date(this.currentMonth());
    newDate.setMonth(newDate.getMonth() + 1);
    
    // Don't go beyond max date
    if (newDate <= this.maxDate) {
      this.currentMonth.set(newDate);
    }
  }

  canGoToNextMonth(): boolean {
    const current = this.currentMonth();
    const nextMonth = new Date(current.getFullYear(), current.getMonth() + 1, 1);
    return nextMonth <= this.maxDate;
  }

  goToToday() {
    this.currentMonth.set(new Date());
    this.selectDate(this.createCalendarDate(new Date(), true));
  }

  getMonthYearLabel(): string {
    return `${this.monthNames[this.currentMonth().getMonth()]} ${this.currentMonth().getFullYear()}`;
  }
}

