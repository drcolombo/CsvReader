using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CsvReader.WebDemo.Models;

namespace CsvReader.WebDemo.Controllers
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
            return View(new FileUpload());
        }

        [HttpPost]
        public IActionResult Upload(FileUpload fileUpload)
        {
            if (ModelState.IsValid)
            {
                var fileName = Path.GetTempFileName();

                // Create new local file and copy contents of uploaded file
                using (var localFile = System.IO.File.OpenWrite(fileName))
                using (var uploadedFile = fileUpload.FormFile.OpenReadStream())
                {
                    uploadedFile.CopyTo(localFile);
                }

                using (var csv = new CsvReader(new StreamReader(fileName), true, fileUpload.Separator))
                {
                    csv.VirtualColumns.Add(new Column { Name = "AmountOfChildren", DefaultValue = "1", Type = typeof(int), NumberStyles = NumberStyles.Integer });
                    csv.VirtualColumns.Add(new Column { Name = "Sex", DefaultValue = "M", Type = typeof(string) });
                    IDataReader reader = csv;
                    var schema = reader.GetSchemaTable();
                    csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                    fileUpload.Data.Clear();
                    fileUpload.Data.Load(reader);
                }
                System.IO.File.Delete(fileName);


                return View("Index", fileUpload);
            }
            else
            {
                return View("Index");
            }
            //var content = string.Empty;
            //using (var sr = new StreamReader(fileUpload.FormFile.OpenReadStream()))
            //{
            //    using (CsvReader csv = new CsvReader(sr, true))
            //    {
            //        rptSampleData.DataSource = csv;
            //        rptSampleData.DataBind();
            //    }
            //}
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
