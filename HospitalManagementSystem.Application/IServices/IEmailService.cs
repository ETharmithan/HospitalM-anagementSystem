using System;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices
{
    public interface IEmailService
    {
        // OTP Verification
        Task<bool> SendOtpEmailAsync(string toEmail, string otp, string userName);
        string GenerateOtp();
        
        // Booking Notifications
        Task<bool> SendBookingConfirmationAsync(BookingEmailDto booking);
        Task<bool> SendBookingCancellationAsync(BookingEmailDto booking);
        Task<bool> SendBookingReminderAsync(BookingEmailDto booking);
        
        // Payment Notifications
        Task<bool> SendPaymentConfirmationAsync(PaymentEmailDto payment);
        Task<bool> SendPaymentReceiptAsync(PaymentEmailDto payment);
        
        // Prescription Notifications
        Task<bool> SendPrescriptionEmailAsync(PrescriptionEmailDto prescription);
        
        // Generic Email
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }

    public class PrescriptionEmailDto
    {
        public string PatientEmail { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialization { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Prescription { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
    }

    public class BookingEmailDto
    {
        public string PatientEmail { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialization { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalAddress { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public decimal? ConsultationFee { get; set; }
        public string? CancellationReason { get; set; }
    }

    public class PaymentEmailDto
    {
        public string PatientEmail { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "LKR";
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public string ReceiptUrl { get; set; } = string.Empty;
    }
}
