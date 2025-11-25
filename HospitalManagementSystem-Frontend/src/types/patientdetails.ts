export type PatientMedicalRelatedInfo = {
  patientId: string;
  bloodType: string;
  allergies: string;
  chronicConditions: string;
};
export type PatientDetails = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string; 
};


export type QuickAction = {
  icon: string;
  title: string;
  description: string;
  actionText: string;
}

export type Appointment = {
  doctorName: string;
  specialty: string;
  date: string;
  time: string;
  avatarUrl?: string;
}
