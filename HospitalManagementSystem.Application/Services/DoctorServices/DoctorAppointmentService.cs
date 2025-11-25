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
    public class DoctorAppointmentService : IDoctorAppointmentService
    {
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;

        public DoctorAppointmentService(IDoctorAppointmentRepository doctorAppointmentRepository)
        {
            _doctorAppointmentRepository = doctorAppointmentRepository;
        }

        public async Task<IEnumerable<DoctorAppointmentResponseDto>> GetAllAsync()
        {
            var data = await _doctorAppointmentRepository.GetAllAsync();
            return data.Select(a => new DoctorAppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                AppointmentStatus = a.AppointmentStatus,
                CreatedDate = a.CreatedDate,
                PatientId = a.PatientId,
                DoctorId = a.DoctorId
            });
        }

        public async Task<DoctorAppointmentResponseDto?> GetByIdAsync(Guid id)
        {
            var a = await _doctorAppointmentRepository.GetByIdAsync(id);
            if (a == null) return null;

            return new DoctorAppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                AppointmentStatus = a.AppointmentStatus,
                CreatedDate = a.CreatedDate,
                PatientId = a.PatientId,
                DoctorId = a.DoctorId
            };
        }

        public async Task<DoctorAppointmentResponseDto> CreateAsync(DoctorAppointmentRequestDto doctorAppointmentRequestDto)
        {
            var entity = new DoctorAppointment
            {
                AppointmentId = Guid.NewGuid(),
                AppointmentDate = doctorAppointmentRequestDto.AppointmentDate,
                AppointmentTime = doctorAppointmentRequestDto.AppointmentTime,
                AppointmentStatus = doctorAppointmentRequestDto.AppointmentStatus,
                CreatedDate = doctorAppointmentRequestDto.CreatedDate,
                PatientId = doctorAppointmentRequestDto.PatientId,
                DoctorId = doctorAppointmentRequestDto.DoctorId
            };

            await _doctorAppointmentRepository.CreateAsync(entity);

            return new DoctorAppointmentResponseDto
            {
                AppointmentId = entity.AppointmentId,
                AppointmentDate = entity.AppointmentDate,
                AppointmentTime = entity.AppointmentTime,
                AppointmentStatus = entity.AppointmentStatus,
                CreatedDate = entity.CreatedDate,
                PatientId = entity.PatientId,
                DoctorId = entity.DoctorId
            };
        }

        public async Task<DoctorAppointmentResponseDto?> UpdateAsync(Guid id, DoctorAppointmentRequestDto doctorAppointmentRequestDto)
        {
            var entity = await _doctorAppointmentRepository.GetByIdAsync(id);
            if (entity == null) return null;

            entity.AppointmentDate = doctorAppointmentRequestDto.AppointmentDate;
            entity.AppointmentTime = doctorAppointmentRequestDto.AppointmentTime;
            entity.AppointmentStatus = doctorAppointmentRequestDto.AppointmentStatus;
            entity.CreatedDate = doctorAppointmentRequestDto.CreatedDate;
            entity.PatientId = doctorAppointmentRequestDto.PatientId;
            entity.DoctorId = doctorAppointmentRequestDto.DoctorId;

            await _doctorAppointmentRepository.UpdateAsync(entity);

            return new DoctorAppointmentResponseDto
            {
                AppointmentId = entity.AppointmentId,
                AppointmentDate = entity.AppointmentDate,
                AppointmentTime = entity.AppointmentTime,
                AppointmentStatus = entity.AppointmentStatus,
                CreatedDate = entity.CreatedDate,
                PatientId = entity.PatientId,
                DoctorId = entity.DoctorId
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _doctorAppointmentRepository.DeleteAsync(id);
        }
    }
}
