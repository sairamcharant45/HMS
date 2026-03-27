using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Web.ViewModel.Doctor
{
    using System.ComponentModel.DataAnnotations;

    public class AddDoctorViewModel
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public string? UserIdType { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name should contain only alphabets and spaces")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Enter Valid Email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        [Display(Name = "Designation")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Designation should contain only alphabets and spaces")]
        public string? Designation { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]
        [Display(Name = "Mobile Number")]
        [MaxLength(14, ErrorMessage = "Invalid Mobile Number")]
        [RegularExpression(@"^\+?[0-9]{10,14}$", ErrorMessage = "Mobile number should be between 10 to 14 digits and may start with +")]
        public string? MobileNumber { get; set; }

        [Required(ErrorMessage = "Specialization is required")]
        [Display(Name = "Specialization")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Specialization should contain only alphabets and spaces")]
        public string? Specilization { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        [RegularExpression(@"^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Blood Group is required")]
        [Display(Name = "Blood Group")]
        [RegularExpression(@"^(A|B|AB|O)[+-]$", ErrorMessage = "Blood Group must be A+, A-, B+, B-, AB+, AB-, O+, or O-")]
        public string? BloodGroup { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date, ErrorMessage = "Invalid Date Format")]
        public string? DateOfBirth { get; set; }

        public string? Status { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Country should contain only alphabets and spaces")]
        public string? Country { get; set; }
    }

}
