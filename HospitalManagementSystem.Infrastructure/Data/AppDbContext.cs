using HospitalManagementSystem.Domain.Models.Doctors;
﻿using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Domain.Models.Patient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<DoctorAppointment> DoctorAppointments { get; set; } = null!;
        public DbSet<DoctorLeave> DoctorLeaves { get; set; } = null!;
        public DbSet<DoctorPatientRecords> DoctorPatientRecords { get; set; } = null!;
        public DbSet<DoctorSalary> DoctorSalaries { get; set; } = null!;
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>()
            .HasMany(d => d.Doctors)
            .WithOne(d => d.Department)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doctor>()
            .HasMany(doctor => doctor.DoctorAppointments)
            .WithOne(doctor => doctor.Doctor)
            .HasForeignKey(doctor => doctor.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
            .HasMany(doctor => doctor.DoctorLeaves)
            .WithOne(doctor => doctor.Doctor)
            .HasForeignKey(doctor => doctor.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
            .HasMany(doctor => doctor.DoctorPatientRecords)
            .WithOne(doctor => doctor.Doctor)
            .HasForeignKey(doctor => doctor.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
            .HasMany(doctor => doctor.DoctorSalaries)
            .WithOne(doctor => doctor.Doctor)
            .HasForeignKey(doctor => doctor.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
            .HasMany(doctor => doctor.DoctorSchedules)
            .WithOne(doctor => doctor.Doctor)
            .HasForeignKey(doctor => doctor.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        }
        //public AppDbContext()
        //{
            
        //}
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Patient_Contact_Information> PatientContactInformation { get; set; }
        public DbSet<Patient_Identification_Details> PatientIdentificationDetails { get; set; }
        public DbSet<Patient_Medical_History> PatientMedicalHistory { get; set; }
        public DbSet<Patient_Medical_Related_Info> PatientMedicalRelatedInfo { get; set; }
        public DbSet<Patient_Emergency_Contact> PatientEmergencyContact { get; set; }
        public DbSet<Patient_Login_Info> PatientLoginInfo { get; set; }
    }
}