using HospitalManagementSystem.Application.IServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _appPassword;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
            _senderName = _configuration["EmailSettings:SenderName"] ?? "Hospital Management System";
            _appPassword = _configuration["EmailSettings:AppPassword"] ?? "";
        }

        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp, string userName)
        {
            var subject = "Email Verification - Hospital Management System";
            var body = GetOtpEmailTemplate(otp, userName);
            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_senderEmail, _appPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, _senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        // ==================== BOOKING EMAILS ====================

        public async Task<bool> SendBookingConfirmationAsync(BookingEmailDto booking)
        {
            var subject = $"Appointment Confirmed - {booking.BookingReference}";
            var body = GetBookingConfirmationTemplate(booking);
            return await SendEmailAsync(booking.PatientEmail, subject, body);
        }

        public async Task<bool> SendBookingCancellationAsync(BookingEmailDto booking)
        {
            var subject = $"Appointment Cancelled - {booking.BookingReference}";
            var body = GetBookingCancellationTemplate(booking);
            return await SendEmailAsync(booking.PatientEmail, subject, body);
        }

        public async Task<bool> SendBookingReminderAsync(BookingEmailDto booking)
        {
            var subject = $"Appointment Reminder - Tomorrow at {booking.AppointmentTime}";
            var body = GetBookingReminderTemplate(booking);
            return await SendEmailAsync(booking.PatientEmail, subject, body);
        }

        // ==================== PAYMENT EMAILS ====================

        public async Task<bool> SendPaymentConfirmationAsync(PaymentEmailDto payment)
        {
            var subject = $"Payment Confirmed - {payment.TransactionId}";
            var body = GetPaymentConfirmationTemplate(payment);
            return await SendEmailAsync(payment.PatientEmail, subject, body);
        }

        public async Task<bool> SendPaymentReceiptAsync(PaymentEmailDto payment)
        {
            var subject = $"Payment Receipt - {payment.TransactionId}";
            var body = GetPaymentReceiptTemplate(payment);
            return await SendEmailAsync(payment.PatientEmail, subject, body);
        }

        // ==================== EMAIL TEMPLATES ====================

        private string GetOtpEmailTemplate(string otp, string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .otp-box {{ background: #667eea; color: white; font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 20px 40px; text-align: center; border-radius: 10px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .warning {{ color: #e74c3c; font-size: 14px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🏥 Hospital Management System</h1>
            <p>Email Verification</p>
        </div>
        <div class='content'>
            <h2>Hello {userName}!</h2>
            <p>Thank you for registering with our Hospital Management System. To complete your registration, please use the following OTP code:</p>
            
            <div class='otp-box'>{otp}</div>
            
            <p>This code will expire in <strong>10 minutes</strong>.</p>
            
            <p class='warning'>⚠️ If you didn't request this verification, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Hospital Management System. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetBookingConfirmationTemplate(BookingEmailDto booking)
        {
            var feeSection = booking.ConsultationFee.HasValue 
                ? $"<tr><td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>Consultation Fee</strong></td><td style='padding: 10px; border-bottom: 1px solid #eee;'>LKR {booking.ConsultationFee:N2}</td></tr>" 
                : "";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .booking-details {{ background: white; border-radius: 10px; padding: 20px; margin: 20px 0; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .booking-ref {{ background: #28a745; color: white; padding: 15px; text-align: center; border-radius: 8px; font-size: 18px; font-weight: bold; margin-bottom: 20px; }}
        .detail-table {{ width: 100%; border-collapse: collapse; }}
        .detail-table td {{ padding: 10px; border-bottom: 1px solid #eee; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .success-icon {{ font-size: 48px; margin-bottom: 10px; }}
        .reminder {{ background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 8px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='success-icon'>✅</div>
            <h1>Appointment Confirmed!</h1>
            <p>Your booking has been successfully confirmed</p>
        </div>
        <div class='content'>
            <h2>Hello {booking.PatientName}!</h2>
            <p>Great news! Your appointment has been confirmed. Here are your booking details:</p>
            
            <div class='booking-details'>
                <div class='booking-ref'>Booking Reference: {booking.BookingReference}</div>
                
                <table class='detail-table'>
                    <tr>
                        <td><strong>📅 Date</strong></td>
                        <td>{booking.AppointmentDate:dddd, MMMM dd, yyyy}</td>
                    </tr>
                    <tr>
                        <td><strong>🕐 Time</strong></td>
                        <td>{booking.AppointmentTime}</td>
                    </tr>
                    <tr>
                        <td><strong>👨‍⚕️ Doctor</strong></td>
                        <td>Dr. {booking.DoctorName}</td>
                    </tr>
                    <tr>
                        <td><strong>🏥 Hospital</strong></td>
                        <td>{booking.HospitalName}</td>
                    </tr>
                    <tr>
                        <td><strong>📍 Address</strong></td>
                        <td>{booking.HospitalAddress}</td>
                    </tr>
                    {feeSection}
                </table>
            </div>
            
            <div class='reminder'>
                <strong>📌 Important Reminders:</strong>
                <ul>
                    <li>Please arrive 15 minutes before your appointment time</li>
                    <li>Bring your ID and any relevant medical records</li>
                    <li>If you need to cancel, please do so at least 24 hours in advance</li>
                </ul>
            </div>
        </div>
        <div class='footer'>
            <p>© 2024 Hospital Management System. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetBookingCancellationTemplate(BookingEmailDto booking)
        {
            var reasonSection = !string.IsNullOrEmpty(booking.CancellationReason)
                ? $"<p><strong>Reason:</strong> {booking.CancellationReason}</p>"
                : "";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .booking-details {{ background: white; border-radius: 10px; padding: 20px; margin: 20px 0; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .cancelled-ref {{ background: #dc3545; color: white; padding: 15px; text-align: center; border-radius: 8px; font-size: 18px; font-weight: bold; margin-bottom: 20px; text-decoration: line-through; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .cancel-icon {{ font-size: 48px; margin-bottom: 10px; }}
        .rebook {{ background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; display: inline-block; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='cancel-icon'>❌</div>
            <h1>Appointment Cancelled</h1>
            <p>Your booking has been cancelled</p>
        </div>
        <div class='content'>
            <h2>Hello {booking.PatientName},</h2>
            <p>Your appointment has been cancelled. Here are the details of the cancelled booking:</p>
            
            <div class='booking-details'>
                <div class='cancelled-ref'>{booking.BookingReference}</div>
                
                <p><strong>📅 Date:</strong> {booking.AppointmentDate:dddd, MMMM dd, yyyy}</p>
                <p><strong>🕐 Time:</strong> {booking.AppointmentTime}</p>
                <p><strong>👨‍⚕️ Doctor:</strong> Dr. {booking.DoctorName}</p>
                <p><strong>🏥 Hospital:</strong> {booking.HospitalName}</p>
                {reasonSection}
            </div>
            
            <p>If you would like to reschedule, please visit our website or contact us.</p>
            
            <center>
                <a href='http://localhost:4200/doctors' class='rebook'>Book New Appointment</a>
            </center>
        </div>
        <div class='footer'>
            <p>© 2024 Hospital Management System. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetBookingReminderTemplate(BookingEmailDto booking)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%); color: #333; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .booking-details {{ background: white; border-radius: 10px; padding: 20px; margin: 20px 0; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .reminder-box {{ background: #fff3cd; border: 2px solid #ffc107; padding: 20px; text-align: center; border-radius: 10px; margin: 20px 0; }}
        .time-highlight {{ font-size: 28px; font-weight: bold; color: #ff9800; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .bell-icon {{ font-size: 48px; margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='bell-icon'>🔔</div>
            <h1>Appointment Reminder</h1>
            <p>Your appointment is tomorrow!</p>
        </div>
        <div class='content'>
            <h2>Hello {booking.PatientName}!</h2>
            <p>This is a friendly reminder about your upcoming appointment:</p>
            
            <div class='reminder-box'>
                <p>📅 <strong>Tomorrow</strong></p>
                <p class='time-highlight'>{booking.AppointmentTime}</p>
            </div>
            
            <div class='booking-details'>
                <p><strong>📅 Date:</strong> {booking.AppointmentDate:dddd, MMMM dd, yyyy}</p>
                <p><strong>👨‍⚕️ Doctor:</strong> Dr. {booking.DoctorName}</p>
                <p><strong>🏥 Hospital:</strong> {booking.HospitalName}</p>
                <p><strong>📍 Address:</strong> {booking.HospitalAddress}</p>
                <p><strong>🎫 Reference:</strong> {booking.BookingReference}</p>
            </div>
            
            <p><strong>📌 Don't forget to:</strong></p>
            <ul>
                <li>Arrive 15 minutes early</li>
                <li>Bring your ID and medical records</li>
                <li>Wear a mask if required</li>
            </ul>
        </div>
        <div class='footer'>
            <p>© 2024 Hospital Management System. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPaymentConfirmationTemplate(PaymentEmailDto payment)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .payment-details {{ background: white; border-radius: 10px; padding: 20px; margin: 20px 0; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .amount-box {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 25px; text-align: center; border-radius: 10px; margin: 20px 0; }}
        .amount {{ font-size: 36px; font-weight: bold; }}
        .transaction-id {{ background: #e9ecef; padding: 10px; border-radius: 5px; font-family: monospace; text-align: center; margin: 15px 0; }}
        .detail-table {{ width: 100%; border-collapse: collapse; }}
        .detail-table td {{ padding: 12px; border-bottom: 1px solid #eee; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .success-icon {{ font-size: 48px; margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='success-icon'>💳 ✅</div>
            <h1>Payment Successful!</h1>
            <p>Your payment has been processed</p>
        </div>
        <div class='content'>
            <h2>Hello {payment.PatientName}!</h2>
            <p>Thank you for your payment. Your transaction has been completed successfully.</p>
            
            <div class='amount-box'>
                <p style='margin: 0; font-size: 14px;'>Amount Paid</p>
                <p class='amount'>{payment.Currency} {payment.Amount:N2}</p>
            </div>
            
            <div class='payment-details'>
                <div class='transaction-id'>
                    Transaction ID: <strong>{payment.TransactionId}</strong>
                </div>
                
                <table class='detail-table'>
                    <tr>
                        <td><strong>📅 Payment Date</strong></td>
                        <td>{payment.PaymentDate:MMMM dd, yyyy HH:mm}</td>
                    </tr>
                    <tr>
                        <td><strong>💳 Payment Method</strong></td>
                        <td>{payment.PaymentMethod}</td>
                    </tr>
                    <tr>
                        <td><strong>👨‍⚕️ Doctor</strong></td>
                        <td>Dr. {payment.DoctorName}</td>
                    </tr>
                    <tr>
                        <td><strong>📅 Appointment Date</strong></td>
                        <td>{payment.AppointmentDate:dddd, MMMM dd, yyyy}</td>
                    </tr>
                    <tr>
                        <td><strong>🕐 Appointment Time</strong></td>
                        <td>{payment.AppointmentTime}</td>
                    </tr>
                    <tr>
                        <td><strong>🏥 Hospital</strong></td>
                        <td>{payment.HospitalName}</td>
                    </tr>
                </table>
            </div>
            
            <p style='text-align: center; color: #666;'>A receipt has been sent to your email. Please keep this for your records.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Hospital Management System. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPaymentReceiptTemplate(PaymentEmailDto payment)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .receipt {{ background: white; border: 1px solid #ddd; border-radius: 10px; padding: 30px; }}
        .receipt-header {{ text-align: center; border-bottom: 2px solid #667eea; padding-bottom: 20px; margin-bottom: 20px; }}
        .receipt-header h1 {{ color: #667eea; margin: 0; }}
        .receipt-header p {{ color: #666; margin: 5px 0 0; }}
        .receipt-number {{ background: #f8f9fa; padding: 10px; text-align: center; border-radius: 5px; margin: 15px 0; font-family: monospace; }}
        .detail-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #eee; }}
        .detail-label {{ color: #666; }}
        .detail-value {{ font-weight: 600; }}
        .total-row {{ background: #667eea; color: white; padding: 15px; margin: 20px -30px; display: flex; justify-content: space-between; font-size: 18px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; padding-top: 20px; border-top: 1px solid #eee; }}
        .stamp {{ text-align: center; margin: 20px 0; }}
        .stamp-text {{ display: inline-block; border: 3px solid #28a745; color: #28a745; padding: 10px 20px; border-radius: 5px; font-weight: bold; transform: rotate(-5deg); }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='receipt'>
            <div class='receipt-header'>
                <h1>🏥 Hospital Management System</h1>
                <p>Official Payment Receipt</p>
            </div>
            
            <div class='receipt-number'>
                Receipt No: <strong>{payment.PaymentId}</strong>
            </div>
            
            <div class='stamp'>
                <span class='stamp-text'>✓ PAID</span>
            </div>
            
            <div style='margin: 20px 0;'>
                <div class='detail-row'>
                    <span class='detail-label'>Patient Name</span>
                    <span class='detail-value'>{payment.PatientName}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Transaction ID</span>
                    <span class='detail-value'>{payment.TransactionId}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Payment Date</span>
                    <span class='detail-value'>{payment.PaymentDate:MMMM dd, yyyy HH:mm}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Payment Method</span>
                    <span class='detail-value'>{payment.PaymentMethod}</span>
                </div>
            </div>
            
            <div style='background: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                <h3 style='margin: 0 0 10px; color: #667eea;'>Appointment Details</h3>
                <p style='margin: 5px 0;'><strong>Doctor:</strong> Dr. {payment.DoctorName}</p>
                <p style='margin: 5px 0;'><strong>Date:</strong> {payment.AppointmentDate:dddd, MMMM dd, yyyy}</p>
                <p style='margin: 5px 0;'><strong>Time:</strong> {payment.AppointmentTime}</p>
                <p style='margin: 5px 0;'><strong>Hospital:</strong> {payment.HospitalName}</p>
            </div>
            
            <div class='total-row'>
                <span>Total Amount Paid</span>
                <span>{payment.Currency} {payment.Amount:N2}</span>
            </div>
            
            <div class='footer'>
                <p>Thank you for choosing our services!</p>
                <p>© 2024 Hospital Management System. All rights reserved.</p>
                <p style='font-size: 10px;'>This is a computer-generated receipt and does not require a signature.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
