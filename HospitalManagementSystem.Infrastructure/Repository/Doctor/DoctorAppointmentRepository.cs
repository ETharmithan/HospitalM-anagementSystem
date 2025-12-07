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
            return await _appDbContext.DoctorAppointments
                .Include(x => x.Doctor)
                .Include(x => x.Patient)
                .Include(x => x.Hospital)
                .ToListAsync();
        }

        public async Task<DoctorAppointment?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.DoctorAppointments
                .Include(x => x.Doctor)
                .Include(x => x.Patient)
                .Include(x => x.Hospital)
                .FirstOrDefaultAsync(x => x.AppointmentId == id);
        }

        public async Task<IEnumerable<DoctorAppointment>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _appDbContext.DoctorAppointments
                .Include(x => x.Doctor)
                .Include(x => x.Patient)
                .Include(x => x.Hospital)
                .Where(x => x.DoctorId == doctorId)
                .OrderByDescending(x => x.AppointmentDate)
                .ThenBy(x => x.AppointmentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorAppointment>> GetByPatientIdAsync(Guid patientId)
        {
            return await _appDbContext.DoctorAppointments
                .Include(x => x.Doctor)
                .Include(x => x.Patient)
                .Include(x => x.Hospital)
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.AppointmentDate)
                .ThenBy(x => x.AppointmentTime)
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

        public async Task<int> CountAsync()
        {
            return await _appDbContext.DoctorAppointments.CountAsync();
        }
    }
}
