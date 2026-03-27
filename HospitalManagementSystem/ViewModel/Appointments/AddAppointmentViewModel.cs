using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Web.ViewModel.Appointments
{
  

        public class PatientData
        {
            public int PatientId { get; set; }
            public string PatientName { get; set; }
        }
        public class AppointmentData
        {
            public int Id { get; set; }

            public int? DoctorId { get; set; }

            public int? PatientId { get; set; }

            public string? Problem { get; set; }

            public string? AppointmentDate { get; set; }

            public string? Status { get; set; }
        }
        public class AddAppointmentViewModel
        {
            public AppointmentData Appointment { get; set; }

            public List<PatientData>? PatientData { get; set; }

        }
    }

