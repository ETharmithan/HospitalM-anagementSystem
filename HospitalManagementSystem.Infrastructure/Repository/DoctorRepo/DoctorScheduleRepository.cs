using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.DoctorModel;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repository.DoctorRepo
{
    internal class DoctorScheduleRepository : IDoctorScheduleRepository
    {
        private readonly AppDbContext _appDbContext;

        public DoctorScheduleRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<DoctorSchedule>> GetAllAsync()
        {
            return await _appDbContext.DoctorSchedules.Include(x => x.Doctor).ToListAsync();
        }

        public async Task<DoctorSchedule?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.DoctorSchedules.Include(x => x.Doctor).FirstOrDefaultAsync(x => x.ScheduleId == id);
        }

        public async Task<DoctorSchedule> CreateAsync(DoctorSchedule doctorSchedule)
        {
            _appDbContext.DoctorSchedules.Add(doctorSchedule);
            await _appDbContext.SaveChangesAsync();
            return doctorSchedule;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var record = await _appDbContext.DoctorSchedules.FindAsync(id);
            if (record == null) return false;

            _appDbContext.DoctorSchedules.Remove(record);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
