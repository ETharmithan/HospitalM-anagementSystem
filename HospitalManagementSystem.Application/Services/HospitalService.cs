using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Response_Dto;
using DepartmentResponseDto = HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto.DepartmentResponseDto;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models;

namespace HospitalManagementSystem.Application.Services
{
    public class HospitalService : IHospitalService
    {
        private readonly IHospitalRepository _hospitalRepository;

        public HospitalService(IHospitalRepository hospitalRepository)
        {
            _hospitalRepository = hospitalRepository;
        }

        public async Task<List<HospitalResponseDto>> GetAllHospitalsAsync()
        {
            var hospitals = await _hospitalRepository.GetAllHospitalsAsync();
            return hospitals.Select(MapToHospitalResponseDto).ToList();
        }

        public async Task<HospitalResponseDto?> GetHospitalByIdAsync(Guid hospitalId)
        {
            var hospital = await _hospitalRepository.GetHospitalByIdAsync(hospitalId);
            return hospital != null ? MapToHospitalResponseDto(hospital) : null;
        }

        public async Task<HospitalResponseDto> CreateHospitalAsync(HospitalRequestDto hospitalDto)
        {
            var hospital = new Hospital
            {
                Name = hospitalDto.Name,
                Address = hospitalDto.Address,
                City = hospitalDto.City,
                State = hospitalDto.State,
                Country = hospitalDto.Country,
                PostalCode = hospitalDto.PostalCode,
                PhoneNumber = hospitalDto.PhoneNumber,
                Email = hospitalDto.Email,
                Website = hospitalDto.Website,
                Description = hospitalDto.Description
            };

            var result = await _hospitalRepository.CreateHospitalAsync(hospital);
            return MapToHospitalResponseDto(result);
        }

        public async Task<HospitalResponseDto?> UpdateHospitalAsync(Guid hospitalId, HospitalRequestDto hospitalDto)
        {
            var hospital = new Hospital
            {
                Name = hospitalDto.Name,
                Address = hospitalDto.Address,
                City = hospitalDto.City,
                State = hospitalDto.State,
                Country = hospitalDto.Country,
                PostalCode = hospitalDto.PostalCode,
                PhoneNumber = hospitalDto.PhoneNumber,
                Email = hospitalDto.Email,
                Website = hospitalDto.Website,
                Description = hospitalDto.Description
            };

            var result = await _hospitalRepository.UpdateHospitalAsync(hospitalId, hospital);
            return result != null ? MapToHospitalResponseDto(result) : null;
        }

        public async Task<bool> DeleteHospitalAsync(Guid hospitalId)
        {
            return await _hospitalRepository.DeleteHospitalAsync(hospitalId);
        }

        public async Task<List<DepartmentResponseDto>> GetHospitalDepartmentsAsync(Guid hospitalId)
        {
            var departments = await _hospitalRepository.GetHospitalDepartmentsAsync(hospitalId);
            return departments.Select(d => new DepartmentResponseDto
            {
                DepartmentId = d.DepartmentId,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                HospitalId = d.HospitalId
            }).ToList();
        }

        private HospitalResponseDto MapToHospitalResponseDto(Hospital hospital)
        {
            return new HospitalResponseDto
            {
                HospitalId = hospital.HospitalId,
                Name = hospital.Name,
                Address = hospital.Address,
                City = hospital.City,
                State = hospital.State,
                Country = hospital.Country,
                PostalCode = hospital.PostalCode,
                PhoneNumber = hospital.PhoneNumber,
                Email = hospital.Email,
                Website = hospital.Website,
                Description = hospital.Description,
                IsActive = hospital.IsActive,
                CreatedAt = hospital.CreatedAt,
                UpdatedAt = hospital.UpdatedAt,
                HospitalAdmins = hospital.HospitalAdmins.Select(ha => new HospitalAdminResponseDto
                {
                    HospitalAdminId = ha.HospitalAdminId,
                    HospitalId = ha.HospitalId,
                    UserId = ha.UserId,
                    UserName = ha.User?.Username,
                    UserEmail = ha.User?.Email,
                    IsActive = ha.IsActive,
                    CreatedAt = ha.CreatedAt,
                    UpdatedAt = ha.UpdatedAt
                }).ToList()
            };
        }
    }
}
