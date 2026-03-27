using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Web.Helper;
using HospitalManagementSystem.Web.Models;
using HospitalManagementSystem.Web.ViewModel;
using HospitalManagementSystem.Web.ViewModel.Doctor;
using HospitalManagementSystem.Web.ViewModel.Patient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HospitalManagementSystem.Web.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;
        private readonly IdpClient idpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        public PatientController(IConfiguration configuration,
            IPatientService patientService, 
            IUserService userService,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _patientService = patientService;
            _userService = userService;
            _httpClientFactory = httpClientFactory;
            idpClient = new IdpClient(configuration, _httpClientFactory);
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddPatient()
        {
            if (_configuration["Client"] == "DTStg") ViewBag.ClientName = "UgPass";
            else ViewBag.clientname = _configuration["Client"];
            return View(new AddPatientViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> GetProfile([FromBody] PatientSearchViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId))
            {
                return BadRequest("User ID is required.");
            }

            try
            {
                var tokenResponse = await idpClient.GetToken();
                if (tokenResponse != null && tokenResponse["access_token"] != null)
                {
                    string accessToken = tokenResponse["access_token"].ToString();
                    var userIdType = model.UserIdType;
                    if (model.UserIdType == "DOCTYPE") model.UserIdType = "1";
                    if (model.UserIdType == "SUID") model.UserIdType = "5";
                    if (model.UserIdType == "EMAIL") model.UserIdType = "3";
                    if (model.UserIdType == "PHONE_NUMBER") model.UserIdType = "4";

                    var getUserProfileRequest = new GetUserProfileRequest
                    {
                        ProfileType = "HMSProfile",
                        UserId = model.UserId,
                        UserIdType = model.UserIdType
                    };

                    string clientName = _configuration["Client"];
                    string ConsentClientId = _configuration["dtidp:ClientId"];

                    if (string.Equals(clientName, "DTStg", StringComparison.OrdinalIgnoreCase))
                    {
                        getUserProfileRequest.ClientId = ConsentClientId;
                        getUserProfileRequest.ProfileType = _configuration["DTStgProfile"];
                        getUserProfileRequest.UserId = model.UserId;
                        getUserProfileRequest.UserIdType = model.UserIdType;
                        getUserProfileRequest.Purpose = _configuration["DTStgPurpose"];
                        getUserProfileRequest.Scopes = _configuration["DTStgScopes"];
                    }
                    else
                    {
                        getUserProfileRequest.ClientId = ConsentClientId;
                        getUserProfileRequest.ProfileType = _configuration["GeneralProfile"];
                        getUserProfileRequest.UserId = model.UserId;
                        getUserProfileRequest.UserIdType = model.UserIdType;
                        getUserProfileRequest.Purpose = _configuration["GeneralPurpose"];
                        getUserProfileRequest.Scopes = _configuration["GeneralScopes"];
                    }

                    var apiResponse = await idpClient.GetUserProfile(accessToken, getUserProfileRequest);
                    if (apiResponse.Success)
                    {
                        return Json(new { Status = "Success", Message = apiResponse.Message, Result = apiResponse.Result });
                    }
                    else
                    {
                        return Json(new { Status = "Failed", Message = apiResponse.Message, Result = apiResponse.Result });
                    }
                }
                else
                {
                    return Json(new { Status = "Failed", Message = "Failed to retrieve access token." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Failed", Message = ex.Message.ToString() });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPatient(AddPatientViewModel model)
        {
            AlertViewModel alert;
            if (!ModelState.IsValid)
            {
                if (_configuration["Client"] == "DTStg") ViewBag.ClientName = "UgPass";
                else ViewBag.clientname = _configuration["Client"];
                return View(model);
                //return Json(new { success = false, message = "Please correct the errors in the form." });
            }
            var userExists = await _userService.isUserExists(model.Email);
            if (userExists)
            {
                //alert = new AlertViewModel { Message = "User Already Exists" };
                var userDetails = _userService.GetUserByEmail(model.Email);
                var displayMessage = userDetails.Result.Roleid == 2 ? "Doctor" : "Patient";
                //TempData["Alert"] = JsonConvert.SerializeObject(alert);
                alert = new AlertViewModel { Message = "User already added as " + displayMessage };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving patient: " + "User already exists" });
            }
            var userid = Guid.NewGuid().ToString();
            User newuser = new User()
            {
                Uuid = userid,
                Email = model.Email,
                Password = "Demo@123",
                Roleid = 3,
                Name = model.Name,
                Phonenumber = model.MobileNumber
            };

            var response = await _userService.CreateUser(newuser);
            if (response == false)
            {
                alert = new AlertViewModel { Message = "Failed to Add User in User DB" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving patient: " + "Failed to add in User DB" });
            }

            var patient = new Patient()
            {
                Userid = userid,
                Name = model.Name,
                Email = model.Email,
                Gender = model.Gender,
                Bloodgroup = model.BloodGroup,
                Dateofbirth = model.DateOfBirth,
                Mobilenumber = model.MobileNumber,
                Country = model.Country
            };

            response = await _patientService.AddPatient(patient);
            if (response == false)
            {
                alert = new AlertViewModel { Message = "Failed to Add Patient in Patient DB" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving patient: " + "Failed to add Doctor DB" });
            }

            alert = new AlertViewModel { IsSuccess = true, Message = "Patient Created Successfully" };
            TempData["Alert"] = JsonConvert.SerializeObject(alert);
            return RedirectToAction("ListOfPatients");
            //return Json(new { success = true, message = "Patient saved successfully." });
        }

        public async Task<IActionResult> ListOfPatients()
        {
            List<PatientListViewModel> list = new List<PatientListViewModel>();
            var doctorsData = await _patientService.ListAllPatientsAsync();
            foreach (var doctor in doctorsData)
            {
                var doctordetails = new PatientListViewModel()
                {
                    Id = doctor.Id,
                    Name = doctor.Name,
                    Gender = doctor.Gender,
                    Email = doctor.Email,
                    BloodGroup= doctor.Bloodgroup,
                };
                list.Add(doctordetails);
            }
            return View(list);
        }

        public async Task<IActionResult> EditPatientDetails(int id)
        {
            var patientDetails = await _patientService.GetPatientById(id);
            var model = new EditPatientViewModel()
            {
                Id = patientDetails.Id,
                UserId = patientDetails.Userid,
                Name = patientDetails.Name,
                Email = patientDetails.Email,
                Gender = patientDetails.Gender,
                BloodGroup = patientDetails.Bloodgroup,
                DateOfBirth = patientDetails.Dateofbirth,
                MobileNumber = patientDetails.Mobilenumber,
                Country = patientDetails.Country
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditPatientDetails(EditPatientViewModel model)
        {
            AlertViewModel alert;
            if (!ModelState.IsValid)
            {
                if (_configuration["Client"] == "DTStg") ViewBag.ClientName = "UgPass";
                else ViewBag.clientname = _configuration["Client"];
                return View(model);
                //return Json(new { success = false, message = "Please correct the errors in the form." });
            }
            var user = await _userService.GetUserDetailsByUUID(model.UserId);
            if (user == null)
            {
                alert = new AlertViewModel { Message = "Failed to fetch user details" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving patient: " + "Failed to fetch user details" });
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.Phonenumber = model.MobileNumber;

            var response = await _userService.UpdateUser(user);
            if (response == false)
            {
                alert = new AlertViewModel { Message = "Failed to update data in User DB" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving patient: " + "Failed to update data in User DB" });
            }

            var patient = await _patientService.GetPatientById(model.Id);
            patient.Name = model.Name;
            patient.Email = model.Email;
            patient.Gender = model.Gender;
            patient.Bloodgroup = model.BloodGroup;
            patient.Dateofbirth = model.DateOfBirth;
            patient.Country = model.Country;
            patient.Mobilenumber = model.MobileNumber;

            response = await _patientService.UpdatePatient(patient);
            if (response == false)
            {
                alert = new AlertViewModel { Message = "Failed to Update details in Patient DB" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving patient: " + "Failed to Update details in patient DB" });
            }

            alert = new AlertViewModel { IsSuccess = true, Message = "Patient Details Updated Successfully" };
            TempData["Alert"] = JsonConvert.SerializeObject(alert);

            return RedirectToAction("ListOfPatients");
            //return Json(new { success = true, message = "Patient Details Updated Successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var response = await _patientService.DeletePatient(id);
            if (response == true)
            {
                //Alert alert = new Alert { IsSuccess = true, Message = "Successfully Deleted Patient" };
                //TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return new JsonResult(true);
            }
            else
            {
                //Alert alert = new Alert { Message = "Internal error please contact to admin" };
                //TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return new JsonResult(false);
            }
        }       
    }
}