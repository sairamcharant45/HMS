using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Web.ViewModel.Login
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "UserName")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        //[Display(Name = "Remember me?")]
        //public bool RememberMe { get; set; }
    }
}
