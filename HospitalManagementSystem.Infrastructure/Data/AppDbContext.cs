using HospitalManagementSystem.Domain.Models.Doctors;
using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Domain.Models.Patient;
using HospitalManagementSystem.Domain.Models.Chat;
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
        public DbSet<Hospital> Hospitals { get; set; } = null!;
        public DbSet<HospitalAdmin> HospitalAdmins { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<DoctorAppointment> DoctorAppointments { get; set; } = null!;
        public DbSet<DoctorLeave> DoctorLeaves { get; set; } = null!;
        public DbSet<DoctorPatientRecords> DoctorPatientRecords { get; set; } = null!;
        public DbSet<EPrescription> EPrescriptions { get; set; } = null!;
        public DbSet<DoctorSalary> DoctorSalaries { get; set; } = null!;
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; } = null!;

        // --- Notifications ---
        public DbSet<Notification> Notifications { get; set; } = null!;

        // --- Patient Sets (Initialized to avoid warnings) ---
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Patient> Patients { get; set; } = default!;
        public DbSet<Patient_Contact_Information> PatientContactInformation { get; set; } = default!;
        public DbSet<Patient_Identification_Details> PatientIdentificationDetails { get; set; } = default!;
        public DbSet<Patient_Medical_History> PatientMedicalHistory { get; set; } = default!;
        public DbSet<Patient_Medical_Related_Info> PatientMedicalRelatedInfo { get; set; } = default!;
        public DbSet<Patient_Emergency_Contact> PatientEmergencyContact { get; set; } = default!;
        public DbSet<Patient_Login_Info> PatientLoginInfo { get; set; } = default!;

        // --- Chat Sets ---
        public DbSet<ChatSession> ChatSessions { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public DbSet<ChatRequest> ChatRequests { get; set; } = null!;
        public DbSet<DoctorChatAvailability> DoctorChatAvailabilities { get; set; } = null!;

       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Hospital>()
                .HasMany(h => h.Departments)
                .WithOne(d => d.Hospital)
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hospital>()
                .HasMany(h => h.HospitalAdmins)
                .WithOne(ha => ha.Hospital)
                .HasForeignKey(ha => ha.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HospitalAdmin>()
                .HasOne(ha => ha.User)
                .WithMany()
                .HasForeignKey(ha => ha.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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

           // Patient Records: DO NOT CASCADE DELETE. 
            // If a doctor is fired, the patient's medical history must remain.
            modelBuilder.Entity<Doctor>()
            .HasMany(doctor => doctor.DoctorPatientRecords)
            .WithOne(doctor => doctor.Doctor)
            .HasForeignKey(doctor => doctor.DoctorId)
            .OnDelete(DeleteBehavior.Restrict); 

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

            modelBuilder.Entity<DoctorSchedule>()
            .HasOne(ds => ds.Hospital)
            .WithMany()
            .HasForeignKey(ds => ds.HospitalId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Doctor>()
            .HasMany(doctor => doctor.DoctorAvailabilities)
            .WithOne(doctor => doctor.Doctor)
            .HasForeignKey(doctor => doctor.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorSalary>()
            .Property(ds => ds.MonthlySalary)
            .HasColumnType("decimal(18,2)");

            // Index for faster availability queries
            modelBuilder.Entity<DoctorAvailability>()
            .HasIndex(da => new { da.DoctorId, da.Date })
            .IsUnique();

            // --- Chat Configurations ---
            modelBuilder.Entity<ChatSession>()
                .HasMany(cs => cs.Messages)
                .WithOne(cm => cm.Session)
                .HasForeignKey(cm => cm.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Patient)
                .WithMany()
                .HasForeignKey(cs => cs.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Doctor)
                .WithMany()
                .HasForeignKey(cs => cs.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatRequest>()
                .HasOne(cr => cr.Patient)
                .WithMany()
                .HasForeignKey(cr => cr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatRequest>()
                .HasOne(cr => cr.Doctor)
                .WithMany()
                .HasForeignKey(cr => cr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorChatAvailability>()
                .HasOne(dca => dca.Doctor)
                .WithMany()
                .HasForeignKey(dca => dca.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorChatAvailability>()
                .HasIndex(dca => dca.DoctorId)
                .IsUnique();

        }
    }
}