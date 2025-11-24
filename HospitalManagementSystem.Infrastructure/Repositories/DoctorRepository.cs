using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models.Doctor;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repositories
{
    public class DoctorRepository(AppDbContext dbContext) : GenericRepository<Doctors>(dbContext), IDoctorRepository
    {
        public async Task<Doctors> GetByNameAsync(string doctorName)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.DoctorName.ToLower() == doctorName.ToLower());
        }

        public async Task<IEnumerable<Doctors>> GetAllDoctorsAsync()
        {
            return await _dbSet.ToListAsync();
        }
    }
}
