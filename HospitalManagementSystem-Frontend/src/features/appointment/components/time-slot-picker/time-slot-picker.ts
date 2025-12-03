import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { TimeSlot } from '../../../../core/services/availability-service';

@Component({
  selector: 'app-time-slot-picker',
  imports: [CommonModule],
  templateUrl: './time-slot-picker.html',
  styleUrl: './time-slot-picker.css',
})
export class TimeSlotPicker {
  @Input() timeSlots: TimeSlot[] = [];
  @Input() selectedTime: string | null = null;
  @Input() appointmentDuration: number = 30; // in minutes
  
  @Output() timeSelected = new EventEmitter<string>();

  onTimeSelect(time: string) {
    const slot = this.timeSlots.find(s => s.time === time);
    if (slot && slot.available) {
      this.selectedTime = time;
      this.timeSelected.emit(time);
    }
  }

  formatTime(time: string): string {
    const [hours, minutes] = time.split(':');
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? 'PM' : 'AM';
    const displayHour = hour % 12 || 12;
    return `${displayHour}:${minutes} ${ampm}`;
  }

  get hasAvailableSlots(): boolean {
    return this.timeSlots?.some(s => s.available) ?? false;
  }
}

