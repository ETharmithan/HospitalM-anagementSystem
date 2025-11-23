using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using HospitalManagementSystem.Application.IServices.Doctor;
using HospitalManagementSystem.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services.Doctor
{
    internal class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;

        public DoctorService(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetAllAsync()
        {
            var doctors = await _doctorRepository.GetAllAsync();

            return doctors.Select(d => new DoctorResponseDto
            {
                DoctorId = d.DoctorId,
                Name = d.Name,
                Email = d.Email,
                Phone = d.Phone,
                Qualification = d.Qualification,
                LicenseNumber = d.LicenseNumber,
                Status = d.Status,
                DepartmentId = d.DepartmentId,
                DepartmentName = d.Department?.Name
            });
        }

        public async Task<DoctorResponseDto?> GetByIdAsync(Guid id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor == null) return null;

            return new DoctorResponseDto
            {
                DoctorId = doctor.DoctorId,
                Name = doctor.Name,
                Email = doctor.Email,
                Phone = doctor.Phone,
                Qualification = doctor.Qualification,
                LicenseNumber = doctor.LicenseNumber,
                Status = doctor.Status,
                DepartmentId = doctor.DepartmentId,
                DepartmentName = doctor.Department?.Name
            };
        }

        public async Task<DoctorResponseDto> CreateAsync(DoctorRequestDto doctorRequestDto)
        {
            var doctor = new Doctor
            {
                DoctorId = Guid.NewGuid(),
                Name = doctorRequestDto.Name,
                Email = doctorRequestDto.Email,
                Phone = doctorRequestDto.Phone,
                Qualification = doctorRequestDto.Qualification,
                LicenseNumber = doctorRequestDto.LicenseNumber,
                Status = doctorRequestDto.Status,
                DepartmentId = doctorRequestDto.DepartmentId
            };

            var created = await _doctorRepository.CreateAsync(doctor);

            return new DoctorResponseDto
            {
                DoctorId = created.DoctorId,
                Name = created.Name,
                Email = created.Email,
                Phone = created.Phone,
                Qualification = created.Qualification,
                LicenseNumber = created.LicenseNumber,
                Status = created.Status,
                DepartmentId = created.DepartmentId,
                DepartmentName = created.Department?.Name
            };
        }

        public async Task<DoctorResponseDto?> UpdateAsync(Guid id, DoctorRequestDto doctorRequestDto)
        {
            var existing = await _doctorRepository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Name = doctorRequestDto.Name;
            existing.Email = doctorRequestDto.Email;
            existing.Phone = doctorRequestDto.Phone;
            existing.Qualification = doctorRequestDto.Qualification;
            existing.LicenseNumber = doctorRequestDto.LicenseNumber;
            existing.Status = doctorRequestDto.Status;
            existing.DepartmentId = doctorRequestDto.DepartmentId;

            var updated = await _doctorRepository.UpdateAsync(existing);

            return new DoctorResponseDto
            {
                DoctorId = updated.DoctorId,
                Name = updated.Name,
                Email = updated.Email,
                Phone = updated.Phone,
                Qualification = updated.Qualification,
                LicenseNumber = updated.LicenseNumber,
                Status = updated.Status,
                DepartmentId = updated.DepartmentId,
                DepartmentName = updated.Department?.Name
            };

        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _doctorRepository.DeleteAsync(id);
        }
    }
}
