using HospitalManagementSystem.Core.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Web.ViewModel.Prescription
{
    public class PatientList
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
    }
    public class PrescriptionData
        {
            public int Id { get; set; }
            public int DoctorId { get; set; }

            [Display(Name = "Patient Name")]
            public int PatientId { get; set; }

            [Display(Name = "Medical Tests")]
            public string? MedicalTest1 { get; set; }
            public string? MedicalTest2 { get; set; }

            public string? Medicine1 { get; set; }
            public bool? Morning1 { get; set; }
            public bool? Afternoon1 { get; set; }
            public bool? Evening1 { get; set; }

            public string? Medicine2 { get; set; }
            public bool? Morning2 { get; set; }
            public bool? Afternoon2 { get; set; }
            public bool? Evening2 { get; set; }

            public string? Medicine3 { get; set; }
            public bool? Morning3 { get; set; }
            public bool? Afternoon3 { get; set; }
            public bool? Evening3 { get; set; }

            public string? Medicine4 { get; set; }
            public bool? Morning4 { get; set; }
            public bool? Afternoon4 { get; set; }
            public bool? Evening4 { get; set; }


            [Display(Name = "Checkup After Days")]
            public int? CheckUpAfterDays { get; set; }

            public string? DoctorTiming { get; set; }
        }
    public class AddPrescriptionViewModel
    {
        public PrescriptionData Prescription { get; set; }

        public List<PatientList>? PatientList { get; set; }

    }
}
