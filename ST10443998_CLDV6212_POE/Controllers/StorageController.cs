using Microsoft.AspNetCore.Mvc;
using ST10443998_CLDV6212_POE.Models;
using ST10443998_CLDV6212_POE.Services;
using System.Reflection.Metadata.Ecma335;
using static System.Net.Mime.MediaTypeNames;

namespace ST10443998_CLDV6212_POE.Controllers
{
    public class StorageController : Controller
    {
        private readonly BlobImageService _blobSvc;
        private readonly FileContractService _fileSvc;
        private readonly OrderQueueService _queueSvc;
        private readonly CustomerTableService _tableSvc;
        public StorageController(
        BlobImageService blobSVC
        , FileContractService fileSvc,
        OrderQueueService queueSvc,
        CustomerTableService tableSvc)
        {
            _blobSvc = blobSVC;
            _fileSvc = fileSvc;
            _queueSvc = queueSvc;
            _tableSvc = tableSvc;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new StorageDashboardVm
            {
                Customers = await _tableSvc.ListAsync(500),
                Blobs = await _blobSvc.ListAsync(),
                Contracts = await _fileSvc.ListAsync(),
                QueueMessages = await _queueSvc.PeekAsync(32)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            try
            {
                var url = await _blobSvc.UploadImageAsync(image);
                TempData["BlobMsg"] = $"Image Uploaded";
            }
            catch (Exception ex) { TempData["BlobMsg"] = $"Error: {ex.Message}"; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadContract(IFormFile contract)
        {
            try
            {
                await _fileSvc.UploadAsync(contract);
                TempData["FileMsg"] = $"Contract Uploaded";
            }
            catch (Exception ex) { TempData["FileMsg"] = $"Error: {ex.Message}"; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enqueue(string orderDescription)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(orderDescription))
                {
                    await _queueSvc.EnqueueAsync(orderDescription);
                    TempData["QueueMsg"] = $"Order message queued";
                }
                else TempData["QueueMsg"] = "Please enter. ";
            }
            catch (Exception ex) { TempData["QueueMsg"] = $"Error: {ex.Message}"; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomer(string firstName, string lastName, string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["TableMsg"] = "Email is required";
                }
                else
                {
                    await _tableSvc.AddCustomerAsync(new CustomerEntity
                    {
                        FirstName = firstName?.Trim() ?? "",
                        LastName = lastName?.Trim() ?? "",
                        Email = email?.Trim() ?? ""
                    });
                    TempData["TableMsg"] = "Customer saved";
                }
            }
            catch (Exception ex) { TempData["TableMsg"] = $"Error: {ex.Message}"; }
            return RedirectToAction(nameof(Index));
        }
    }
}