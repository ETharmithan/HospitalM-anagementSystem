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
            existingHospital.Latitude = hospital.Latitude;
            existingHospital.Longitude = hospital.Longitude;
            existingHospital.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingHospital;
        }

        public async Task<bool> DeleteHospitalAsync(Guid hospitalId)
        {
            var hospital = await _context.Hospitals
                .Include(h => h.HospitalAdmins)
                .Include(h => h.Departments)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId);
            
            if (hospital == null) return false;

            // Remove all appointments for this hospital
            var appointments = await _context.DoctorAppointments
                .Where(a => a.HospitalId == hospitalId)
                .ToListAsync();
            if (appointments.Any())
            {
                _context.DoctorAppointments.RemoveRange(appointments);
            }

            // Remove all doctor schedules for this hospital
            var schedules = await _context.DoctorSchedules
                .Where(s => s.HospitalId == hospitalId)
                .ToListAsync();
            if (schedules.Any())
            {
                _context.DoctorSchedules.RemoveRange(schedules);
            }

            // Remove all hospital admins
            if (hospital.HospitalAdmins != null && hospital.HospitalAdmins.Any())
            {
                _context.HospitalAdmins.RemoveRange(hospital.HospitalAdmins);
            }

            // Remove all departments (and their doctors will be handled by cascade or restrict)
            if (hospital.Departments != null && hospital.Departments.Any())
            {
                // Get all doctors in these departments
                var departmentIds = hospital.Departments.Select(d => d.DepartmentId).ToList();
                var doctors = await _context.Doctors
                    .Where(d => departmentIds.Contains(d.DepartmentId.Value))
                    .ToListAsync();
                
                // Set department to null for doctors instead of deleting them
                foreach (var doctor in doctors)
                {
                    doctor.DepartmentId = null;
                }

                _context.Departments.RemoveRange(hospital.Departments);
            }

            // Finally remove the hospital
            _context.Hospitals.Remove(hospital);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Department>> GetHospitalDepartmentsAsync(Guid hospitalId)
        {
            return await _context.Departments
                .Where(d => d.HospitalId == hospitalId && d.IsActive)
                .ToListAsync();
        }

        public async Task<Hospital> GetByIdAsync(Guid id)
        {
            return await _context.Hospitals
                .Include(h => h.HospitalAdmins)
                .ThenInclude(ha => ha.User)
                .FirstOrDefaultAsync(h => h.HospitalId == id);
        }

        public async Task<bool> AssignHospitalAdminAsync(Guid hospitalId, Guid userId)
        {
            // Check if already assigned
            var existing = await _context.HospitalAdmins
                .FirstOrDefaultAsync(ha => ha.HospitalId == hospitalId && ha.UserId == userId);
            
            if (existing != null)
                return false; // Already assigned

            var hospitalAdmin = new HospitalAdmin
            {
                HospitalAdminId = Guid.NewGuid(),
                HospitalId = hospitalId,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.HospitalAdmins.Add(hospitalAdmin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveHospitalAdminAsync(Guid hospitalId, Guid userId)
        {
            var hospitalAdmin = await _context.HospitalAdmins
                .FirstOrDefaultAsync(ha => ha.HospitalId == hospitalId && ha.UserId == userId);
            
            if (hospitalAdmin == null)
                return false;

            _context.HospitalAdmins.Remove(hospitalAdmin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserHospitalAdminAsync(Guid hospitalId, Guid userId)
        {
            return await _context.HospitalAdmins
                .AnyAsync(ha => ha.HospitalId == hospitalId && ha.UserId == userId && ha.IsActive);
        }
    }
}
