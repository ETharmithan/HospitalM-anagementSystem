using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto
{
    public class DoctorAppointmentRequestDto
    {
        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Appointment time is required")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Invalid time format (HH:mm)")]
        public string AppointmentTime { get; set; } = null!;

        [Required(ErrorMessage = "Appointment status is required")]
        [MaxLength(50)]
        public string AppointmentStatus { get; set; } = null!;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }



        //ForeignKey
        [Required(ErrorMessage = "PatientId is required")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "DoctorId is required")]
        public Guid DoctorId { get; set; }

    }
}
