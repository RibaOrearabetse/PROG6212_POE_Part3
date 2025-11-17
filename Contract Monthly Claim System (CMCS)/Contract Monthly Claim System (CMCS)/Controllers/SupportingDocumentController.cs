using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Models;
using System.Text.Json;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class SupportingDocumentController : Controller
    {
        private static List<SupportingDocument> _documents = new List<SupportingDocument>();
        private static int _nextDocumentId = 1;
        private readonly IWebHostEnvironment _environment;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx", ".doc", ".xls" };
        private static readonly string DataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "documents.json");

        public SupportingDocumentController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            // Load data from file if not already loaded
            if (!_documents.Any())
            {
                LoadDocumentsFromFile();

                // If still no documents after loading, initialize with sample data
                if (!_documents.Any())
                {
                    _documents.AddRange(GetSampleDocuments());
                    _nextDocumentId = _documents.Max(d => d.DocumentID) + 1;
                    SaveDocumentsToFile();
                }
            }

            var documents = _documents.OrderByDescending(d => d.UploadDate).ToList();
            return View(documents);
        }
        public IActionResult Details(int id)
        {
            // Initialize with sample documents if no documents exist
            if (!_documents.Any())
            {
                _documents.AddRange(GetSampleDocuments());
                _nextDocumentId = _documents.Max(d => d.DocumentID) + 1;
            }

            var document = _documents.FirstOrDefault(d => d.DocumentID == id);
            if (document == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }
        public IActionResult Upload(int claimId)
        {
            ViewBag.ClaimID = claimId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(int claimId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    TempData["ErrorMessage"] = "Please select a file to upload.";
                    return RedirectToAction("Upload", new { claimId });
                }

                // Validate file size
                if (file.Length > MaxFileSize)
                {
                    TempData["ErrorMessage"] = $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB.";
                    return RedirectToAction("Upload", new { claimId });
                }

                // Validate file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = $"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}";
                    return RedirectToAction("Upload", new { claimId });
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create document record
                var document = new SupportingDocument
                {
                    DocumentID = _nextDocumentId++,
                    FileName = file.FileName,
                    FilePath = $"/uploads/documents/{fileName}",
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    UploadDate = DateTime.Now,
                    ClaimID = claimId
                };

                _documents.Add(document);

                // Save to file
                SaveDocumentsToFile();

                TempData["SuccessMessage"] = $"File '{file.FileName}' uploaded successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error uploading file: {ex.Message}";
                return RedirectToAction("Upload", new { claimId });
            }
        }

        public IActionResult GetDocumentsForClaim(int claimId)
        {
            var documents = _documents.Where(d => d.ClaimID == claimId).ToList();
            return Json(documents);
        }
        private List<SupportingDocument> GetSampleDocuments()
        {
            return new List<SupportingDocument>
            {
                new SupportingDocument { DocumentID = 1, FileName = "timesheet_jan.pdf", FilePath = "/documents/timesheet_jan.pdf", UploadDate = DateTime.Now.AddDays(-5), ClaimID = 1 },
                new SupportingDocument { DocumentID = 2, FileName = "contract_agreement.pdf", FilePath = "/documents/contract_agreement.pdf", UploadDate = DateTime.Now.AddDays(-12), ClaimID = 2 },
                new SupportingDocument { DocumentID = 3, FileName = "work_log.xlsx", FilePath = "/documents/work_log.xlsx", UploadDate = DateTime.Now.AddDays(-18), ClaimID = 3 },
                new SupportingDocument { DocumentID = 4, FileName = "invoice_feb.pdf", FilePath = "/documents/invoice_feb.pdf", UploadDate = DateTime.Now.AddDays(-1), ClaimID = 4 }
            };
        }

        // File persistence methods
        private static void SaveDocumentsToFile()
        {
            try
            {
                // Ensure the Data directory exists
                var dataDir = Path.GetDirectoryName(DataFilePath);
                if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_documents, options);
                System.IO.File.WriteAllText(DataFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving documents to file: {ex.Message}");
            }
        }

        private static void LoadDocumentsFromFile()
        {
            try
            {
                if (System.IO.File.Exists(DataFilePath))
                {
                    var json = System.IO.File.ReadAllText(DataFilePath);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var documents = JsonSerializer.Deserialize<List<SupportingDocument>>(json);
                        if (documents != null && documents.Any())
                        {
                            _documents = documents;
                            _nextDocumentId = _documents.Max(d => d.DocumentID) + 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading documents from file: {ex.Message}");
            }
        }
    }
}