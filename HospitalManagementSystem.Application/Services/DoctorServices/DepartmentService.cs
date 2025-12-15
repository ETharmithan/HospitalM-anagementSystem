using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services.DoctorServices
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();

            return departments.Select(d => new DepartmentResponseDto
            {
                DepartmentId = d.DepartmentId,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                HospitalId = d.HospitalId
            });
        }

        public async Task<DepartmentResponseDto?> GetByIdAsync(Guid id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return null;

            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name,
                Description = department.Description,
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt,
                HospitalId = department.HospitalId
            };
        }

        public async Task<DepartmentResponseDto> CreateAsync(DepartmentRequestDto departmentRequestDto)
        {
            if (!departmentRequestDto.HospitalId.HasValue)
                throw new ArgumentException("HospitalId is required to create a department");

            var department = new Department
            {
                Name = departmentRequestDto.Name,
                Description = departmentRequestDto.Description,
                HospitalId = departmentRequestDto.HospitalId.Value
            };

            var Created = await _departmentRepository.CreateAsync(department);

            return new DepartmentResponseDto
            {
                DepartmentId = Created.DepartmentId,
                Name = Created.Name,
                Description = Created.Description,
                IsActive = Created.IsActive,
                CreatedAt = Created.CreatedAt,
                UpdatedAt = Created.UpdatedAt,
                HospitalId = Created.HospitalId
            };
        }

        public async Task<bool> UpdateAsync(Guid id, DepartmentRequestDto departmentRequestDto)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                return false;

            department.Name = departmentRequestDto.Name;
            department.Description = departmentRequestDto.Description;
            department.UpdatedAt = DateTime.UtcNow;

            await _departmentRepository.UpdateAsync(department);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                return false;

            await _departmentRepository.DeleteAsync(department);
            return true;
        }
    }
}
