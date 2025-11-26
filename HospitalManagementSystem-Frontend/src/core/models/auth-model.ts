export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone: string;
  userType: UserType;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiration: Date;
  userId: string;
  email: string;
  userType: string;
  firstName: string;
  lastName: string;
}

export enum UserType {
  Patient = 1,
  Doctor = 2,
  Staff = 3,
  Admin = 4,
  SuperAdmin = 5
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string;
  userType: UserType;
}
