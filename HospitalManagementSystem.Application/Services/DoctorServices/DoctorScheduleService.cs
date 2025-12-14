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
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IHospitalRepository _hospitalRepository;

        public DoctorScheduleService(
            IDoctorRepository doctorRepository, 
            IDoctorScheduleRepository doctorScheduleRepository,
            IHospitalRepository hospitalRepository)
        {
            _doctorRepository = doctorRepository;
            _doctorScheduleRepository = doctorScheduleRepository;
            _hospitalRepository = hospitalRepository;
        }

        public async Task<IEnumerable<DoctorScheduleResponseDto>> GetAllAsync()
        {
            var records = await _doctorScheduleRepository.GetAllAsync();

            return records.Select(x => new DoctorScheduleResponseDto
            {
                ScheduleId = x.ScheduleId,
                ScheduleDate = x.ScheduleDate,
                DayOfWeek = x.DayOfWeek,
                IsRecurring = x.IsRecurring,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor?.Name ?? "",
                HospitalId = x.HospitalId,
                HospitalName = x.Hospital?.Name
            });
        }

        public async Task<DoctorScheduleResponseDto?> GetByIdAsync(Guid id)
        {
            var x = await _doctorScheduleRepository.GetByIdAsync(id);
            if (x == null) return null;

            return new DoctorScheduleResponseDto
            {
                ScheduleId = x.ScheduleId,
                ScheduleDate = x.ScheduleDate,
                DayOfWeek = x.DayOfWeek,
                IsRecurring = x.IsRecurring,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor?.Name ?? "",
                HospitalId = x.HospitalId,
                HospitalName = x.Hospital?.Name
            };
        }

        public async Task<IEnumerable<DoctorScheduleResponseDto>> GetByDoctorIdAsync(Guid doctorId)
        {
            var records = await _doctorScheduleRepository.GetByDoctorIdAsync(doctorId);

            var result = new List<DoctorScheduleResponseDto>();
            foreach (var x in records)
            {
                string? hospitalName = null;
                if (x.HospitalId.HasValue)
                {
                    var hospital = await _hospitalRepository.GetByIdAsync(x.HospitalId.Value);
                    hospitalName = hospital?.Name;
                }

                result.Add(new DoctorScheduleResponseDto
                {
                    ScheduleId = x.ScheduleId,
                    ScheduleDate = x.ScheduleDate,
                    DayOfWeek = x.DayOfWeek,
                    IsRecurring = x.IsRecurring,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor?.Name ?? "",
                    HospitalId = x.HospitalId,
                    HospitalName = hospitalName
                });
            }

            return result;
        }

        public async Task<DoctorScheduleResponseDto> CreateAsync(DoctorScheduleRequestDto doctorScheduleRequestDto)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorScheduleRequestDto.DoctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            string? hospitalName = null;
            if (doctorScheduleRequestDto.HospitalId != Guid.Empty)
            {
                var hospital = await _hospitalRepository.GetByIdAsync(doctorScheduleRequestDto.HospitalId);
                if (hospital == null)
                    throw new Exception("Hospital not found");
                hospitalName = hospital.Name;
            }

            // Validate: Check for overlapping schedules
            var existingSchedules = await _doctorScheduleRepository.GetByDoctorIdAsync(doctorScheduleRequestDto.DoctorId);
            
            foreach (var existing in existingSchedules)
            {
                // Check if same date
                if (existing.ScheduleDate.HasValue && doctorScheduleRequestDto.ScheduleDate.HasValue &&
                    existing.ScheduleDate.Value.Date == doctorScheduleRequestDto.ScheduleDate.Value.Date)
                {
                    // Check for time overlap
                    var newStart = TimeSpan.Parse(doctorScheduleRequestDto.StartTime);
                    var newEnd = TimeSpan.Parse(doctorScheduleRequestDto.EndTime);
                    var existingStart = TimeSpan.Parse(existing.StartTime);
                    var existingEnd = TimeSpan.Parse(existing.EndTime);

                    // Times overlap if: newStart < existingEnd AND newEnd > existingStart
                    if (newStart < existingEnd && newEnd > existingStart)
                    {
                        // Same hospital = duplicate entry
                        if (existing.HospitalId == doctorScheduleRequestDto.HospitalId)
                        {
                           throw new Exception($"You already have a schedule on <b>{doctorScheduleRequestDto.ScheduleDate.Value:MMM dd, yyyy}</b> from <b>{existing.StartTime}</b> to <b>{existing.EndTime}</b> at <b>this</b> hospital.");

                        }
                        // Different hospital = conflict (doctor can only be at one place)
                        else
                        {
                            throw new Exception(
                                $"Schedule conflict: You already have a schedule on <b>{doctorScheduleRequestDto.ScheduleDate.Value:MMM dd, yyyy}</b> " +
                                $"from <b>{existing.StartTime}</b> to <b>{existing.EndTime}</b> at <b>{existing.Hospital?.Name ?? "another hospital"}</b>. " +
                                "A doctor can only be at one hospital at a time."
                            );
                        }
                    }
                }
            }

            var entity = new DoctorSchedule
            {
                ScheduleId = Guid.NewGuid(),
                ScheduleDate = doctorScheduleRequestDto.ScheduleDate,
                DayOfWeek = doctorScheduleRequestDto.DayOfWeek ?? 
                           (doctorScheduleRequestDto.ScheduleDate?.DayOfWeek.ToString()),
                IsRecurring = doctorScheduleRequestDto.IsRecurring,
                StartTime = doctorScheduleRequestDto.StartTime,
                EndTime = doctorScheduleRequestDto.EndTime,
                DoctorId = doctorScheduleRequestDto.DoctorId,
                HospitalId = doctorScheduleRequestDto.HospitalId
            };

            await _doctorScheduleRepository.CreateAsync(entity);

            return new DoctorScheduleResponseDto
            {
                ScheduleId = entity.ScheduleId,
                ScheduleDate = entity.ScheduleDate,
                DayOfWeek = entity.DayOfWeek,
                IsRecurring = entity.IsRecurring,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                DoctorId = entity.DoctorId,
                DoctorName = doctor.Name,
                HospitalId = entity.HospitalId,
                HospitalName = hospitalName
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _doctorScheduleRepository.DeleteAsync(id);
        }
    }
}
