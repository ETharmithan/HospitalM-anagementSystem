using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.IRepository;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public AdminDashboardService(
            IUserRepository userRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IDoctorAppointmentRepository doctorAppointmentRepository,
            IDepartmentRepository departmentRepository)
        {
            _userRepository = userRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _departmentRepository = departmentRepository;
        }

        public async Task<AdminOverviewDto> GetOverviewAsync(Guid? hospitalId = null)
        {
            // For now, return all data - hospital filtering will be done in controller
            // This maintains clean architecture (Application layer doesn't reference Infrastructure)
            var overview = new AdminOverviewDto
            {
                TotalUsers = await _userRepository.CountAsync(),
                TotalDoctors = await _doctorRepository.CountAsync(),
                TotalPatients = await _patientRepository.CountAsync(),
                TotalAppointments = await _doctorAppointmentRepository.CountAsync(),
                TotalDepartments = await _departmentRepository.CountAsync()
            };

            return overview;
        }
    }
}
