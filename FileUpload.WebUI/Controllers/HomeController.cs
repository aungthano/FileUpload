using FileUpload.Core;
using FileUpload.Core.Entities;
using FileUpload.Core.Services;
using FileUpload.WebUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUpload.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CSVFileService _csvFileService;
        private readonly XMLFileService _xmlFileService;
        private readonly InvoiceTransService _invoiceTransService;

        public HomeController(
            ILogger<HomeController> logger, 
            CSVFileService csvfileService,
            XMLFileService xmlFileService,
            InvoiceTransService invoiceTransService)
        {
            _logger = logger;
            _csvFileService = csvfileService;
            _xmlFileService = xmlFileService;
            _invoiceTransService = invoiceTransService;
        }

        public IActionResult Index()
        {
            return View();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // file validations
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please choose a file before upload!";
                return RedirectToAction("Index");
            }

            int maxFileSizeInMB = 1;
            long fileSizeInMB = file.Length / (1024 * 1024);
            if (fileSizeInMB > maxFileSizeInMB)
            {
                TempData["Error"] = $"Failed to upload file! { System.Environment.NewLine } Maximum allowed file size is { maxFileSizeInMB } MB.";
                return RedirectToAction("Index");
            }

            string[] allowFileExts = { ".csv", ".xml" };
            var fileExt = Path.GetExtension(file.FileName);
            if (!allowFileExts.Contains(fileExt.ToLower()))
            {
                TempData["Error"] = $"Unknown Format!";
                return RedirectToAction("Index");
            }

            // save file to server path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // read file
            MessageResult<List<InvoiceTrans>> fileReadStatus = null;
            if (fileExt == ".csv")
            {
                fileReadStatus = _csvFileService.ReadCSVFile<InvoiceTrans>(filePath);
            }
            else
            {
                fileReadStatus = _xmlFileService.ReadXMLFile<InvoiceTrans>(filePath);
            }

            // log error and return bad request
            if (!fileReadStatus.IsSucceed)
            {
                string errorLog = $"\nError at - { DateTime.Now }\nFile Path - { filePath }\nError Details{ fileReadStatus.Message }";
                _logger.LogInformation(errorLog);
                return BadRequest(fileReadStatus.Message);
            }

            // save data into database
            var createInvTransStatus = _invoiceTransService.CreateInvoiceTrans(fileReadStatus.Result);
            if (!createInvTransStatus.IsSucceed)
            {
                string dbErrorLog = $"\nError at - { DateTime.Now }\nDatabase Error\nError Details{ createInvTransStatus.Message }"; 
                _logger.LogInformation(dbErrorLog);
                return BadRequest(createInvTransStatus.Message);
            }

            //TempData["Success"] = "Successfully Saved Data";
            //return RedirectToAction("Index");
            return Ok("Successfully Saved Data");
        }
    }
}
