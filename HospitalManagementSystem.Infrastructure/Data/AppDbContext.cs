using HospitalManagementSystem.Domain.Models.Doctor;
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
    }
}