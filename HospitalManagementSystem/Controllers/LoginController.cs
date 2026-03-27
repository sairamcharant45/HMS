using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HospitalManagementSystem.Web.Helper;
using Newtonsoft.Json.Linq;
using HospitalManagementSystem.Core.DTOs;
using Newtonsoft.Json;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Web.ViewModel.Login;
using HospitalManagementSystem.Web.ViewModel;
using HospitalManagementSystem.Core.Domain.Models;
using StackExchange.Redis;
using HospitalManagementSystem.Core.Services;

namespace HospitalManagementSystem.Web.Controllers
{
    public class LoginController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;

        //private readonly IDatabase _redisDb;

        private readonly IdpClient openIDHelper;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IConfiguration configuration, IHttpClientFactory httpClientFactory,
            IUserService userService, IRoleService roleService,
            IPatientService patientService, IDoctorService doctorService,
            ILogger<LoginController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _userService = userService;
            _roleService = roleService;
            _doctorService = doctorService;
            openIDHelper = new IdpClient(_configuration, _httpClientFactory);
            //_redisDb = redis.GetDatabase();
            _patientService = patientService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            HttpContext.Session.Clear();
            string clientName = _configuration["Client"];
            ViewBag.clientName = clientName;
            return View();
        }

        public IActionResult IDPLogin()
        {
            try
            {
                /* Verify user is authenticated or not
                  1-:  if user not authenticated create IDP login url
                       with service provider details
                  2-:  if user is authenticated redirect to home page
                 */
                if (!HttpContext.User.Identity.IsAuthenticated)
                {

                    /* Generate nonce and state value and store in session or somewhere else
                     to validate authorization and token endpoint responce from idp*/
                    var state = Guid.NewGuid().ToString("N");
                    var nonce = Guid.NewGuid().ToString("N");

                    HttpContext.Session.SetString("Nonce", nonce);
                    HttpContext.Session.SetString("state", state);

                    //generate IDP login url
                    return Redirect(openIDHelper.GetAuthorizationUrl(nonce, state));
                }

                return RedirectToAction("Index", "Home");

            }
            catch (Exception e)
            {
                ViewBag.error = "Something Went wrong";
                ViewBag.error_description = e.Message;
                return View("CustomError");
            }
        }


        public async Task<IActionResult> callback()
        {
            try
            {
                _logger.LogError("Hit callback endpoint");
                if (!string.IsNullOrEmpty(Request.Query["error"]) && !string.IsNullOrEmpty(Request.Query["error_description"]))
                {
                    ViewBag.error = Request.Query["error"].ToString();
                    ViewBag.error_description = Request.Query["error_description"].ToString();
                    return View("Errorp");
                }
                _logger.LogError("Received code: " + Request.Query["code"].ToString());
                string code = Request.Query["code"].ToString();
                if (string.IsNullOrEmpty(code))
                {
                    ViewBag.error = "Invalid code";
                    ViewBag.error_description = "The code value is empty string or null";
                    return View("Errorp");
                }
                _logger.LogError("Exchanging code for access token");
                JObject TokenResponse = openIDHelper.GetAccessToken(code).Result;
                if (TokenResponse.ContainsKey("error") && TokenResponse.ContainsKey("error_description"))
                {
                    ViewBag.error = TokenResponse["error"].ToString();
                    ViewBag.error_description = TokenResponse["error_description"].ToString();
                    return View("Errorp");
                }
                _logger.LogError("Token response received: " + TokenResponse.ToString());
                UserSessionObj user = new UserSessionObj();

                var isOpenId = _configuration.GetValue<bool>("OpenId_Connect");

                var ID_Token = "";
                if (isOpenId)
                {

                    ID_Token = TokenResponse["id_token"].ToString();
                    _logger.LogError("ID Token received: " + ID_Token);
                    if (string.IsNullOrEmpty(ID_Token))
                    {
                        ViewBag.error = "Invalid code";
                        ViewBag.error_description = "The ID_Token value is empty string or null";
                        return View("Errorp");
                    }
                }

                var accessToken = TokenResponse["access_token"].ToString();
                if (string.IsNullOrEmpty(accessToken))
                {
                    ViewBag.error = "Invalid code";
                    ViewBag.error_description = "The code value is empty string or null";
                    return View("Errorp");
                }



                var expiresIn = (int)TokenResponse["expires_in"];

                if (isOpenId == true)
                {
                    //code for openid connect

                    //validate id_token and get cliam values from  id_token
                    ClaimsPrincipal userObj = openIDHelper.ValidateIdentityToken(ID_Token)
                        .Result;
                    _logger.LogError("User claims from ID Token: " + JsonConvert.SerializeObject(userObj.Claims.Select(c => new { c.Type, c.Value })));


                    if (userObj == null)
                    {
                        ViewBag.error = "Something went wrong ";
                        ViewBag.error_description = "Claim Object getting null value";
                        return View("Errorp");
                    }

                    //get nonce value from session which is send from idp login url
                    var Nonce = HttpContext.Session.GetString("Nonce");
                    if (string.IsNullOrEmpty(Nonce))
                    {
                        ViewBag.error = "Something went wrong ";
                        ViewBag.error_description = "Nonce value not found";
                        return View("Errorp");
                    }

                    //validate nonce value is matched with our nonce value
                    //which is send from login url
                    var nonce = userObj.FindFirst("nonce")?.Value ?? "";
                    if (!string.Equals(nonce, Nonce)) throw new Exception("invalid nonce");

                    var daesClaim = userObj.FindFirst("daes_claims")?.Value ?? "";



                    _logger.LogError("User Info from ID Token: " + daesClaim);

                    SubscriberProfileDTO userdata = JsonConvert.DeserializeObject<SubscriberProfileDTO>(daesClaim);
                    //user.Uuid = userdata?.Suid ?? "";
                    //user.fullname = userdata?.FullnameEN ?? "";
                    //user.mailId = userdata?.UnifiedId ?? "";
                    //user.gender = userdata?.Gender ?? "";
                    //user.mobileNo =  "";
                    //user.country = userdata?.NationalityEN ?? "";
                    //user.suid = userdata?.Suid ?? "";

                    user.Uuid = userObj.FindFirst("Suid")?.Value ?? "";
                    user.fullname = userObj.FindFirst("FullnameEN")?.Value ?? "";
                    user.mailId = userObj.FindFirst("FullnameEN")?.Value ?? "";
                    user.gender = userObj.FindFirst("Gender")?.Value ?? "";
                    user.mobileNo = "";
                    user.country = userObj.FindFirst("NationalityEN")?.Value ?? "";
                    user.suid = userObj.FindFirst("Suid")?.Value ?? "";


                    _logger.LogInformation("User Object", JsonConvert.SerializeObject(user));


                    if (!string.IsNullOrEmpty(user.mailId))
                    {
                        user.mailId = userdata?.Idn ?? "";

                        if (!string.IsNullOrEmpty(user.mailId))
                        {
                            user.mailId = userdata?.PassportNumber ?? "";
                        }

                    }

                    user.mailId = user.fullname;

                }
                else
                {
                    //code for oauth

                    JObject userObj = openIDHelper.GetUserInfo(accessToken).Result;
                    _logger.LogError("hioii");

                    _logger.LogInformation("User Info from UserInfo Endpoint: " + userObj.ToString());


                    if (userObj.ContainsKey("error") && userObj.ContainsKey("error_description"))
                    {
                        ViewBag.error = userObj["error"].ToString();
                        ViewBag.error_description = userObj["error_description"].ToString();
                        return View("Errorp");
                    }

                    //user.fullname = userObj["name"].ToString();                    
                    //user.dob = userObj["birthdate"].ToString();
                    //user.mailId = userObj["email"].ToString() ?? "";
                    //user.mobileNo = "";
                    //user.mobileNo = userObj["phone"].ToString();

                    user.suid = userObj["suid"].ToString() ?? "";
                    user.fullname = userObj["fullnameEN"].ToString() ?? "";
                    user.gender = userObj["gender"].ToString() ?? "";
                    user.mailId = userObj["unifiedId"].ToString() ?? "";
                    user.mobileNo = "";
                    user.country = userObj["nationalityEN"].ToString() ?? "";

                    if (!string.IsNullOrEmpty(user.mailId))
                    {
                        user.mailId = userObj["idn"].ToString() ?? "";

                        if (!string.IsNullOrEmpty(user.mailId))
                        {
                            user.mailId = userObj["passportNumber"].ToString() ?? "";
                        }
                    }
                    user.mailId = user.fullname;
                }

                var userIdentifier = user.fullname;

                _logger.LogInformation("User Identifier: " + userIdentifier);

                if (!string.IsNullOrEmpty(userIdentifier))
                {
                    ViewBag.error = "User Identifier is null";
                    ViewBag.error_description = "User Identifier is null";
                    return View("Errorp");
                }

                var userInDb = await _userService.GetUserByEmail(userIdentifier);

                _logger.LogError("User in db - ", JsonConvert.SerializeObject(userInDb));
                if (userInDb == null)
                {
                    var userid = Guid.NewGuid().ToString();
                    User newuser = new User()
                    {
                        Uuid = userid,
                        Email = user.mailId,
                        Password = "Demo@123",
                        Roleid = 3,
                        Name = user.fullname,
                        Phonenumber = ""
                    };

                    _logger.LogError("Failed login - ", user.mailId);

                    var response = await _userService.CreateUser(newuser);
                    if (response == false)
                    {
                        return NotFound("Failed to add User in DB");
                    }

                    string dobString = user.dob;

                    // Parse the string to DateTime
                    DateTime dob;
                    string formattedDob = "";

                    string[] formats = { "M/d/yyyy h:mm:ss tt", "MM/dd/yyyy h:mm:ss tt" };

                    if (DateTime.TryParseExact(dobString, formats, null, System.Globalization.DateTimeStyles.None, out dob))
                    {
                        formattedDob = dob.ToString("yyyy-MM-dd");
                    }

                    var patient = new Patient()
                    {
                        Userid = userid,
                        Name = user.fullname,
                        Email = user.mailId,
                        Gender = user.gender,
                        Dateofbirth = formattedDob,
                        Mobilenumber = user.mobileNo,
                        Country = user.country,
                        Bloodgroup = "O+"
                    };

                    response = await _patientService.AddPatient(patient);
                    if (response == false)
                    {
                        return NotFound("Failed to add User in Patient DB");
                    }

                    userInDb = newuser;
                }

                var role = await _roleService.GetRoleById((int)userInDb.Roleid);
                if (role == null)
                {
                    return NotFound();
                }
                user.suid = user.suid == null ? user.Uuid : user.suid;

                var identity = new ClaimsIdentity(new[] {
                    new Claim("Access_Token", accessToken),
                    new Claim(ClaimTypes.Name, user.fullname),
                    new Claim(ClaimTypes.NameIdentifier, user.suid),
                    new Claim(ClaimTypes.Email,user.mailId),
                    new Claim(ClaimTypes.Role,role),
                    new Claim("Suid", user.suid),
                    new Claim("UserRoleID",userInDb.Roleid.ToString()),
                    new Claim("Email",user.mailId),
                    new Claim("ExternalLogin","false"),
                    new Claim("uuid",userInDb.Uuid),
                    new Claim(ClaimTypes.UserData,user.suid.ToString()),
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                if (isOpenId)
                {
                    identity.AddClaim(new Claim("ID_Token", ID_Token));
                }


                var principal = new ClaimsPrincipal(identity);

                var properties = new AuthenticationProperties();
                properties.IsPersistent = true;
                properties.AllowRefresh = false;

                int SessionTimeOut = expiresIn;

                properties.ExpiresUtc = DateTime.UtcNow.AddSeconds(Convert.ToDouble(SessionTimeOut));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

                var basepath = _configuration.GetSection("BasePath").Value;

                string popupScript = $@"
    <script type='text/javascript'>
        if (window.opener) {{
            window.opener.location.href = '{basepath}/Dashboard/Index';
            window.close();
        }} else {{
            window.location.href = '{basepath}/Dashboard/Index';
        }}
    </script>";

                return Content(popupScript, "text/html");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in callback: " + e.Message);
                ViewBag.error = "Internal Error";
                ViewBag.error_description = "Something went wrong!";
                return View("Errorp");
            }
        }


        [Route("signout")]
        public IActionResult OpenIDLogout()
        {
            try
            {
                //clear session and authentication cookies
                var login = HttpContext.SignOutAsync(CookieAuthenticationDefaults
                            .AuthenticationScheme);
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                ViewBag.error = "Something Went Wrong!";
                ViewBag.error_description = e.Message;
                return View("CustomError");
            }
        }

        public async Task<IActionResult> ExternalLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var userInDb = await _userService.VerifyUserLogin(model.Email, model.Password);
            if (userInDb == null || userInDb.Success == false)
            {
                Alert alert = new Alert { Message = userInDb.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("Index", model);
            }

            var user = await _userService.GetUserByEmail(model.Email);
            if (user.Roleid == 2)
            {
                var doctorDetails = await _doctorService.GetDoctorDetailsByUUID(user.Uuid);
                if (doctorDetails.Status == "DEACTIVE")
                {
                    Alert alert = new Alert { Message = "Your account is Deactivated. Please contact Admin." };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("Index", model);
                }
            }
            var role = await _roleService.GetRoleById((int)user.Roleid);
            if (role == null)
            {
                return NotFound();
            }

            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Uuid),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role,role),
                new Claim("uuid", user.Uuid),
                new Claim("UserRoleID",user.Roleid.ToString()),
                new Claim("Email",user.Email),
                new Claim("ExternalLogin","true"),
                }, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties();
            properties.IsPersistent = true;
            properties.AllowRefresh = false;

            int SessionTimeOut = 3600;

            properties.ExpiresUtc = DateTime.UtcNow.AddSeconds(Convert.ToDouble(SessionTimeOut));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
