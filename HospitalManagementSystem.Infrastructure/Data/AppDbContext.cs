using HospitalManagementSystem.Domain.Models;
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
