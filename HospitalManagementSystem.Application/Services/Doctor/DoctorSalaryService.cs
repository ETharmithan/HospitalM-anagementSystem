using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using HospitalManagementSystem.Application.IServices.Doctor;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services.Doctor
{
    internal class DoctorSalaryService : IDoctorSalaryService
    {
        private readonly IDoctorSalaryRepository _doctorSalaryRepository;
        private readonly IDoctorRepository _doctorRepository;

        public DoctorSalaryService(IDoctorRepository doctorRepository, IDoctorSalaryRepository doctorSalaryRepository)
        {
            _doctorRepository = doctorRepository;
            _doctorSalaryRepository = doctorSalaryRepository;

        }

        public async Task<IEnumerable<DoctorSalaryResponseDto>> GetAllAsync()
        {
            var records = await _doctorSalaryRepository.GetAllAsync();

            return records.Select(x => new DoctorSalaryResponseDto
            {
                SalaryId = x.SalaryId,
                MonthlySalary = x.MonthlySalary,
                PaymentDate = x.PaymentDate,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor?.Name ?? ""
            });

        }

        public async Task<DoctorSalaryResponseDto?> GetByIdAsync(Guid id)
        {
            var x = await _doctorSalaryRepository.GetByIdAsync(id);
            if (x == null) return null;

            return new DoctorSalaryResponseDto
            {
                SalaryId = x.SalaryId,
                MonthlySalary = x.MonthlySalary,
                PaymentDate = x.PaymentDate,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor?.Name ?? ""
            };
        }

        public async Task<DoctorSalaryResponseDto> CreateAsync(DoctorSalaryRequestDto doctorSalaryRequestDto)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorSalaryRequestDto.DoctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            var entity = new DoctorSalary
            {
                SalaryId = Guid.NewGuid(),
                MonthlySalary = doctorSalaryRequestDto.MonthlySalary,
                PaymentDate = doctorSalaryRequestDto.PaymentDate,
                DoctorId = doctorSalaryRequestDto.DoctorId
            };

            await _doctorSalaryRepository.CreateAsync(entity);

            return new DoctorSalaryResponseDto
            {
                SalaryId = entity.SalaryId,
                MonthlySalary = entity.MonthlySalary,
                PaymentDate = entity.PaymentDate,
                DoctorId = entity.DoctorId,
                DoctorName = doctor.Name
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _doctorSalaryRepository.DeleteAsync(id);
        }
    }
}
