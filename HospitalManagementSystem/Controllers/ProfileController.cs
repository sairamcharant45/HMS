using HospitalManagementSystem.Web.ViewModel.Doctor;
using HospitalManagementSystem.Web.ViewModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Web.Helper;
using System.Net.Http;
using HospitalManagementSystem.Web.ViewModel.Profile;
using Microsoft.AspNetCore.Authorization;

namespace HospitalManagementSystem.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IPatientService _patientService;

        public ProfileController(IDoctorService doctorService, IConfiguration configuration, IUserService userService, IPatientService patientService)
        {
            _doctorService = doctorService;
            _configuration = configuration;
            _userService = userService;
            _patientService = patientService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UpdateProfileDetails()
        {

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            UpdateProfileViewModel model = null;

            // Check UserRole 
            if (userRole == "Doctor")
            {
                var doctorUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
                var doctorDetails = await _doctorService.GetDoctorDetailsByUUID(doctorUUID);
                //int doctorId = doctorDetails.Id;
                if (doctorDetails != null)
                {
                    model = new UpdateProfileViewModel()
                    {
                        Id = doctorDetails.Id,
                        UserId = doctorDetails.Userid,
                        Name = doctorDetails.Name,
                        Designation = doctorDetails.Designation,
                        Email = doctorDetails.Email,
                        Specilization = doctorDetails.Specilization,
                        Gender = doctorDetails.Gender,
                       
                        BloodGroup = doctorDetails.Bloodgroup,
                        MobileNumber = doctorDetails.Mobilenumber,
                        DateOfBirth = doctorDetails.Dateofbirth,
                        Country = doctorDetails.Country
                    };
                }
            }
            else if (userRole == "Patient")
            {
                var patientUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
                var patientDetails = await _patientService.GetPatientByUUID(patientUUID);
                //int patientId = patientDetails.Id;
                if (patientDetails != null)
                {
                    model = new UpdateProfileViewModel()
                    {
                        Id = patientDetails.Id,
                        UserId = patientDetails.Userid,
                        Name = patientDetails.Name,
                        Email = patientDetails.Email,
                        Gender = patientDetails.Gender,
                        BloodGroup = patientDetails.Bloodgroup,
                        MobileNumber = patientDetails.Mobilenumber,
                        DateOfBirth = patientDetails.Dateofbirth,
                        Country = patientDetails.Country
                    };
                }
            }

            if (model == null) // Handle the case where neither role matched or data retrieval failed
            {
                return RedirectToAction("Error"); // Redirect to an error page or show a message
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfileDetails(UpdateProfileViewModel model)
        {
            AlertViewModel alert;
            if (!ModelState.IsValid)
            {
                if (_configuration["Client"] == "DTStg") ViewBag.ClientName = "UgPass";
                else ViewBag.clientname = _configuration["Client"];
                return View(model);
            }
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;     

            var user = await _userService.GetUserDetailsByUUID(model.UserId);
            if (user == null)
            {
                alert = new AlertViewModel { Message = "Failed to fetch user details" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
            }

            // Common user updates
            user.Name = model.Name;
            user.Email = model.Email;
            user.Phonenumber = model.MobileNumber;

            var response = await _userService.UpdateUser(user);
            if (!response)
            {
                alert = new AlertViewModel { Message = "Failed to update data in User DB" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
            }

            if (userRole == "Doctor")
            {
                var doctor = await _doctorService.GetDoctorById(model.Id);
                if (doctor == null)
                {
                    alert = new AlertViewModel { Message = "Doctor record not found" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View(model);
                }

                doctor.Name = model.Name;
                doctor.Email = model.Email;
                doctor.Designation = model.Designation;
                doctor.Specilization = model.Specilization;
                doctor.Gender = model.Gender;
                doctor.Bloodgroup = model.BloodGroup;
                doctor.Dateofbirth = model.DateOfBirth;
                doctor.Country = model.Country;
                doctor.Mobilenumber = model.MobileNumber;
                

                response = await _doctorService.UpdateDoctor(doctor);
                if (!response)
                {
                    alert = new AlertViewModel { Message = "Failed to update details in DB" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View(model);
                }

                alert = new AlertViewModel { IsSuccess = true, Message = "Details Updated Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("UpdateProfileDetails");
            }
            else if (userRole == "Patient")
            {
                var patient = await _patientService.GetPatientById(model.Id);
                if (patient == null)
                {
                    alert = new AlertViewModel { Message = "Patient record not found" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View(model);
                }

                patient.Name = model.Name;
                patient.Email = model.Email;
                patient.Gender = model.Gender;
                patient.Bloodgroup = model.BloodGroup;
                patient.Dateofbirth = model.DateOfBirth;
                patient.Country = model.Country;
                patient.Mobilenumber = model.MobileNumber;

                response = await _patientService.UpdatePatient(patient);
                if (!response)
                {
                    alert = new AlertViewModel { Message = "Failed to update details in  DB" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View(model);
                }

                alert = new AlertViewModel { IsSuccess = true, Message = "Details Updated Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("UpdateProfileDetails");
            }

            alert = new AlertViewModel { Message = "Invalid user role" };
            TempData["Alert"] = JsonConvert.SerializeObject(alert);
            return RedirectToAction("ErrorPage");
        }


    }
}
