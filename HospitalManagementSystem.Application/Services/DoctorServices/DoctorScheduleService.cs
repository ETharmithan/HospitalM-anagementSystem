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
    internal class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;

        public DoctorScheduleService(IDoctorRepository doctorRepository, IDoctorScheduleRepository doctorScheduleRepository)
        {
            _doctorRepository = doctorRepository;
            _doctorScheduleRepository = doctorScheduleRepository;
        }

        public async Task<IEnumerable<DoctorScheduleResponseDto>> GetAllAsync()
        {
            var records = await _doctorScheduleRepository.GetAllAsync();

            return records.Select(x => new DoctorScheduleResponseDto
            {
                ScheduleId = x.ScheduleId,
                DayOfWeek = x.DayOfWeek,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor?.Name ?? ""
            });
        }

        public async Task<DoctorScheduleResponseDto?> GetByIdAsync(Guid id)
        {
            var x = await _doctorScheduleRepository.GetByIdAsync(id);
            if (x == null) return null;

            return new DoctorScheduleResponseDto
            {
                ScheduleId = x.ScheduleId,
                DayOfWeek = x.DayOfWeek,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor?.Name ?? ""
            };
        }
        public async Task<DoctorScheduleResponseDto> CreateAsync(DoctorScheduleRequestDto doctorScheduleRequestDto)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorScheduleRequestDto.DoctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            var entity = new DoctorSchedule
            {
                ScheduleId = Guid.NewGuid(),
                DayOfWeek = doctorScheduleRequestDto.DayOfWeek,
                StartTime = doctorScheduleRequestDto.StartTime,
                EndTime = doctorScheduleRequestDto.EndTime,
                DoctorId = doctorScheduleRequestDto.DoctorId,
            };

            await _doctorScheduleRepository.CreateAsync(entity);

            return new DoctorScheduleResponseDto
            {
                ScheduleId = entity.ScheduleId,
                DayOfWeek = entity.DayOfWeek,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                DoctorId = entity.DoctorId,
                DoctorName = doctor.Name
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _doctorScheduleRepository.DeleteAsync(id);
        }
    }
}
