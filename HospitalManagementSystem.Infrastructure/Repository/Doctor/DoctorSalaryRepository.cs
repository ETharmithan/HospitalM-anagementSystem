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
    public class DoctorSalaryRepository : IDoctorSalaryRepository
    {
        private readonly AppDbContext _appDbContext;

        public DoctorSalaryRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<DoctorSalary>> GetAllAsync()
        {
            return await _appDbContext.DoctorSalaries.Include(x => x.Doctor).ToListAsync();
        }

        public async Task<DoctorSalary?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.DoctorSalaries.Include(x => x.Doctor).FirstOrDefaultAsync(x => x.SalaryId == id);
        }
        public async Task<DoctorSalary> CreateAsync(DoctorSalary doctorSalary)
        {
            _appDbContext.DoctorSalaries.Add(doctorSalary);
            await _appDbContext.SaveChangesAsync();
            return doctorSalary;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var record = await _appDbContext.DoctorSalaries.FindAsync(id);
            if (record == null) return false;

            _appDbContext.DoctorSalaries.Remove(record);
            await _appDbContext.SaveChangesAsync();
            return true;
        }


    }
}
