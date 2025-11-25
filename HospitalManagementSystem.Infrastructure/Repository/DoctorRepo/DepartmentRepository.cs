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
    internal class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _appDbContext;

        public DepartmentRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
            => await _appDbContext.Departments.ToListAsync();

        public async Task<Department?> GetByIdAsync(Guid id)
            => await _appDbContext.Departments.FindAsync(id);

        public async Task<Department> CreateAsync(Department department)
        {
            _appDbContext.Departments.Add(department);
            await _appDbContext.SaveChangesAsync();
            return department;
        }

        public async Task UpdateAsync(Department department)
        {
            _appDbContext.Departments.Update(department);
            await _appDbContext.SaveChangesAsync();
        }

        public Task DeleteAsync(Department department)
        {
            _appDbContext.Departments.Remove(department);
            return _appDbContext.SaveChangesAsync();
        }
    }
}
