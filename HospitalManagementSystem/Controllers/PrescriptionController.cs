using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Core.DTOs;
using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Web.Communication;
using HospitalManagementSystem.Web.Utilities;
using HospitalManagementSystem.Web.ViewModel;
using HospitalManagementSystem.Web.ViewModel.Doctor;
using HospitalManagementSystem.Web.ViewModel.Prescription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Web.Controllers
{

    public partial class SigningEnvelope
    {


        public string? Status { get; set; }

        public string? DocumentBase64 { get; set; }

        public string? DocumentName { get; set; }


        public string? EnvelopeId { get; set; }
    }
    [Authorize]
    public class PrescriptionController : Controller
    {
        private readonly IUserService _userService;
        private readonly DataExportService _dataExportService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        private readonly IConfiguration _configuration;
        private readonly IRazorRendererHelper _razorRendererHelper;

        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<PrescriptionController> _logger;

        public PrescriptionController(IUserService userService, IPrescriptionService prescriptionService,
            IPatientService patientService, IDoctorService doctorService, IConfiguration configuration, DataExportService dataExportService,
            IRazorViewEngine razorViewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider, ILogger<PrescriptionController> logger, IRazorRendererHelper razorRendererHelper)
        {
            _userService = userService;
            _prescriptionService = prescriptionService;
            _patientService = patientService;
            _doctorService = doctorService;
            _configuration = configuration;
            _dataExportService = dataExportService;
            _razorViewEngine = razorViewEngine;
            _razorRendererHelper = razorRendererHelper;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddPrescription()
        {
            var patients = await _patientService.ListAllPatientsAsync();
            var viewModel = new AddPrescriptionViewModel
            {
                PatientList = patients.Select(p => new PatientList { PatientId = p.Id, PatientName = p.Name }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPrescription(AddPrescriptionViewModel model)
        {
            AlertViewModel alert;
            if (!ModelState.IsValid)
            {
                var patients = await _patientService.ListAllPatientsAsync();
                var viewModel = new AddPrescriptionViewModel
                {
                    PatientList = patients.Select(p => new PatientList { PatientId = p.Id, PatientName = p.Name }).ToList()
                };
                return View(viewModel);
            }

            try
            {
                var doctorUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
                var doctor = await _doctorService.GetDoctorDetailsByUUID(doctorUUID);
                int doctorId = doctor.Id;
                var prescription = new Prescription
                {
                    Patientid = model.Prescription.PatientId,
                    Doctorid = doctorId,
                    Medicaltest1 = model.Prescription.MedicalTest1,
                    Medicaltest2 = model.Prescription.MedicalTest2,

                    Medicine1 = model.Prescription.Medicine1,
                    Morning1 = ConvertBoolToSbyte(model.Prescription.Morning1),
                    Afternoon1 = ConvertBoolToSbyte(model.Prescription.Afternoon1),
                    Evening1 = ConvertBoolToSbyte(model.Prescription.Evening1),

                    Medicine2 = model.Prescription.Medicine2,
                    Morning2 = ConvertBoolToSbyte(model.Prescription.Morning2),
                    Afternoon2 = ConvertBoolToSbyte(model.Prescription.Afternoon2),
                    Evening2 = ConvertBoolToSbyte(model.Prescription.Evening2),

                    Medicine3 = model.Prescription.Medicine3,
                    Morning3 = ConvertBoolToSbyte(model.Prescription.Morning3),
                    Afternoon3 = ConvertBoolToSbyte(model.Prescription.Afternoon3),
                    Evening3 = ConvertBoolToSbyte(model.Prescription.Evening3),

                    Medicine4 = model.Prescription.Medicine4,
                    Morning4 = ConvertBoolToSbyte(model.Prescription.Morning4),
                    Afternoon4 = ConvertBoolToSbyte(model.Prescription.Afternoon4),
                    Evening4 = ConvertBoolToSbyte(model.Prescription.Evening4),

                    Checkupafterdays = model.Prescription.CheckUpAfterDays==null ? 0 : model.Prescription.CheckUpAfterDays,
                    Createddate = DateTime.Now
                };

                var res = await _prescriptionService.CreatePrescription(prescription);
                if(res == false)
                {
                    alert = new AlertViewModel { Message = "Failed to Add Prescription" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View(model);
                }
                else
                {
                    alert = new AlertViewModel { IsSuccess = true, Message = "Prescription Created Successfully" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return RedirectToAction("ListOfPrescriptions");
                }
            }
            catch (Exception ex)
            {
                alert = new AlertViewModel { Message = "Failed to Add Prescription" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
            }
        }

        private static sbyte ConvertBoolToSbyte(bool? value)
        {
            return value.HasValue && value.Value ? (sbyte)1 : (sbyte)0;
        }

        public async Task<IActionResult> ListOfPrescriptions()
        {
            List<PrescriptionListViewModel> list = new List<PrescriptionListViewModel>();
            var doctorUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
            var doctor = await _doctorService.GetDoctorDetailsByUUID(doctorUUID);
            int doctorId = doctor.Id;

            var prescriptionsData = await _prescriptionService.ListAllPrescriptionsOfDoctorAsync(doctorId);

            var patientDict = new Dictionary<string, string>();
            var patients = await _patientService.ListAllPatientsAsync();
            foreach( var patient in patients)
            {
                patientDict[patient.Id.ToString()] = patient.Name;
            }

            foreach (var prescription in prescriptionsData)
            {
                DateTime? nullableDateTime = prescription.Createddate;
                DateTime? dateOnly = nullableDateTime?.Date;

                var prescriptionDetails = new PrescriptionListViewModel()
                {
                    Id = prescription.Id,
                    Name = patientDict[prescription.Patientid.ToString()],
                    CreatedDate = dateOnly?.ToString("dd-MM-yyyy")
                };
                list.Add(prescriptionDetails);
            }
            return View(list);
        }

        public async Task<IActionResult> ListOfPrescriptionsOfPatients()
        {
            List<PrescriptionListViewModel> list = new List<PrescriptionListViewModel>();
            var patientUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
            var patientDetails = await _patientService.GetPatientByUUID(patientUUID);
            int patientId = patientDetails.Id;

            var prescriptionsData = await _prescriptionService.ListAllPrescriptionsOfPatientAsync(patientId);

            var doctorDict = new Dictionary<string, string>();
            var doctors = await _doctorService.ListAllDoctorsAsync();
            foreach (var doctor in doctors)
            {
                doctorDict[doctor.Id.ToString()] = doctor.Name;
            }

            foreach (var prescription in prescriptionsData)
            {
                DateTime? nullableDateTime = prescription.Createddate;
                DateTime? dateOnly = nullableDateTime?.Date;

                var prescriptionDetails = new PrescriptionListViewModel()
                {
                    Id = prescription.Id,
                    Name = doctorDict[prescription.Doctorid.ToString()],
                    CreatedDate = dateOnly?.ToString("dd-MM-yyyy")
                };
                list.Add(prescriptionDetails);
            }
            return View(list);
        }

        public async Task<IActionResult> EditPrescription(int id)
        {
            var prescription = await _prescriptionService.GetPrescriptionById(id);
            var model = new EditPrescriptionViewModel()
            {
                Id = prescription.Id,
                MedicalTest1 = prescription.Medicaltest1,
                MedicalTest2 = prescription.Medicaltest2,
                Medicine1 = prescription.Medicine1,
                Morning1 = ConvertSbyteToBool(prescription.Morning1),
                Afternoon1 = ConvertSbyteToBool(prescription.Afternoon1),
                Evening1 = ConvertSbyteToBool(prescription.Evening1),
                Medicine2 = prescription.Medicine2,
                Morning2 = ConvertSbyteToBool(prescription.Morning2),
                Afternoon2 = ConvertSbyteToBool(prescription.Afternoon2),
                Evening2 = ConvertSbyteToBool(prescription.Evening2),
                Medicine3 = prescription.Medicine3,
                Morning3 = ConvertSbyteToBool(prescription.Morning3),
                Afternoon3 = ConvertSbyteToBool(prescription.Afternoon3),
                Evening3 = ConvertSbyteToBool(prescription.Evening3),
                Medicine4 = prescription.Medicine4,
                Morning4 = ConvertSbyteToBool(prescription.Morning4),
                Afternoon4 = ConvertSbyteToBool(prescription.Afternoon4),
                Evening4 = ConvertSbyteToBool(prescription.Evening4),
                CheckUpAfterDays = (int)prescription.Checkupafterdays
            };
            return View(model);
        }

        private static bool ConvertSbyteToBool(short? value)
        {
            return value.HasValue && value.Value != 0;
        }


        [HttpPost]
        public async Task<IActionResult> EditPrescription(EditPrescriptionViewModel model)
        {
            AlertViewModel alert;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var prescription = await _prescriptionService.GetPrescriptionById(model.Id);
                if (prescription == null)
                {
                    alert = new AlertViewModel { Message = "Failed to get Prescription Details" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View(model);
                }

                prescription.Medicaltest1 = model.MedicalTest1;
                prescription.Medicaltest2 = model.MedicalTest2;
                prescription.Medicine1 = model.Medicine1;
                prescription.Morning1 = ConvertBoolToSbyte(model.Morning1);
                prescription.Afternoon1 = ConvertBoolToSbyte(model.Afternoon1);
                prescription.Evening1 = ConvertBoolToSbyte(model.Evening1);
                prescription.Medicine2 = model.Medicine2;
                prescription.Morning2 = ConvertBoolToSbyte(model.Morning2);
                prescription.Afternoon2 = ConvertBoolToSbyte(model.Afternoon2);
                prescription.Evening2 = ConvertBoolToSbyte(model.Evening2);
                prescription.Medicine3 = model.Medicine3;
                prescription.Morning3 = ConvertBoolToSbyte(model.Morning3);
                prescription.Afternoon3 = ConvertBoolToSbyte(model.Afternoon3);
                prescription.Evening3 = ConvertBoolToSbyte(model.Evening3);
                prescription.Medicine4 = model.Medicine4;
                prescription.Morning4 = ConvertBoolToSbyte(model.Morning4);
                prescription.Afternoon4 = ConvertBoolToSbyte(model.Afternoon4);
                prescription.Evening4 = ConvertBoolToSbyte(model.Evening4);
                prescription.Checkupafterdays = model.CheckUpAfterDays;
                prescription.Createddate = DateTime.Now;

                var res = await _prescriptionService.UpdatePrescription(prescription);

                if (res == false)
                {
                    alert = new AlertViewModel { Message = "Failed to Update Prescription" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View(model);
                }
                else
                {
                    alert = new AlertViewModel { IsSuccess = true, Message = "Prescription Updated Successfully" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return RedirectToAction("ListOfPrescriptions");
                }
            }
            catch (Exception ex)
            {
                alert = new AlertViewModel { Message = "Failed to Update Prescription" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var response = await _prescriptionService.DeletePrescriotion(id);
            if (response == true)
            {
                //Alert alert = new Alert { IsSuccess = true, Message = "Successfully Deleted Prescription" };
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

        public async Task<IActionResult> ViewPrescription(int id)
        {
            var prescription = await _prescriptionService.GetPrescriptionById(id);
            var doctorDetails = await _doctorService.GetDoctorById((int)prescription.Doctorid);
            var patientDetails = await _patientService.GetPatientById((int)prescription.Patientid);
            var patientAge = CalculateAge(patientDetails.Dateofbirth);
            var userDB = await _userService.GetUserByEmail(patientDetails.Email);
            var model = new PrescriptionViewModel()
            {
                Id = prescription.Id,
                DoctorName = doctorDetails.Name,
                DoctorSpecialization = doctorDetails.Specilization,
                PatientName = patientDetails.Name,
                PatientAge = patientAge,
                PatientGender = patientDetails.Gender,
                PatientEmailAddress = patientDetails.Email, 
                PatientPhoneNo = userDB.Phonenumber,
                MedicalTest1 = prescription.Medicaltest1,
                MedicalTest2 = prescription.Medicaltest2,
                Medicine1 = prescription.Medicine1,
                Morning1 = ConvertSbyteToBool(prescription.Morning1),
                Afternoon1 = ConvertSbyteToBool(prescription.Afternoon1),
                Evening1 = ConvertSbyteToBool(prescription.Evening1),
                Medicine2 = prescription.Medicine2,
                Morning2 = ConvertSbyteToBool(prescription.Morning2),
                Afternoon2 = ConvertSbyteToBool(prescription.Afternoon2),
                Evening2 = ConvertSbyteToBool(prescription.Evening2),
                Medicine3 = prescription.Medicine3,
                Morning3 = ConvertSbyteToBool(prescription.Morning3),
                Afternoon3 = ConvertSbyteToBool(prescription.Afternoon3),
                Evening3 = ConvertSbyteToBool(prescription.Evening3),
                Medicine4 = prescription.Medicine4,
                Morning4 = ConvertSbyteToBool(prescription.Morning4),
                Afternoon4 = ConvertSbyteToBool(prescription.Afternoon4),
                Evening4 = ConvertSbyteToBool(prescription.Evening4),
                CheckUpAfterDays = (int)prescription.Checkupafterdays,
                PrescriptionAddDate = (DateTime)prescription.Createddate,

            };
            return View(model);
        }

        public async Task<IActionResult> ViewPrescriptionPdf(int id)
        {
            var prescription = await _prescriptionService.GetPrescriptionById(id);
            var doctorDetails = await _doctorService.GetDoctorById((int)prescription.Doctorid);
            var patientDetails = await _patientService.GetPatientById((int)prescription.Patientid);
            var patientAge = CalculateAge(patientDetails.Dateofbirth);
            var userDB = await _userService.GetUserByEmail(patientDetails.Email);
            var model = new PrescriptionViewModel()
            {
                Id = prescription.Id,
                DoctorName = doctorDetails.Name,
                DoctorSpecialization = doctorDetails.Specilization,
                PatientName = patientDetails.Name,
                PatientAge = patientAge,
                PatientGender = patientDetails.Gender,
                PatientEmailAddress = patientDetails.Email,
                PatientPhoneNo = userDB.Phonenumber,
                MedicalTest1 = prescription.Medicaltest1,
                MedicalTest2 = prescription.Medicaltest2,
                Medicine1 = prescription.Medicine1,
                Morning1 = ConvertSbyteToBool(prescription.Morning1),
                Afternoon1 = ConvertSbyteToBool(prescription.Afternoon1),
                Evening1 = ConvertSbyteToBool(prescription.Evening1),
                Medicine2 = prescription.Medicine2,
                Morning2 = ConvertSbyteToBool(prescription.Morning2),
                Afternoon2 = ConvertSbyteToBool(prescription.Afternoon2),
                Evening2 = ConvertSbyteToBool(prescription.Evening2),
                Medicine3 = prescription.Medicine3,
                Morning3 = ConvertSbyteToBool(prescription.Morning3),
                Afternoon3 = ConvertSbyteToBool(prescription.Afternoon3),
                Evening3 = ConvertSbyteToBool(prescription.Evening3),
                Medicine4 = prescription.Medicine4,
                Morning4 = ConvertSbyteToBool(prescription.Morning4),
                Afternoon4 = ConvertSbyteToBool(prescription.Afternoon4),
                Evening4 = ConvertSbyteToBool(prescription.Evening4),
                CheckUpAfterDays = (int)prescription.Checkupafterdays,
                PrescriptionAddDate = (DateTime)prescription.Createddate,

            };
            return View(model);
        }

        public int CalculateAge(string birthdate)
        {
            try
            {
                DateTime dob = DateTime.Parse(birthdate);

                DateTime today = DateTime.Today;
                int age = today.Year - dob.Year;

                if (dob.Date > today.AddYears(-age)) age--;

                return age;
            }
            catch(Exception ex)
            {
                return 0;
            }
        }

        [HttpGet]
        public async Task<ActionResult> GeneratePDF(int id)
        {
            try
            {
                // Fetch prescription details
                var prescription = await _prescriptionService.GetPrescriptionById(id);
                var doctorDetails = await _doctorService.GetDoctorById((int)prescription.Doctorid);
                var patientDetails = await _patientService.GetPatientById((int)prescription.Patientid);
                var patientAge = CalculateAge(patientDetails.Dateofbirth);
                var userDB = await _userService.GetUserByEmail(patientDetails.Email);

                // Map to ViewModel
                var model = new PrescriptionViewModel()
                {
                    Id = prescription.Id,
                    DoctorName = doctorDetails.Name,
                    DoctorSpecialization = doctorDetails.Specilization,
                    PatientName = patientDetails.Name,
                    PatientAge = patientAge,
                    PatientGender = patientDetails.Gender,
                    PatientEmailAddress = patientDetails.Email,
                    PatientPhoneNo = userDB.Phonenumber,
                    MedicalTest1 = prescription.Medicaltest1,
                    MedicalTest2 = prescription.Medicaltest2,
                    Medicine1 = prescription.Medicine1,
                    Morning1 = ConvertSbyteToBool(prescription.Morning1),
                    Afternoon1 = ConvertSbyteToBool(prescription.Afternoon1),
                    Evening1 = ConvertSbyteToBool(prescription.Evening1),
                    Medicine2 = prescription.Medicine2,
                    Morning2 = ConvertSbyteToBool(prescription.Morning2),
                    Afternoon2 = ConvertSbyteToBool(prescription.Afternoon2),
                    Evening2 = ConvertSbyteToBool(prescription.Evening2),
                    Medicine3 = prescription.Medicine3,
                    Morning3 = ConvertSbyteToBool(prescription.Morning3),
                    Afternoon3 = ConvertSbyteToBool(prescription.Afternoon3),
                    Evening3 = ConvertSbyteToBool(prescription.Evening3),
                    Medicine4 = prescription.Medicine4,
                    Morning4 = ConvertSbyteToBool(prescription.Morning4),
                    Afternoon4 = ConvertSbyteToBool(prescription.Afternoon4),
                    Evening4 = ConvertSbyteToBool(prescription.Evening4),
                    CheckUpAfterDays = (int)prescription.Checkupafterdays,
                    PrescriptionAddDate = (DateTime)prescription.Createddate
                };

                // Render View to HTML
                var partialName = "/Views/Prescription/ViewPrescriptionPdf.cshtml";
                var htmlContent = _razorRendererHelper.RenderPartialToString(partialName, model);
                byte[] pdfBytes = _dataExportService.GeneratePdf(htmlContent);

                // Return the PDF as a downloadable file
                var userClaims = ((ClaimsIdentity)User.Identity).Claims;
                var suid = userClaims.LastOrDefault(c => c.Type == "Suid")?.Value;
                var accessToken = userClaims.LastOrDefault(c => c.Type == "Access_Token")?.Value;

                // Initialize the SigningServiceNewDTO
                var data = new SignatureCoordinatesDTO()
                {
                    documentType = "PADES",
                    subscriberUniqueId = suid
                };

                var placeHolderCoordinates = new placeHolderCoordinates
                {
                    pageNumber = _configuration["PageNumber"],
                    signatureXaxis = _configuration["SignatureXaxis"],
                    signatureYaxis = _configuration["SignatureYaxis"],
                    imgHeight = _configuration["SignatureImageHeight"],
                    imgWidth = _configuration["SignatureImageWidth"]
                };
                var esealPlaceHolderCoordinates = new esealplaceHolderCoordinates
                {
                    pageNumber = _configuration["EsealPageNumber"],
                    signatureXaxis = _configuration["EsealSignatureXaxis"],
                    signatureYaxis = _configuration["EsealSignatureYaxis"],
                    imgHeight = _configuration["EsealImageHeight"],
                    imgWidth = _configuration["EsealImageWidth"]
                };

                data.esealPlaceHolderCoordinates = esealPlaceHolderCoordinates;
                data.placeHolderCoordinates = placeHolderCoordinates;

                var fileName = "Prescription.pdf";
                string url = _configuration[$"SignatureUrl"];
                var TokenHeaderName = _configuration["dtidp:AuthorizationHeader"];

                using (var httpClient = new HttpClient())
                {
                    using (var multipartFormContent = new MultipartFormDataContent())
                    {
                        httpClient.DefaultRequestHeaders.Add(TokenHeaderName, "Bearer " + accessToken);

                        // Add the file content
                        var fileContent = new ByteArrayContent(pdfBytes);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                        multipartFormContent.Add(fileContent, "file", fileName);

                        // Add the JSON data
                        var jsonData = JsonConvert.SerializeObject(data);
                        multipartFormContent.Add(new StringContent(jsonData, Encoding.UTF8, "application/json"), "model");

                        HttpResponseMessage response = await httpClient.PostAsync(url, multipartFormContent);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<APIResponse>(responseBody);
                            if (result != null && result.Success)
                            {
                                _logger.LogError("Pdf Signed Successfully");
                                return Json(new { status = "Success", message = "PDF generated successfully", fileContent = result.Result });
                            }
                            else
                            {
                                _logger.LogError("Failed to sign : " + result.Message.ToString());
                                return Json(new { status = "Failed", message = result?.Message, details = "PDF generation failed" });
                            }
                        }
                        else
                        {
                            _logger.LogError($"Request to {url} failed with status code {response.StatusCode}");
                            var errorBody = await response.Content.ReadAsStringAsync();
                            _logger.LogError("Failed to sign : " + errorBody.ToString());
                            return Json(new { status = "Failed", message = "API call failed", details = errorBody });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to sign : " + ex.ToString());
                return Json(new { status = "Failed", message = "An unexpected error occurred", details = ex.Message });
            }
        }




        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor);
            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);
                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} not found.");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var tempData = new TempDataDictionary(HttpContext, _tempDataProvider);
                var viewContext = new ViewContext(actionContext, viewResult.View, viewDictionary, tempData, sw, new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }



        //private async Task<string> RenderRazorViewToString(string viewName, object model)
        //{
        //    ViewData.Model = model;
        //    using (var writer = new StringWriter())
        //    {
        //        var viewResult = ViewEngine.FindView(ControllerContext, viewName, false);
        //        if (!viewResult.Success)
        //        {
        //            throw new FileNotFoundException("View not found.");
        //        }

        //        var viewContext = new ViewContext(
        //            ControllerContext,
        //            viewResult.View,
        //            ViewData,
        //            TempData,
        //            writer,
        //            new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions()
        //        );

        //        await viewResult.View.RenderAsync(viewContext);
        //        return writer.GetStringBuilder().ToString();
        //    }
        //}


        //private string RenderViewToString(string viewName, object model)
        //{
        //    try
        //    {
        //        ViewData.Model = model;
        //        using (var writer = new StringWriter())
        //        {
        //            var viewResult = ViewEngines.Engines.FindView(ControllerContext, viewName, null);
        //            if (viewResult.View != null)
        //            {
        //                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, writer);
        //                viewResult.View.Render(viewContext, writer);
        //                return writer.ToString();
        //            }
        //            else
        //            {
        //                throw new Exception($"View '{viewName}' not found.");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }

        //}

        public async Task<ActionResult> SendPDF(int id)
        {
            try
            {
                // Fetch prescription details
                var prescription = await _prescriptionService.GetPrescriptionById(id);
                var doctorDetails = await _doctorService.GetDoctorById((int)prescription.Doctorid);
                var patientDetails = await _patientService.GetPatientById((int)prescription.Patientid);
                var patientAge = CalculateAge(patientDetails.Dateofbirth);
                var userDB = await _userService.GetUserByEmail(patientDetails.Email);

                // Map to ViewModel
                var model = new PrescriptionViewModel()
                {
                    Id = prescription.Id,
                    DoctorName = doctorDetails.Name,
                    DoctorSpecialization = doctorDetails.Specilization,
                    PatientName = patientDetails.Name,
                    PatientAge = patientAge,
                    PatientGender = patientDetails.Gender,
                    PatientEmailAddress = patientDetails.Email,
                    PatientPhoneNo = userDB.Phonenumber,
                    MedicalTest1 = prescription.Medicaltest1,
                    MedicalTest2 = prescription.Medicaltest2,
                    Medicine1 = prescription.Medicine1,
                    Morning1 = ConvertSbyteToBool(prescription.Morning1),
                    Afternoon1 = ConvertSbyteToBool(prescription.Afternoon1),
                    Evening1 = ConvertSbyteToBool(prescription.Evening1),
                    Medicine2 = prescription.Medicine2,
                    Morning2 = ConvertSbyteToBool(prescription.Morning2),
                    Afternoon2 = ConvertSbyteToBool(prescription.Afternoon2),
                    Evening2 = ConvertSbyteToBool(prescription.Evening2),
                    Medicine3 = prescription.Medicine3,
                    Morning3 = ConvertSbyteToBool(prescription.Morning3),
                    Afternoon3 = ConvertSbyteToBool(prescription.Afternoon3),
                    Evening3 = ConvertSbyteToBool(prescription.Evening3),
                    Medicine4 = prescription.Medicine4,
                    Morning4 = ConvertSbyteToBool(prescription.Morning4),
                    Afternoon4 = ConvertSbyteToBool(prescription.Afternoon4),
                    Evening4 = ConvertSbyteToBool(prescription.Evening4),
                    CheckUpAfterDays = (int)prescription.Checkupafterdays,
                    PrescriptionAddDate = (DateTime)prescription.Createddate
                };

                var partialName = "/Views/Prescription/ViewPrescriptionPdf.cshtml";
                var htmlContent = _razorRendererHelper.RenderPartialToString(partialName, model);
                var redirectBackUrl = _configuration[$"RedirectUrl"];
                byte[] pdfBytes = _dataExportService.GeneratePdf(htmlContent);
                string base64Pdf = Convert.ToBase64String(pdfBytes);

                var userClaims = ((ClaimsIdentity)User.Identity).Claims;
                var suid = userClaims.LastOrDefault(c => c.Type == "Suid")?.Value;
                var accessToken = userClaims.LastOrDefault(c => c.Type == "Access_Token")?.Value;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Suid", suid);
                    client.DefaultRequestHeaders.Add("Access-Token", accessToken);

                    var payload = new
                    {
                        documentName = "Prescription_" + model.PatientName,
                        base64Document = base64Pdf,
                        fileExtension = "pdf",
                        redirectUrl = redirectBackUrl,
                        recipient = new
                        {
                            name = model.PatientName,
                            email = model.PatientEmailAddress,
                            phone = model.PatientPhoneNo
                        },
                        meta = new
                        {
                            prescriptionId = model.Id,
                            createdDate = model.PrescriptionAddDate,
                            doctorName = model.DoctorName
                        },
                        status = "sent"
                    };

                    var jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var createEnvelopeUrl = _configuration[$"EnvelopeUrl"];

                    var response = await client.PostAsync(createEnvelopeUrl, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        string signingUrl = responseObject.signingUrl;
                        return Json(new { status = "Success", redirectUrl = signingUrl });
                    }
                    else
                    {
                        _logger.LogError($"Envelope API Failed: {response.StatusCode}, Response: {responseString}");
                        return Json(new
                        {
                            status = "Failed",
                            message = "Envelope API call failed.",
                            httpStatus = (int)response.StatusCode,
                            apiResponse = responseString
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to sign: " + ex.ToString());
                return Json(new
                {
                    status = "Failed",
                    message = "An unexpected error occurred.",
                    details = ex.Message
                });
            }
        }



        [HttpGet]
        public async Task<IActionResult> DisplaySignedPdf(string envelopeId)
        {
            if (string.IsNullOrEmpty(envelopeId))
            {
                return BadRequest("Missing envelope ID.");
            }

            if (!Regex.IsMatch(envelopeId, "^[a-zA-Z0-9-]+$"))
            {
                return BadRequest("Invalid envelope ID format.");
            }

            var client = new HttpClient();
            var baseUrl = _configuration[$"GetSignedDocument"];
            var apiUrl = $"{baseUrl}?envelopeId={envelopeId}";
            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var envelope = JsonConvert.DeserializeObject<SigningEnvelope>(jsonString);

                if (envelope != null && !string.IsNullOrEmpty(envelope.DocumentBase64))
                {
                    List<PrescriptionListViewModel> list = new List<PrescriptionListViewModel>();
                    var doctorUUID = User.Claims.FirstOrDefault(c => c.Type == "uuid")?.Value;
                    var doctor = await _doctorService.GetDoctorDetailsByUUID(doctorUUID);
                    int doctorId = doctor.Id;
                    ViewBag.Base64Pdf = envelope.DocumentBase64;
                    var prescriptionsData = await _prescriptionService.ListAllPrescriptionsOfDoctorAsync(doctorId);

                    var patientDict = new Dictionary<string, string>();
                    var patients = await _patientService.ListAllPatientsAsync();
                    foreach (var patient in patients)
                    {
                        patientDict[patient.Id.ToString()] = patient.Name;
                    }

                    foreach (var prescription in prescriptionsData)
                    {
                        DateTime? nullableDateTime = prescription.Createddate;
                        DateTime? dateOnly = nullableDateTime?.Date;

                        var prescriptionDetails = new PrescriptionListViewModel()
                        {
                            Id = prescription.Id,
                            Name = patientDict[prescription.Patientid.ToString()],
                            CreatedDate = dateOnly?.ToString("dd-MM-yyyy")
                        };
                        list.Add(prescriptionDetails);
                    }
                    return View("ListOfPrescriptions", list);
                }
            }

            ViewBag.Error = "Failed to fetch the signed PDF.";
            return RedirectToAction("ListOfPrescriptions", "Prescription");
        }
    }
}
