import { Component, signal } from '@angular/core';
import { AppointmentItem } from '../features/patient/appointment-item/appointment-item';
import { QuickCard } from '../layout/cards/quick-card/quick-card';
import { Footer } from '../layout/patient/footer/footer';
import { Appointment, QuickAction } from '../types/patientdetails';

@Component({
  selector: 'app-home',
  imports: [AppointmentItem, QuickCard, Footer],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected registermode = signal(false);

  welcomeName = 'Alex';
  quickActions: QuickAction[] = [
    { icon: 'add_box', title: 'Book a New Appointment', description: 'Find available slots and schedule your next visit with a specialist.', actionText: 'Book Now' },
    { icon: 'stethoscope', title: 'Find a Doctor', description: 'Search our directory of world-class doctors and specialists.', actionText: 'Search Doctors' },
    { icon: 'folder_open', title: 'View Medical Records', description: 'Access your health history, lab results, and prescriptions securely.', actionText: 'View Records' },
  ];

  nextAppointments: Appointment[] = [
    { doctorName: 'Dr. Evelyn Reed', specialty: 'Cardiology', date: 'November 15, 2023', time: '10:30 AM', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuACplVZt...' }
  ];





  showRegister()
  {
    this.registermode.set(true);
  }
}
