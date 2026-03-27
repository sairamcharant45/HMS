namespace HospitalManagementSystem.Web.ViewModel.Appointments
{
    public class RequestAppointmentViewModel
    {
        public AppointmentReq Appointment { get; set; }

        public List<DoctorData>? DoctorData { get; set; }


    }


    public class DoctorData
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
    }
    public class AppointmentReq
    {
        public int Id { get; set; }

        public int? DoctorId { get; set; }

        public int? PatientId { get; set; }

        public string? Problem { get; set; }

        public string? AppointmentDate { get; set; }

        public string? Status { get; set; }
    }

}
