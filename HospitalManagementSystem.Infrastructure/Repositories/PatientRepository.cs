using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models.Patient;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repositories
{
    public class PatientRepository(AppDbContext dbContext) : GenericRepository<Patient>(dbContext), IPatientRepository
    {
        public async Task<Patient> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Patient> GetPatientWithDetailsAsync(Guid patientId)
        {
            return await _dbSet
                .Include(p => p.ContactInfo)
                .Include(p => p.IdentificationDetails)
                .Include(p => p.MedicalHistory)
                .Include(p => p.MedicalRelatedInfo)
                .Include(p => p.EmergencyContact)
                .Include(p => p.LoginInfo)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            return await _dbSet
                .Include(p => p.ContactInfo)
                .Include(p => p.IdentificationDetails)
                .ToListAsync();
        }

        public async Task<IEnumerable<Patient>> GetPatientsByGenderAsync(string gender)
        {
            return await _dbSet
                .Where(p => p.Gender.ToLower() == gender.ToLower())
                .ToListAsync();
        }
    }
}
