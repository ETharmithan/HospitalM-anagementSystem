using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.DoctorModel;
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
        public async Task<Doctor> GetByNameAsync(string doctorName)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.DoctorName.ToLower() == doctorName.ToLower());
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
        {
            return await _dbSet.ToListAsync();
        }
    }
}
