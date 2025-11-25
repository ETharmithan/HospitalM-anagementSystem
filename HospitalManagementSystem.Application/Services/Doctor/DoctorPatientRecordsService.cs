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
    internal class DoctorPatientRecordsService : IDoctorPatientRecordsService
    {
        private readonly IDoctorPatientRecordsRepository _doctorPatientRecordsRepository;

        public DoctorPatientRecordsService(IDoctorPatientRecordsRepository doctorPatientRecordsRepository)
        {
            _doctorPatientRecordsRepository = doctorPatientRecordsRepository;
        }

        public async Task<IEnumerable<DoctorPatientRecordsResponseDto>> GetAllAsync()
        {
            var list = await _doctorPatientRecordsRepository.GetAllAsync();
            return list.Select(r => new DoctorPatientRecordsResponseDto
            {
                RecordId = r.TreatmentId,
                Diagnosis = r.Diagnosis,
                Prescription = r.Prescription,
                Notes = r.Notes,
                VisitDate = r.VisitDate,
                DoctorId = r.DoctorId,
                PatientId = r.PatientId
            });
        }

        public async Task<DoctorPatientRecordsResponseDto?> GetByIdAsync(Guid id)
        {
            var r = await _doctorPatientRecordsRepository.GetByIdAsync(id);
            if (r == null) return null;

            return new DoctorPatientRecordsResponseDto
            {
                RecordId = r.TreatmentId,
                Diagnosis = r.Diagnosis,
                Prescription = r.Prescription,
                Notes = r.Notes,
                VisitDate = r.VisitDate,
                DoctorId = r.DoctorId,
                PatientId = r.PatientId
            };
        }

        public async Task<DoctorPatientRecordsResponseDto> CreateAsync(DoctorPatientRecordsRequestDto doctorPatientRecordsRequestDto)
        {
            var entity = new DoctorPatientRecords
            {
                TreatmentId = Guid.NewGuid(),
                Diagnosis = doctorPatientRecordsRequestDto.Diagnosis,
                Prescription = doctorPatientRecordsRequestDto.Prescription,
                Notes = doctorPatientRecordsRequestDto.Notes,
                VisitDate = doctorPatientRecordsRequestDto.VisitDate,
                DoctorId = doctorPatientRecordsRequestDto.DoctorId,
                PatientId = doctorPatientRecordsRequestDto.PatientId
            };

            await _doctorPatientRecordsRepository.CreateAsync(entity);

            return new DoctorPatientRecordsResponseDto
            {
                RecordId = entity.TreatmentId,
                Diagnosis = entity.Diagnosis,
                Prescription = entity.Prescription,
                Notes = entity.Notes,
                VisitDate = entity.VisitDate,
                DoctorId = entity.DoctorId,
                PatientId = entity.PatientId
            };
        }

        public async Task<DoctorPatientRecordsResponseDto?> UpdateAsync(Guid id, DoctorPatientRecordsRequestDto doctorPatientRecordsRequestDto)
        {
            var entity = await _doctorPatientRecordsRepository.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Diagnosis = doctorPatientRecordsRequestDto.Diagnosis;
            entity.Prescription = doctorPatientRecordsRequestDto.Prescription;
            entity.Notes = doctorPatientRecordsRequestDto.Notes;
            entity.VisitDate = doctorPatientRecordsRequestDto.VisitDate;
            entity.DoctorId = doctorPatientRecordsRequestDto.DoctorId;
            entity.PatientId = doctorPatientRecordsRequestDto.PatientId;

            await _doctorPatientRecordsRepository.UpdateAsync(entity);

            return new DoctorPatientRecordsResponseDto
            {
                RecordId = entity.TreatmentId,
                Diagnosis = entity.Diagnosis,
                Prescription = entity.Prescription,
                Notes = entity.Notes,
                VisitDate = entity.VisitDate,
                DoctorId = entity.DoctorId,
                PatientId = entity.PatientId
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _doctorPatientRecordsRepository.DeleteAsync(id);
        }
    }
}
