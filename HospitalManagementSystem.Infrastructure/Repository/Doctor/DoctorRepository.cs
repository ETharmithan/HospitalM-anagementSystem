using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repository.Doctor
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _appDbContext;

        public DoctorRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await _appDbContext.Doctors
                                            .Include(d => d.Department)
                                            .ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.Doctors
                                            .Include(d => d.Department)
                                            .FirstOrDefaultAsync(d => d.DoctorId == id);
        }

        public async Task<Doctor> CreateAsync(Doctor doctor)
        {
            await _appDbContext.Doctors.AddAsync(doctor);
            await _appDbContext.SaveChangesAsync();
            return doctor;
        }

        public async Task<Doctor?> UpdateAsync(Doctor doctor)
        {
            var existing = await _appDbContext.Doctors.FindAsync(doctor.DoctorId);
            if (existing == null) return null;

            _appDbContext.Entry(existing).CurrentValues.SetValues(doctor);
            await _appDbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var doctor = await _appDbContext.Doctors.FindAsync(id);
            if (doctor == null) return false;

            _appDbContext.Doctors.Remove(doctor);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
