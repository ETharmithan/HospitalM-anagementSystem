export interface Doctor {
  doctorId: string;
  name: string;
  email: string;
  phone: string;
  qualification: string;
  licenseNumber: string;
  status: string;
  departmentId?: string;
  departmentName?: string;
}

export interface Department {
  departmentId: string;
  name: string;
}

export interface Appointment {
  appointmentId: string;
  appointmentDate: string;
  appointmentTime: string;
  appointmentEndTime?: string;
  appointmentStatus: string;
  createdDate: string;
  durationMinutes?: number;
  patientId: string;
  doctorId: string;
  hospitalId?: string;
  doctor?: Doctor;
  doctorName?: string;
  patientName?: string;
  hospitalName?: string;
}

export interface CreateAppointmentRequest {
  appointmentDate: string; // ISO date string
  appointmentTime: string; // HH:mm format
  appointmentStatus: string;
  createdDate: string; // ISO date string
  patientId: string;
  doctorId: string;
  hospitalId?: string;
  durationMinutes?: number;
}

export interface AppointmentSlot {
  time: string;
  available: boolean;
}

