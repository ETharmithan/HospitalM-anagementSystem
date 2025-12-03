namespace HospitalManagementSystem.Application.DTOs
{
    public class AdminOverviewDto
    {
        public int TotalUsers { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalDepartments { get; set; }
    }
}
