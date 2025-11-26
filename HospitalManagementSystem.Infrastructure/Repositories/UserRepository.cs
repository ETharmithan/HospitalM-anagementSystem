using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext dbContext) : GenericRepository<User>(dbContext), IUserRepository
    {
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
        }
    }
}
