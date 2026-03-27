using HospitalManagementSystem.Web.Helper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using HospitalManagementSystem.Core.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using StackExchange.Redis;
using System.Security.Claims;

namespace HospitalManagementSystem.Web.Controllers
{
    [Authorize]
    public class LogoutController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IdpClient idpClient;

        //private readonly IDatabase _redisDb;

        private readonly ILogger<LogoutController> _logger;

        public LogoutController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<LogoutController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            idpClient = new IdpClient(configuration, _httpClientFactory);
            //_redisDb = redis.GetDatabase();
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Login");
            }

            /* Generate state value and store in session or somewhere else
               to validate logout responce from idp*/
            var state = Guid.NewGuid().ToString("N");

            HttpContext.Session.Remove("Nonce");
            HttpContext.Session.SetString("state", state);

            /* get id_token from session which we get from token endpoint at the 
             * time of login. this id_token we need to pass in logout url for
             * validate service provider logout request at idp side
             */
            var idToken = "";
            var isOpenId = _configuration.GetValue<bool>("OpenId_Connect");
            if (isOpenId)
            {
                idToken = HttpContext.User.Claims.FirstOrDefault(c => c.Type ==
                        "ID_Token").Value;
            }
            //generate idp logout url
            return Redirect(idpClient.GetLogoutUrl(idToken, state));
        }

        [HttpGet]
        public async Task<IActionResult> callback()
        {
            //if (!User.Identity.IsAuthenticated)
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            //var state = Guid.NewGuid().ToString("N");

            //HttpContext.Session.Remove("Nonce");
            //HttpContext.Session.SetString("state", state);
            // HttpContext.Session.Clear();

            var userSuid = User.Claims.FirstOrDefault(c => c.Type == "Suid")?.Value;
            if (userSuid != null && userSuid != "")
            {
                //bool keyExists = _redisDb.KeyExists($"{userSuid}-UserImage");

                //if (keyExists)
                //{
                //    // Key exists, remove the image from Redis
                //    bool removed = _redisDb.KeyDelete($"{userSuid}-UserImage");

                //    if (!removed)
                //    {
                //        return BadRequest("Failed to Logout.");
                //    }
                //}
            }

            if (User.Identity.IsAuthenticated && HttpContext.User.Claims.Count() != 0)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            HttpContext.Session.Clear();

            _logger.LogInformation("User Logout successfully");

            return RedirectToAction("Index", "Login");
        }
    }
}
