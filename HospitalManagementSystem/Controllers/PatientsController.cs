using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Core.DTOs;
using HospitalManagementSystem.Web.Communication;
using HospitalManagementSystem.Web.Helper;
using HospitalManagementSystem.Web.ViewModel.Patient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private IConfiguration _configuration { get; set; }
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;
        private readonly IdpClient idpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        public PatientsController(IConfiguration configuration, IPatientService patientService, IUserService userService, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _patientService = patientService;
            _userService = userService;
            _httpClientFactory = httpClientFactory;
            idpClient = new IdpClient(configuration, _httpClientFactory);
        }

        [HttpPost]
        [Route("AddPatient")]
        public async Task<IActionResult> AddPatient(AddPatientDTO model)
        {
            var userExists = await _userService.isUserExists(model.EmailAddress);
            if (userExists)
            {
                //alert = new AlertViewModel { Message = "User Already Exists" };
                var userDetails = _userService.GetUserByEmail(model.EmailAddress);
                var displayMessage = userDetails.Result.Roleid == 2 ? "Doctor" : "Patient";
                //TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Ok(new APIResponse(false, "User Already Exists"));
                //return Json(new { success = false, message = "Error saving patient: " + "User already exists" });
            }
            var userid = Guid.NewGuid().ToString();
            User newuser = new User()
            {
                Uuid = userid,
                Email = model.EmailAddress,
                Password = "Demo@123",
                Roleid = 3,
                Name = model.FullName,
                Phonenumber = model.PhoneNo
            };

            var response = await _userService.CreateUser(newuser);
            if (response == false)
            {
                return Ok(new APIResponse(false, "Failed to add user in DB"));
                //return Json(new { success = false, message = "Error saving patient: " + "Failed to add in User DB" });
            }

            var patient = new Patient()
            {
                Userid = userid,
                Name = model.FullName,
                Email = model.EmailAddress,
                Gender = model.Gender,
                Bloodgroup = model.BloodGroup == null ? "" : model.BloodGroup,
                Dateofbirth = model.DateOfBirth.ToString(),
                Mobilenumber = model.PhoneNo,
                Country = ""
            };

            response = await _patientService.AddPatient(patient);
            if (response == false)
            {
                return Ok(new APIResponse(false, "Failed to add data in DB")); 
                //return View(model);
                //return Json(new { success = false, message = "Error saving patient: " + "Failed to add Doctor DB" });
            }
            return Ok(new APIResponse(true, "Patient Saved Successfully"));
            //return RedirectToAction("ListOfPatients");
            //return Json(new { success = true, message = "Patient saved successfully." });
        }

    }
}
