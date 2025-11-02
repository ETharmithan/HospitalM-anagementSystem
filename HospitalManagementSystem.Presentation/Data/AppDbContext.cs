using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Presentation.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
    }
}
