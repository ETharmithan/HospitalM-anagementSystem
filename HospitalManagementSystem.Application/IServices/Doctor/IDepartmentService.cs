using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.Doctor
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentResponseDto>> GetAllAsync();
        Task<DepartmentResponseDto?> GetByIdAsync(Guid id);
        Task<DepartmentResponseDto> CreateAsync(DepartmentRequestDto departmentRequestDto);
        Task<bool> UpdateAsync(Guid id, DepartmentRequestDto departmentRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
