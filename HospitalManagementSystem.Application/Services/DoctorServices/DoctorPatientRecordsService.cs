using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using HospitalManagementSystem.Application.IServices;
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
    public class DoctorPatientRecordsService : IDoctorPatientRecordsService
    {
        private readonly IDoctorPatientRecordsRepository _doctorPatientRecordsRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public DoctorPatientRecordsService(
            IDoctorPatientRecordsRepository doctorPatientRecordsRepository,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _doctorPatientRecordsRepository = doctorPatientRecordsRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _emailService = emailService;
            _notificationService = notificationService;
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
                DoctorName = r.Doctor?.Name,
                PatientId = r.PatientId
            };
        }

        public async Task<IEnumerable<DoctorPatientRecordsResponseDto>> GetByPatientIdAsync(Guid patientId)
        {
            Console.WriteLine($"[DEBUG] GetByPatientIdAsync called with patientId: {patientId}");
            var list = await _doctorPatientRecordsRepository.GetByPatientIdAsync(patientId);
            Console.WriteLine($"[DEBUG] Found {list.Count()} records for patientId: {patientId}");
            
            var result = list.Select(r => new DoctorPatientRecordsResponseDto
            {
                RecordId = r.TreatmentId,
                Diagnosis = r.Diagnosis,
                Prescription = r.Prescription,
                Notes = r.Notes,
                VisitDate = r.VisitDate,
                DoctorId = r.DoctorId,
                DoctorName = r.Doctor?.Name,
                PatientId = r.PatientId,
                PatientName = r.Patient != null ? $"{r.Patient.FirstName} {r.Patient.LastName}" : null
            }).ToList();
            
            Console.WriteLine($"[DEBUG] Returning {result.Count} prescription DTOs");
            return result;
        }

        public async Task<IEnumerable<DoctorPatientRecordsResponseDto>> GetByDoctorIdAsync(Guid doctorId)
        {
            var list = await _doctorPatientRecordsRepository.GetByDoctorIdAsync(doctorId);
            return list.Select(r => new DoctorPatientRecordsResponseDto
            {
                RecordId = r.TreatmentId,
                Diagnosis = r.Diagnosis,
                Prescription = r.Prescription,
                Notes = r.Notes,
                VisitDate = r.VisitDate,
                DoctorId = r.DoctorId,
                DoctorName = r.Doctor?.Name,
                PatientId = r.PatientId,
                PatientName = r.Patient != null ? $"{r.Patient.FirstName} {r.Patient.LastName}" : null
            });
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

            // Send prescription email to patient (fire and forget)
            _ = SendPrescriptionEmailAsync(entity);

            // Create in-app notification (fire and forget)
            _ = CreatePrescriptionNotificationAsync(entity);

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

        private async Task SendPrescriptionEmailAsync(DoctorPatientRecords record)
        {
            try
            {
                var patient = await _patientRepository.GetPatientWithDetailsAsync(record.PatientId);
                var doctor = await _doctorRepository.GetByIdAsync(record.DoctorId);

                if (patient == null || doctor == null) return;

                var prescriptionEmail = new PrescriptionEmailDto
                {
                    PatientEmail = patient.ContactInfo?.EmailAddress ?? "",
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    DoctorName = doctor.Name,
                    DoctorSpecialization = doctor.Qualification ?? "",
                    HospitalName = "Hospital", // TODO: Get from appointment if available
                    VisitDate = record.VisitDate,
                    Diagnosis = record.Diagnosis,
                    Prescription = record.Prescription,
                    Notes = record.Notes
                };

                if (!string.IsNullOrEmpty(prescriptionEmail.PatientEmail))
                {
                    await _emailService.SendPrescriptionEmailAsync(prescriptionEmail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send prescription email: {ex.Message}");
            }
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

        private async Task CreatePrescriptionNotificationAsync(DoctorPatientRecords record)
        {
            try
            {
                var doctor = await _doctorRepository.GetByIdAsync(record.DoctorId);

                await _notificationService.CreatePrescriptionNotificationAsync(
                    record.PatientId,
                    doctor?.Name ?? "Doctor",
                    record.Diagnosis,
                    record.TreatmentId.ToString()
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create prescription notification: {ex.Message}");
            }
        }
    }
}
