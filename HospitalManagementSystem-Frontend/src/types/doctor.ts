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
  appointmentStatus: string;
  createdDate: string;
  patientId: string;
  doctorId: string;
  doctor?: Doctor;
}

export interface CreateAppointmentRequest {
  appointmentDate: string; // ISO date string
  appointmentTime: string; // HH:mm format
  appointmentStatus: string;
  createdDate: string; // ISO date string
  patientId: string;
  doctorId: string;
}

export interface AppointmentSlot {
  time: string;
  available: boolean;
}

