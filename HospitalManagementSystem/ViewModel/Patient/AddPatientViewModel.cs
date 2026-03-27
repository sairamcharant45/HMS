using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Web.ViewModel.Patient
{
    public class AddPatientViewModel
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

        [Required]
        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Required]
        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date, ErrorMessage = "Invalid Date Format")]
        public string? DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Mobile Number")]
        [MaxLength(14, ErrorMessage = "Invalid Mobile Number")]
        [RegularExpression(@"^\+?[0-9]{10,14}$", ErrorMessage = "Invalid Mobile Number")]
        public string? MobileNumber { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Country should contain only alphabets and spaces")]
        public string? Country { get; set; }
    }
}
