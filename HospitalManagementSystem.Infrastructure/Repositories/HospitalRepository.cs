using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Domain.Models.Doctors;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repositories
{
    public class HospitalRepository : IHospitalRepository
    {
        private readonly AppDbContext _context;

        public HospitalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Hospital>> GetAllHospitalsAsync()
        {
            return await _context.Hospitals
                .Include(h => h.HospitalAdmins)
                .ThenInclude(ha => ha.User)
                .ToListAsync();
        }

        public async Task<Hospital?> GetHospitalByIdAsync(Guid hospitalId)
        {
            return await _context.Hospitals
                .Include(h => h.HospitalAdmins)
                .ThenInclude(ha => ha.User)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId);
        }

        public async Task<Hospital> CreateHospitalAsync(Hospital hospital)
        {
            hospital.HospitalId = Guid.NewGuid();
            hospital.IsActive = true;
            hospital.CreatedAt = DateTime.UtcNow;
            hospital.UpdatedAt = DateTime.UtcNow;

            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            return hospital;
        }

        public async Task<Hospital?> UpdateHospitalAsync(Guid hospitalId, Hospital hospital)
        {
            var existingHospital = await _context.Hospitals.FindAsync(hospitalId);
            if (existingHospital == null) return null;

            existingHospital.Name = hospital.Name;
            existingHospital.Address = hospital.Address;
            existingHospital.City = hospital.City;
            existingHospital.State = hospital.State;
            existingHospital.Country = hospital.Country;
            existingHospital.PostalCode = hospital.PostalCode;
            existingHospital.PhoneNumber = hospital.PhoneNumber;
            existingHospital.Email = hospital.Email;
            existingHospital.Website = hospital.Website;
            existingHospital.Description = hospital.Description;
            existingHospital.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingHospital;
        }

        public async Task<bool> DeleteHospitalAsync(Guid hospitalId)
        {
            var hospital = await _context.Hospitals.FindAsync(hospitalId);
            if (hospital == null) return false;

            hospital.IsActive = false;
            hospital.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Department>> GetHospitalDepartmentsAsync(Guid hospitalId)
        {
            return await _context.Departments
                .Where(d => d.HospitalId == hospitalId && d.IsActive)
                .ToListAsync();
        }
    }
}
