using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Web.ViewModel.DashBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Web.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly IAppointmentService _appointmentService;

        private readonly ILogger<DashBoardController> _logger;

        public DashBoardController(IDoctorService doctorService, IPatientService patientService,
            IPrescriptionService prescriptionService, IAppointmentService appointmentService,
            ILogger<DashBoardController> logger)
        {
            _doctorService = doctorService;
            _patientService = patientService;
            _prescriptionService = prescriptionService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("This is log");
            var model = new DashboardViewModel();
            model.PrescriptionCount = await _prescriptionService.GetPrescriptionsCount();
            model.PatientCount = await _patientService.GetPatientsCount();
            model.DoctorCount = await _doctorService.GetDoctorsCount();
            model.ActiveAppointmentsCount = await _appointmentService.GetCountOfActiveAppointments();
            model.PendingAppointmentsCount = await _appointmentService.GetCountOfPendingAppointments();

            return View(model);
        }
    }
}
