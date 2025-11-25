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
    public class DoctorLeaveService : IDoctorLeaveService
    {
        private readonly IDoctorLeaveRepository _doctorleaveRepository;

        public DoctorLeaveService(IDoctorLeaveRepository doctorleaveRepository)
        {
            _doctorleaveRepository = doctorleaveRepository;
        }

        public async Task<IEnumerable<DoctorLeaveResponseDto>> GetAllAsync()
        {
            var leaves = await _doctorleaveRepository.GetAllAsync();
            return leaves.Select(l => new DoctorLeaveResponseDto
            {
                LeaveId = l.LeaveId,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                Status = l.Status,
                DoctorId = l.DoctorId
            });
        }

        public async Task<DoctorLeaveResponseDto?> GetByIdAsync(Guid id)
        {
            var leave = await _doctorleaveRepository.GetByIdAsync(id);
            if (leave == null) return null;
            return new DoctorLeaveResponseDto
            {
                LeaveId = leave.LeaveId,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status,
                DoctorId = leave.DoctorId
            };
        }

        public async Task<DoctorLeaveResponseDto> CreateAsync(DoctorLeaveRequestDto doctorLeaveRequestDto)
        {
            var entity = new DoctorLeave
            {
                LeaveId = Guid.NewGuid(),
                StartDate = doctorLeaveRequestDto.StartDate,
                EndDate = doctorLeaveRequestDto.EndDate,
                Reason = doctorLeaveRequestDto.Reason,
                Status = doctorLeaveRequestDto.Status,
                DoctorId = doctorLeaveRequestDto.DoctorId
            };

            await _doctorleaveRepository.CreateAsync(entity);
            return new DoctorLeaveResponseDto
            {
                LeaveId = entity.LeaveId,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Reason = entity.Reason,
                Status = entity.Status,
                DoctorId = entity.DoctorId
            };
        }

        public async Task<DoctorLeaveResponseDto?> UpdateAsync(Guid id, DoctorLeaveRequestDto doctorLeaveRequestDto)
        {
            var entity = await _doctorleaveRepository.GetByIdAsync(id);
            if (entity == null) return null;

            entity.StartDate = doctorLeaveRequestDto.StartDate;
            entity.EndDate = doctorLeaveRequestDto.EndDate;
            entity.Reason = doctorLeaveRequestDto.Reason;
            entity.Status = doctorLeaveRequestDto.Status;
            entity.DoctorId = doctorLeaveRequestDto.DoctorId;

            await _doctorleaveRepository.UpdateAsync(entity);

            return new DoctorLeaveResponseDto
            {
                LeaveId = entity.LeaveId,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Reason = entity.Reason,
                Status = entity.Status,
                DoctorId = entity.DoctorId
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _doctorleaveRepository.DeleteAsync(id);
        }
    }
}
