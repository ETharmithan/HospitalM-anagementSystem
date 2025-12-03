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
    public class DoctorAppointmentRepository : IDoctorAppointmentRepository
    {
        private readonly AppDbContext _appDbContext;

        public DoctorAppointmentRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<DoctorAppointment>> GetAllAsync()
        {
            return await _appDbContext.DoctorAppointments.ToListAsync();
        }

        public async Task<DoctorAppointment?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.DoctorAppointments.FindAsync(id);
        }

        public async Task<IEnumerable<DoctorAppointment>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _appDbContext.DoctorAppointments
                .Where(x => x.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<DoctorAppointment> CreateAsync(DoctorAppointment doctorAppointment)
        {
            await _appDbContext.DoctorAppointments.AddAsync(doctorAppointment);
            await _appDbContext.SaveChangesAsync();
            return doctorAppointment;
        }

        public async Task<DoctorAppointment> UpdateAsync(DoctorAppointment doctorAppointment)
        {
            _appDbContext.DoctorAppointments.Update(doctorAppointment);
            await _appDbContext.SaveChangesAsync();
            return doctorAppointment;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _appDbContext.DoctorAppointments.FindAsync(id);
            if (entity == null) return false;

            _appDbContext.DoctorAppointments.Remove(entity);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
