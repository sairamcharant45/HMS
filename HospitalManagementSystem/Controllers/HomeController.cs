using HospitalManagementSystem.Models;
using HospitalManagementSystem.Web.ViewModel.DashBoard;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel();
            model.PrescriptionCount = 0;
            model.PatientCount = 0;
            model.DoctorCount = 0;
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
