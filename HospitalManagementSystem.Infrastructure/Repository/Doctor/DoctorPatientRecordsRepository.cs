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
    public class DoctorPatientRecordsRepository : IDoctorPatientRecordsRepository
    {
        private readonly AppDbContext _appDbContext;

        public DoctorPatientRecordsRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<DoctorPatientRecords>> GetAllAsync()
        {
            return await _appDbContext.DoctorPatientRecords.ToListAsync();
        }

        public async Task<DoctorPatientRecords?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.DoctorPatientRecords.FindAsync(id);
        }

        public async Task<DoctorPatientRecords> CreateAsync(DoctorPatientRecords doctorPatientRecords)
        {
            await _appDbContext.DoctorPatientRecords.AddAsync(doctorPatientRecords);
            await _appDbContext.SaveChangesAsync();
            return doctorPatientRecords;
        }

        public async Task<DoctorPatientRecords?> UpdateAsync(DoctorPatientRecords doctorPatientRecords)
        {
            _appDbContext.DoctorPatientRecords.Update(doctorPatientRecords);
            await _appDbContext.SaveChangesAsync();
            return doctorPatientRecords;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _appDbContext.DoctorPatientRecords.FindAsync(id);
            if (entity == null) return false;

            _appDbContext.DoctorPatientRecords.Remove(entity);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
