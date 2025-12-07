using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Doctors;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repositories
{
    public class DoctorRepository(AppDbContext dbContext) : GenericRepository<Doctor>(dbContext), IDoctorRepository
    {
        public async Task AddAsync(Doctor doctor)
        {
            await _dbSet.AddAsync(doctor);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Doctor> CreateAsync(Doctor doctor)
        {
            await _dbSet.AddAsync(doctor);
            await _dbContext.SaveChangesAsync();
            return doctor;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var doctor = await _dbSet.FindAsync(id);
            if (doctor == null) return false;

            _dbSet.Remove(doctor);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<Doctor> GetByNameAsync(string doctorName)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.Name.ToLower() == doctorName.ToLower());
        }

        public async Task<Doctor?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.Email.ToLower() == email.ToLower());
        }

        // public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
        // {
        //     return await _dbSet.ToListAsync();
        // }
        public async Task<Doctor?> UpdateAsync(Doctor doctor)
        {
            var existingDoctor = await _dbSet.FindAsync(doctor.DoctorId);
            if (existingDoctor == null) return null;

            _dbContext.Entry(existingDoctor).CurrentValues.SetValues(doctor);
            await _dbContext.SaveChangesAsync();
            return existingDoctor;
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }
    }
}
