using System.Security.Claims;
using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Web.Communication;
using HospitalManagementSystem.Web.Helper;
using HospitalManagementSystem.Web.Models;
using HospitalManagementSystem.Web.ViewModel;
using HospitalManagementSystem.Web.ViewModel.Doctor;
using HospitalManagementSystem.Web.ViewModel.Patient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace HospitalManagementSystem.Web.Controllers
{
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IdpClient idpClient;

        public DoctorController(IDoctorService doctorService, IConfiguration configuration, IUserService userService, IHttpClientFactory httpClientFactory)
        {
            _doctorService = doctorService;
            _configuration = configuration;
            _userService = userService;
            _httpClientFactory = httpClientFactory;
            idpClient = new IdpClient(configuration, _httpClientFactory);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddDoctor()
        {
            if (_configuration["Client"] == "DTStg") ViewBag.ClientName = "UgPass";
            else ViewBag.clientname = _configuration["Client"];
            return View(new AddDoctorViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> GetProfile([FromBody] DoctorSearchViewModel model)
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
        public async Task<IActionResult> AddDoctor(AddDoctorViewModel model)
        {
            AlertViewModel alert;
            if (!ModelState.IsValid)
            {
                if (_configuration["Client"] == "DTStg") ViewBag.ClientName = "UgPass";
                else ViewBag.clientname = _configuration["Client"];
                return View(model);
            }
            var userExists = await _userService.isUserExists(model.Email);
            if (userExists)
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var userDetails =_userService.GetUserByEmail(model.Email);
                var displayMessage = userDetails.Result.Roleid == 2 ? "Doctor" : "Patient";


                alert = new AlertViewModel { Message = "User already added as "+displayMessage };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving doctor: " + "User already exists" });
            }
            var userid = Guid.NewGuid().ToString();
            User newuser = new User()
            {
                Uuid = userid,
                Email = model.Email,
                Password = "Demo@123",
                Roleid = 2,
                Name = model.Name,
                Phonenumber = model.MobileNumber
            };

            var response = await _userService.CreateUser(newuser);
            if(response == false)
            {
                alert = new AlertViewModel { Message = "Failed to Add User in User DB" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving doctor: " + "Failed to add in User DB" });
            }

            var doctor = new Doctor()
            {
                Userid = userid,
                Name = model.Name,
                Email = model.Email,
                Designation = model.Designation,
                Mobilenumber = model.MobileNumber,
                Specilization = model.Specilization,
                Gender = model.Gender,
                Bloodgroup = model.BloodGroup,
                Dateofbirth = model.DateOfBirth,
                Country = model.Country,
                Status = "ACTIVE"
            };

            response = await _doctorService.AddDoctor(doctor);
            if (response == false)
            {
                alert = new AlertViewModel { Message = "Failed to Add Doctor in Doctor DB" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving doctor: " + "Failed to add Doctor DB" });
            }

            alert = new AlertViewModel { IsSuccess = true, Message = "Doctor Created Successfully" };
            TempData["Alert"] = JsonConvert.SerializeObject(alert);
            return RedirectToAction("ListOfDoctors");
            //return Json(new { success = true, message = "Doctor saved successfully." });
        }

        public async Task<IActionResult> ListOfDoctors()
        {
            List<DoctorListViewModel> list = new List<DoctorListViewModel>();
            var doctorsData = await _doctorService.ListAllDoctorsAsync();
            foreach (var doctor in doctorsData)
            {
                var doctordetails = new DoctorListViewModel()
                {
                    Id = doctor.Id,
                    Name = doctor.Name,
                    Designation = doctor.Designation,
                    Specialization = doctor.Specilization,
                    Gender = doctor.Gender,
                    Status = doctor.Status,
                };
                list.Add(doctordetails);
            }
            return View(list);
        }

        public async Task<IActionResult> EditDoctorDetails(int id)
        {
            var doctorDetails = await _doctorService.GetDoctorById(id);
            var model = new EditDoctorViewModel()
            {
                Id = doctorDetails.Id,
                UserId = doctorDetails.Userid,
                Name = doctorDetails.Name,
                Designation = doctorDetails.Designation,
                Email = doctorDetails.Email,
                Specilization = doctorDetails.Specilization,
                Gender = doctorDetails.Gender,
                Status = doctorDetails.Status,
                BloodGroup = doctorDetails.Bloodgroup,
                MobileNumber = doctorDetails.Mobilenumber,
                DateOfBirth = doctorDetails.Dateofbirth,
                Country = doctorDetails.Country
            };
            return View(model);
        }

       
        [HttpPost]
        public async Task<IActionResult> EditDoctorDetails(EditDoctorViewModel model)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
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
                //return Json(new { success = false, message = "Error saving doctor: " + "Failed to fetch user details" });
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
                //return Json(new { success = false, message = "Error saving doctor: " + "Failed to update data in User DB" });
            }
            
            var doctor = await _doctorService.GetDoctorById(model.Id);
            doctor.Name = model.Name;
            doctor.Email = model.Email;
            doctor.Designation = model.Designation;
            doctor.Specilization = model.Specilization;
            doctor.Gender = model.Gender;
            doctor.Bloodgroup = model.BloodGroup;
            doctor.Dateofbirth = model.DateOfBirth;
            doctor.Country = model.Country;
            doctor.Mobilenumber = model.MobileNumber;
            doctor.Status = model.Status;

            response = await _doctorService.UpdateDoctor(doctor);
            if (response == false)
            {
                alert = new AlertViewModel { Message = "Failed to Update details in Doctor DB" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
                //return Json(new { success = false, message = "Error saving doctor: " + "Failed to Update details in Doctor DB" });
            }

            alert = new AlertViewModel { IsSuccess = true, Message = "Doctor Details Updated Successfully" };
            TempData["Alert"] = JsonConvert.SerializeObject(alert);
   
            return RedirectToAction("ListOfDoctors");
            //return Json(new { success = true, message = "Doctor Details Updated Successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var response = await _doctorService.DeleteDoctor(id);
            if (response == true)
            {
                //Alert alert = new Alert { IsSuccess = true, Message = "Successfully Deleted Doctor" };
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


        public async Task<IActionResult> ListOfDoctorsForPatients()
        {
            List<DoctorListViewModel> list = new List<DoctorListViewModel>();
            var doctorsData = await _doctorService.ListAllDoctorsAsync();
            foreach (var doctor in doctorsData)
            {
                var doctordetails = new DoctorListViewModel()
                {
                    Id = doctor.Id,
                    Name = doctor.Name,
                    Email = doctor.Email,
                    MobileNumber = doctor.Mobilenumber
                };
                list.Add(doctordetails);
            }
            return View(list);
        }
    }
}
