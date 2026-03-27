using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Web.ViewModel.Prescription
{
    public class PrescriptionViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Doctor Name")]
        public string DoctorName { get; set; }

        [Display(Name = "Doctor Specialization")]
        public string DoctorSpecialization { get; set; }

        [Display(Name = "Patient Name")]
        public string PatientName { get; set; }

        [Display(Name = "Patient Age")]
        public int? PatientAge { get; set; }  // Nullable int

        [Display(Name = "Patient Gender")]
        public string PatientGender { get; set; }

        [Display(Name = "Patient Email")]
        public string PatientEmailAddress { get; set; }

        [Display(Name = "Patient Phone")]
        public string PatientPhoneNo { get; set; }

        // Medical Tests
        [Display(Name = "Medical Test 1")]
        public string MedicalTest1 { get; set; }

        [Display(Name = "Medical Test 2")]
        public string MedicalTest2 { get; set; }

        // Medicines
        [Display(Name = "Medicine 1")]
        public string Medicine1 { get; set; }
        public bool Morning1 { get; set; }
        public bool Afternoon1 { get; set; }
        public bool Evening1 { get; set; }

        [Display(Name = "Medicine 2")]
        public string Medicine2 { get; set; }
        public bool Morning2 { get; set; }
        public bool Afternoon2 { get; set; }
        public bool Evening2 { get; set; }

        [Display(Name = "Medicine 3")]
        public string Medicine3 { get; set; }
        public bool Morning3 { get; set; }
        public bool Afternoon3 { get; set; }
        public bool Evening3 { get; set; }

        [Display(Name = "Medicine 4")]
        public string Medicine4 { get; set; }
        public bool Morning4 { get; set; }
        public bool Afternoon4 { get; set; }
        public bool Evening4 { get; set; }

        [Display(Name = "Check Up After (Days)")]
        public int CheckUpAfterDays { get; set; }

        [Display(Name = "Prescription Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PrescriptionAddDate { get; set; }

        //Patient Details
        //public Patient Patient { get; set; }
    }
}
