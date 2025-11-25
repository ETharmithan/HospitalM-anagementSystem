import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-appointment-item',
  imports: [CommonModule],
  templateUrl: './appointment-item.html',
  styleUrl: './appointment-item.css',
})
export class AppointmentItem {
  @Input() doctorName = 'Dr. Name';
  @Input() specialty = 'Specialty';
  @Input() date = 'Date';
  @Input() time = 'Time';
  @Input() avatarUrl = '';
}
