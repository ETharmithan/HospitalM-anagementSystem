using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto
{
    public class DoctorRequestDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = null!;


        [Required(ErrorMessage = "License number is required")]
        [MaxLength(50)]
        public string LicenseNumber { get; set; } = null!;

        [Required(ErrorMessage = "Qualification is required")]
        public string Qualification { get; set; } = null!;

        [Required(ErrorMessage = "Status is required")]
        [MaxLength(50)]
        public string Status { get; set; } = null!;

        [MaxLength(500)]
        public string? ProfileImage { get; set; }

        //ForeignKey
        [Required(ErrorMessage = "DepartmentId is required")]
        public Guid DepartmentId { get; set; }

    }
}
