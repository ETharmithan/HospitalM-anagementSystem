using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Doctors;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repository.Doctor
{
    public class DoctorLeaveRepository : IDoctorLeaveRepository
    {
        private readonly AppDbContext _appDbContext;

        public DoctorLeaveRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<DoctorLeave>> GetAllAsync()
        {
            return await _appDbContext.DoctorLeaves.ToListAsync();
        }

        public async Task<DoctorLeave?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.DoctorLeaves.FindAsync(id);
        }

        public async Task<IEnumerable<DoctorLeave>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _appDbContext.DoctorLeaves
                .Where(x => x.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<DoctorLeave> CreateAsync(DoctorLeave doctorLeave)
        {
            await _appDbContext.DoctorLeaves.AddAsync(doctorLeave);
            await _appDbContext.SaveChangesAsync();
            return doctorLeave;
        }

        public async Task<DoctorLeave?> UpdateAsync(DoctorLeave doctorLeave)
        {
            _appDbContext.DoctorLeaves.Update(doctorLeave);
            await _appDbContext.SaveChangesAsync();
            return doctorLeave;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var data = await _appDbContext.DoctorLeaves.FindAsync(id);
            if (data == null) return false;

            _appDbContext.DoctorLeaves.Remove(data);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
