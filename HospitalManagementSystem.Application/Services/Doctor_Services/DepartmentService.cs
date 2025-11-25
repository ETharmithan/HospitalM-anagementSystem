using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using HospitalManagementSystem.Application.IServices.Doctor_IServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.DoctorModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services.Doctor_Services
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
                Name = d.Name
            });
        }

        public async Task<DepartmentResponseDto?> GetByIdAsync(Guid id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return null;

            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name
            };
        }

        public async Task<DepartmentResponseDto> CreateAsync(DepartmentRequestDto departmentRequestDto)
        {
            var department = new Department
            {
                Name = departmentRequestDto.Name
            };

            var Created = await _departmentRepository.CreateAsync(department);

            return new DepartmentResponseDto
            {
                DepartmentId = Created.DepartmentId,
                Name = Created.Name
            };
        }

        public async Task<bool> UpdateAsync(Guid id, DepartmentRequestDto departmentRequestDto)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                return false;

            department.Name = departmentRequestDto.Name;

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
