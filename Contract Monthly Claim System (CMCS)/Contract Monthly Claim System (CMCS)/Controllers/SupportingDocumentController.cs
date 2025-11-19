using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class SupportingDocumentController : Controller
    {
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
            var documents = LoadDocuments()
                .OrderByDescending(d => d.UploadDate)
                .ToList();

            if (!documents.Any())
            {
                TempData["InfoMessage"] = "No supporting documents have been uploaded yet. Upload your first document to get started.";
            }

            return View(documents);
        }
        public IActionResult Details(int id)
        {
            var documents = LoadDocuments();

            var document = documents.FirstOrDefault(d => d.DocumentID == id);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        public IActionResult Upload(int claimId = 0)
        {
            var claims = ClaimController.GetAllClaims()?
                .Where(c => c != null)
                .OrderByDescending(c => c.SubmissionDate)
                .ToList() ?? new List<Claim>();

            var selectItems = new List<SelectListItem>();
            foreach (var claim in claims)
            {
                selectItems.Add(new SelectListItem
                {
                    Value = claim.ClaimID.ToString(),
                    Text = $"Claim #{claim.ClaimID} - {claim.ClaimStatus} ({claim.ClaimDate:dd MMM yyyy})",
                    Selected = claim.ClaimID == claimId && claimId != 0
                });
            }

            selectItems.Add(new SelectListItem
            {
                Value = "0",
                Text = "Unassigned (link later)",
                Selected = claimId == 0 || !selectItems.Any(si => si.Selected)
            });

            ViewBag.ClaimSelectList = selectItems;
            ViewBag.SelectedClaimID = claimId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(int claimId, IFormFile file)
        {
            try
            {
                var documents = LoadDocuments();

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
                var nextId = documents.Any() ? documents.Max(d => d.DocumentID) + 1 : 1;
                var document = new SupportingDocument
                {
                    DocumentID = nextId,
                    FileName = file.FileName,
                    FilePath = $"/uploads/documents/{fileName}",
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    UploadDate = DateTime.Now,
                    ClaimID = claimId
                };

                documents.Add(document);
                SaveDocuments(documents);

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
            var documents = LoadDocuments()
                .Where(d => d.ClaimID == claimId)
                .OrderByDescending(d => d.UploadDate)
                .Select(d => new
                {
                    documentId = d.DocumentID,
                    fileName = d.FileName,
                    fileSize = d.FileSize,
                    uploadDate = d.UploadDate,
                    filePath = d.FilePath,
                    contentType = d.ContentType
                })
                .ToList();
            return Json(documents);
        }

        public IActionResult Download(int id)
        {
            try
            {
                var documents = LoadDocuments();

                var document = documents.FirstOrDefault(d => d.DocumentID == id);
                if (document == null)
                {
                    TempData["ErrorMessage"] = "Document not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get the physical file path
                var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                
                // Check if file exists
                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] = $"File not found: {document.FileName}";
                    return RedirectToAction(nameof(Index));
                }

                // Read file content
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                
                // Determine content type
                var contentType = document.ContentType ?? "application/octet-stream";
                
                // Return file for download
                return File(fileBytes, contentType, document.FileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error downloading document: {ex.Message}");
                TempData["ErrorMessage"] = $"Error downloading file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var documents = LoadDocuments();

                var document = documents.FirstOrDefault(d => d.DocumentID == id);
                if (document == null)
                {
                    TempData["ErrorMessage"] = "Document not found.";
                    return RedirectToAction(nameof(Index));
                }

                var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/', '\\'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                documents.Remove(document);
                SaveDocuments(documents);

                TempData["SuccessMessage"] = $"Document '{document.FileName}' deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting document: {ex.Message}");
                TempData["ErrorMessage"] = "Error deleting document.";
                return RedirectToAction(nameof(Index));
            }
        }
        private static List<SupportingDocument> LoadDocuments()
        {
            try
            {
                if (!System.IO.File.Exists(DataFilePath))
                {
                    return new List<SupportingDocument>();
                }

                var json = System.IO.File.ReadAllText(DataFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<SupportingDocument>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var documents = JsonSerializer.Deserialize<List<SupportingDocument>>(json, options) ?? new List<SupportingDocument>();
                var validDocuments = documents
                    .Where(d => d != null &&
                                d.DocumentID > 0 &&
                                !string.IsNullOrWhiteSpace(d.FileName) &&
                                !string.IsNullOrWhiteSpace(d.FilePath))
                    .ToList();

                if (validDocuments.Count != documents.Count)
                {
                    SaveDocuments(validDocuments);
                }

                return validDocuments;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading documents from file: {ex.Message}");
                return new List<SupportingDocument>();
            }
        }

        private static void SaveDocuments(List<SupportingDocument> documents)
        {
            try
            {
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

                var json = JsonSerializer.Serialize(documents, options);
                System.IO.File.WriteAllText(DataFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving documents to file: {ex.Message}");
            }
        }
    }
}