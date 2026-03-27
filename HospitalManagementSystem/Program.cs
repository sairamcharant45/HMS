using DinkToPdf.Contracts;
using DinkToPdf;
using HospitalManagementSystem.Core.Domain.Models;
using HospitalManagementSystem.Core.Domain.Repositories;
using HospitalManagementSystem.Core.Domain.Services;
using HospitalManagementSystem.Core.Persistence.Repositories;
using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Web.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using StackExchange.Redis;


var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("Init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureServices(builder);

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddNLogWeb();

    var app = builder.Build();

    logger.Info("WebApplication build successful");

    // For Proxy Servers
    string basePath = builder.Configuration["BasePath"];
    if (!string.IsNullOrEmpty(basePath))
    {
        app.Use(async (context, next) =>
        {
            context.Request.PathBase = basePath;
            await next.Invoke();
        });
    }

    // Configure the HTTP request pipeline.
    //if (!app.Environment.IsDevelopment())
    //{
    //    app.UseExceptionHandler("/Home/Error");
    //    app.UseHsts();
    //}

    app.UseDeveloperExceptionPage();
    app.UseForwardedHeaders();

    app.UseHttpsRedirection();
    app.UseStaticFiles();



    app.UseRouting();

    app.UseCors();

    // Rate Limiting Middleware
    app.UseRateLimiter();

    // Authentication and Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Session Middleware
    app.UseSession();

    // Map routes
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, ex.Message);
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddHttpClient();

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        // Only loopback proxies are allowed by default.
        // Clear that restriction because forwarders are enabled by explicit 
        // configuration.
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddCors();

    // Add services to the container.

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter(policyName: "fixed", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromSeconds(10);
            opt.QueueLimit = 5;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    builder.Services.AddControllersWithViews();

    builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();


    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Login/Index"; // Redirect path if not authenticated
            options.LogoutPath = "/Logout/Index";
            options.AccessDeniedPath = "/AccessDenied";  // Path to redirect when access is denied
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            options.SlidingExpiration = true;
        });

    builder.Services.AddAuthorization();


    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the timeout for sessions
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true; // Mark the session cookie as essential
    });

    var HMSConnectionString = builder.Configuration.GetConnectionString("HMSConnectionString");
    //builder.Services.AddDbContext<HospitalManagementSystemContext>(options =>
    //   options.UseMySql(HMSConnectionString,
    //   new MySqlServerVersion("8.0.12-mysql")));

    builder.Services.AddDbContext<HospitalManagementSystemContext>(options =>
    options.UseNpgsql(HMSConnectionString));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IDoctorService, DoctorService>();
    builder.Services.AddScoped<IPatientService, PatientService>();
    builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
    builder.Services.AddScoped<IRoleService, RoleService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IRazorRendererHelper, RazorRendererHelper>();

    var context = new CustomAssemblyLoadContext();
    if (builder.Environment.IsDevelopment())
    {
        context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
    }
    else
    {
        context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.so"));
    }
    builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
    builder.Services.AddScoped<DataExportService>();




    builder.Services.AddScoped<IAppointmentService, AppointmentService>();



    //var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");

    //// Add Redis connection
    //builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    //    ConnectionMultiplexer.Connect(redisConnectionString));
}
