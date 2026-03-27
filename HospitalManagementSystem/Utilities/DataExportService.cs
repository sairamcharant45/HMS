using DinkToPdf.Contracts;
using DinkToPdf;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.Net;
using System.Text;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DinkToPdf;
using DinkToPdf.Contracts;



namespace HospitalManagementSystem.Web.Utilities
{
    public class DataExportService
    {

        private readonly IRazorRendererHelper _razorRendererHelper;
        private readonly IConverter _converter;
        private readonly IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly ILogger<DataExportService> _logger;
        private const string FOLDER_PATH = "Reports";

        public DataExportService(
            IConverter converter,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<DataExportService> logger,
            IRazorRendererHelper razorRendererHelper)
        {
 
            _converter = converter;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
            _razorRendererHelper = razorRendererHelper;
        }


        public byte[] GeneratePdf(string htmlContent)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 18, Bottom = 18 },
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" },
                //HeaderSettings = { FontSize = 10, Right = "Page [page] of [toPage]", Line = true },
                //FooterSettings = { FontSize = 8, Center = "PDF demo from JeminPro", Line = true },
            };

            var htmlToPdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings },
            };

            return _converter.Convert(htmlToPdfDocument);
        }

     
    }
}
