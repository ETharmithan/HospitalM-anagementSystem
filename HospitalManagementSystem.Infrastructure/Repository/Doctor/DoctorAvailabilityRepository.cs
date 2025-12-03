using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Doctors;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repository.Doctor
{
    public class DoctorAvailabilityRepository : IDoctorAvailabilityRepository
    {
        private readonly AppDbContext _appDbContext;

        public DoctorAvailabilityRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<DoctorAvailability>> GetAllAsync()
        {
            return await _appDbContext.DoctorAvailabilities
                .Include(x => x.Doctor)
                .ToListAsync();
        }

        public async Task<DoctorAvailability?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.DoctorAvailabilities
                .Include(x => x.Doctor)
                .FirstOrDefaultAsync(x => x.AvailabilityId == id);
        }

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _appDbContext.DoctorAvailabilities
                .Include(x => x.Doctor)
                .Where(x => x.DoctorId == doctorId)
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<DoctorAvailability?> GetByDoctorIdAndDateAsync(Guid doctorId, DateTime date)
        {
            var dateOnly = date.Date;
            return await _appDbContext.DoctorAvailabilities
                .Include(x => x.Doctor)
                .FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.Date.Date == dateOnly);
        }

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAndDateRangeAsync(Guid doctorId, DateTime startDate, DateTime endDate)
        {
            return await _appDbContext.DoctorAvailabilities
                .Include(x => x.Doctor)
                .Where(x => x.DoctorId == doctorId && x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date)
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<DoctorAvailability> CreateAsync(DoctorAvailability doctorAvailability)
        {
            await _appDbContext.DoctorAvailabilities.AddAsync(doctorAvailability);
            await _appDbContext.SaveChangesAsync();
            return doctorAvailability;
        }

        public async Task<DoctorAvailability> UpdateAsync(DoctorAvailability doctorAvailability)
        {
            doctorAvailability.ModifiedDate = DateTime.UtcNow;
            _appDbContext.DoctorAvailabilities.Update(doctorAvailability);
            await _appDbContext.SaveChangesAsync();
            return doctorAvailability;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var record = await _appDbContext.DoctorAvailabilities.FindAsync(id);
            if (record == null) return false;

            _appDbContext.DoctorAvailabilities.Remove(record);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}

