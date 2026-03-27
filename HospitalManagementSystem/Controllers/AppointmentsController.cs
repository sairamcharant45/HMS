using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Web.ViewModel;
using HospitalManagementSystem.Web.ViewModel.Appointments;
using HospitalManagementSystem.Web.ViewModel.Doctor;
using HospitalManagementSystem.Web.ViewModel.Prescription;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace HospitalManagementSystem.Web.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;
        public AppointmentsController(IDoctorService doctorService, IAppointmentService appointmentService, IPatientService patientService)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
            _patientService = patientService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddAppointment()
        {
            var patients = await _patientService.ListAllPatientsAsync();
            var viewModel = new AddAppointmentViewModel
            {
                PatientData = patients.Select(p => new PatientData { PatientId = p.Id, PatientName = p.Name }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAppointment(AddAppointmentViewModel model)
        {
            AlertViewModel alert;
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}"); // Log to console (for debugging)
                }

                return View(model); // Return to the form with validation errors
            }

            try
            {
                var doctorUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
                var doctor = await _doctorService.GetDoctorDetailsByUUID(doctorUUID);
                int doctorId = doctor.Id;

                var appointment = new Appointment
                {
                    Patientid = model.Appointment.PatientId,
                    Doctorid = doctorId,
                    Problem = model.Appointment.Problem,
                    Appointmentdate = model.Appointment.AppointmentDate,
                    Status = "APPROVED"
                };

                var res = await _appointmentService.CreateAppointment(appointment);

                if (res)
                {
                    alert = new AlertViewModel { IsSuccess = true, Message = "Appointment Added Successfully" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return RedirectToAction("ListOfAppointments", "Appointments");
                }
                else
                {
                    alert = new AlertViewModel { Message = "Failed to Add Appointment" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> RequestAppointment()
        {
            var doctors = await _doctorService.ListAllDoctorsAsync();
            var viewModel = new RequestAppointmentViewModel
            {
                DoctorData = doctors.Select(p => new DoctorData { DoctorId = p.Id, DoctorName = p.Name }).ToList()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> EditAppointmentDetails(int id)
        {
            var appointment = await _appointmentService.GetAppointmentById(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var doctorDetails = await _doctorService.GetDoctorById((int)appointment.Doctorid);
            var patientDetails = await _patientService.GetPatientById((int)appointment.Patientid);

            var model = new AppointmentViewModel()
            {
                Id = appointment.Id,
                Name = doctorDetails.Name,
                Problem = appointment.Problem,
                AppointmentDate = appointment.Appointmentdate,
                Status = appointment.Status
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditAppointmentDetails(AppointmentViewModel model)
        {
            AlertViewModel alert;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var appointment = await _appointmentService.GetAppointmentById(model.Id);
                if (appointment == null)
                {
                    return NotFound();
                }

                // Update values from form inputs
                appointment.Problem = model.Problem;
                appointment.Appointmentdate = model.AppointmentDate;

                await _appointmentService.UpdateAppointment(appointment);

                alert = new AlertViewModel { IsSuccess = true, Message = "Appointment Details Edited Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("ViewAppointmentsByPatient");
            }
            catch (Exception ex)
            {
                alert = new AlertViewModel { Message = "Failed to Edit Appointment Details" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestAppointment(AddAppointmentViewModel model)
        {
            AlertViewModel alert;
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}"); // Log to console (for debugging)
                }

                return View(model); // Return to the form with validation errors
            }

            try
            {
                var patientUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
                var patient = await _patientService.GetPatientByUUID(patientUUID);
                int patientId = patient.Id;

                var appointment = new Appointment
                {
                    Patientid = patientId,
                    Doctorid = model.Appointment.DoctorId,
                    Problem = model.Appointment.Problem,
                    Appointmentdate = model.Appointment.AppointmentDate,
                    Status = "PENDING"
                };

                var res = await _appointmentService.CreateAppointment(appointment);

                if (res)
                {
                    alert = new AlertViewModel { IsSuccess = true, Message = "Appointment Requested Successfully" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return RedirectToAction("ViewAppointmentsByPatient", "Appointments");
                }
                else
                {
                    alert = new AlertViewModel { Message = "Failed To Request Appointment" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                alert = new AlertViewModel { Message = "Failed to Request Appointment" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
            }
        }


        public async Task<IActionResult> ListOfAppointments()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Check UserRole 
            if (userRole == "Doctor")
            {
                var doctorUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
                var doctor = await _doctorService.GetDoctorDetailsByUUID(doctorUUID);
                int doctorId = doctor.Id;


                // Fetch all appointments for the doctor
                var appointmentDetails = (await _appointmentService.GetAppointmentsByDoctorId(doctorId)).ToList();
                var doctorName = doctor.Name;

                // Create a list to store appointment view models
                var appointmentList = new List<AppointmentListViewModel>();

                for (int i = 0; i < appointmentDetails.Count; i++)
                {
                    var patientId = appointmentDetails[i].Patientid ?? 0;
                    var patient = await _patientService.GetPatientById(patientId);
                    var patientName = patient?.Name ?? "Unknown";
                    var doctordetails = new AppointmentListViewModel()
                    {
                        Id = appointmentDetails[i].Id,
                        Name = patientName,  // Assign patient name instead of ID
                        Problem = appointmentDetails[i].Problem,
                        AppointmentDate = appointmentDetails[i].Appointmentdate,
                        Status = appointmentDetails[i].Status,
                    };

                    appointmentList.Add(doctordetails);
                }

                return View(appointmentList); // Pass the list to the view
            }
            return Unauthorized();
        }


        public async Task<IActionResult> ViewAppointmentsByPatient()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Check UserRole 
            if (userRole == "Patient")
            {
                var patientUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
                var patient = await _patientService.GetPatientByUUID(patientUUID);
                int patientId = patient.Id;


                // Fetch all appointments for the doctor
                var appointmentDetails = (await _appointmentService.GetAppointmentsByPatientId(patientId)).ToList();
                var patientName = patient.Name;

                // Create a list to store appointment view models
                var appointmentList = new List<AppointmentListViewModel>();

                for (int i = 0; i < appointmentDetails.Count; i++)
                {
                    var doctorId = appointmentDetails[i].Doctorid ?? 0;
                    var doctor = await _doctorService.GetDoctorById(doctorId);
                    var doctorName = doctor?.Name ?? "Unknown";
                    var doctordetails = new AppointmentListViewModel()
                    {
                        Id = appointmentDetails[i].Id,
                        Name = doctorName,  // Assign patient name instead of ID
                        Problem = appointmentDetails[i].Problem,
                        AppointmentDate = appointmentDetails[i].Appointmentdate,
                        Status = appointmentDetails[i].Status,
                    };

                    appointmentList.Add(doctordetails);
                }

                return View(appointmentList); // Pass the list to the view
            }

            return Unauthorized();

        }


        public async Task<IActionResult> ViewAppointmentDetails(int id)
        {
            var appointment = await _appointmentService.GetAppointmentById(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var doctorDetails = await _doctorService.GetDoctorById((int)appointment.Doctorid);
            var patientDetails = await _patientService.GetPatientById((int)appointment.Patientid);

            var model = new AppointmentViewModel()
            {
                Id = appointment.Id,
                Name = patientDetails.Name,
                Problem = appointment.Problem,
                AppointmentDate = appointment.Appointmentdate,
                Status = appointment.Status
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
        {
            var appointment = await _appointmentService.GetAppointmentById(id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            appointment.Status = status;
            await _appointmentService.UpdateAppointment(appointment);

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var response = await _appointmentService.DeleteAppointment(id);
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
