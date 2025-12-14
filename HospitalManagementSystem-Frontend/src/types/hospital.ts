export interface Hospital {
  hospitalId: string;
  name: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  phoneNumber: string;
  email: string;
  website?: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  departments?: HospitalDepartment[];
}

export interface HospitalDepartment {
  departmentId: string;
  name: string;
  description?: string;
  headDoctor?: string;
}
