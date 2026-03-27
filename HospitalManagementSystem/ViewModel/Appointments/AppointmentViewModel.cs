namespace HospitalManagementSystem.Web.ViewModel.Appointments
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? PatientId { get; set; }

        public string? Problem { get; set; }

        public string? AppointmentDate { get; set; }

        public string? Status { get; set; }

   
    }
}
