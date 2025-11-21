using Contract_Monthly_Claim_System__CMCS_.Models;
using Contract_Monthly_Claim_System__CMCS_.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class LecturerController : Controller
    {
        private readonly CmcsDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private const int LecturerRoleID = 1; // RoleID for Lecturer
        private const int MaxHoursPerMonth = 180;
        private const decimal DefaultLecturerHourlyRate = 450m;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx", ".doc", ".xls", ".jpg", ".jpeg", ".png" };
        private static readonly string DocumentsDataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "documents.json");

        public LecturerController(CmcsDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Lecturer/Login
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            if (HttpContext.Session.GetInt32("LecturerID") != null)
            {
                return RedirectToAction(nameof(Dashboard));
            }
            return View();
        }

        // POST: Lecturer/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Please enter both email and password.";
                return View();
            }

            try
            {
                // Find user by email with Lecturer role
                var user = await FindLecturerByEmailAsync(email);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Invalid email or password. Please ensure you are logging in as a Lecturer.";
                    return View();
                }

                // Simple password verification (for demo - in production, use proper hashing)
                // Check if password hash matches or if it's a default/empty hash (for initial setup)
                bool passwordValid = false;
                if (string.IsNullOrEmpty(user.PasswordHash) || user.PasswordHash == "default_password_hash")
                {
                    // For initial setup, accept any password if hash is default
                    // In production, you'd want to set a proper password first
                    passwordValid = true;
                }
                else
                {
                    // Verify password hash (simplified for demo)
                    passwordValid = VerifyPassword(password, user.PasswordHash);
                }

                if (!passwordValid)
                {
                    TempData["ErrorMessage"] = "Invalid email or password.";
                    return View();
                }

                // Set session variables
                HttpContext.Session.SetInt32("LecturerID", user.UserID);
                HttpContext.Session.SetString("LecturerName", $"{user.FirstName} {user.LastName}");
                HttpContext.Session.SetString("LecturerEmail", user.Email);

                return RedirectToAction(nameof(Dashboard));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
                return View();
            }
        }

        // GET: Lecturer/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");
            if (lecturerId == null)
            {
                TempData["ErrorMessage"] = "Please log in to access the dashboard.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var lecturer = await FindLecturerByIdAsync(lecturerId.Value);

                if (lecturer == null)
                {
                    HttpContext.Session.Clear();
                    TempData["ErrorMessage"] = "Lecturer account not found.";
                    return RedirectToAction(nameof(Login));
                }

                // Get all claims for this lecturer
                var claims = ClaimController.GetAllClaims()
                    .Where(c => c.UserID == lecturerId.Value)
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToList();

                // Calculate statistics
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var monthlyHours = claims
                    .Where(c => c.ClaimDate.Month == currentMonth && c.ClaimDate.Year == currentYear)
                    .Sum(c => c.HoursWorked);

                ViewBag.Lecturer = lecturer;
                ViewBag.Claims = claims;
                ViewBag.MonthlyHours = monthlyHours;
                ViewBag.MaxHoursPerMonth = MaxHoursPerMonth;
                ViewBag.RemainingHours = Math.Max(0, MaxHoursPerMonth - monthlyHours);

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard error: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return RedirectToAction(nameof(Login));
            }
        }

        // GET: Lecturer/SubmitClaim
        public async Task<IActionResult> SubmitClaim()
        {
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");
            if (lecturerId == null)
            {
                TempData["ErrorMessage"] = "Please log in to submit a claim.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var lecturer = await FindLecturerByIdAsync(lecturerId.Value);

                if (lecturer == null)
                {
                    HttpContext.Session.Clear();
                    TempData["ErrorMessage"] = "Lecturer account not found.";
                    return RedirectToAction(nameof(Login));
                }

                // Check monthly hours limit
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var claims = ClaimController.GetAllClaims()
                    .Where(c => c.UserID == lecturerId.Value && 
                                c.ClaimDate.Month == currentMonth && 
                                c.ClaimDate.Year == currentYear)
                    .ToList();
                var monthlyHours = claims.Sum(c => c.HoursWorked);
                var remainingHours = MaxHoursPerMonth - monthlyHours;

                if (remainingHours <= 0)
                {
                    TempData["ErrorMessage"] = $"You have reached the maximum limit of {MaxHoursPerMonth} hours for this month. Cannot submit more claims.";
                    return RedirectToAction(nameof(Dashboard));
                }

                var viewModel = new ClaimCreateViewModel
                {
                    ClaimDate = DateTime.Now,
                    UserID = lecturer.UserID,
                    HourlyRate = lecturer.HourlyRate
                };

                ViewBag.Lecturer = lecturer;
                ViewBag.MonthlyHours = monthlyHours;
                ViewBag.RemainingHours = remainingHours;
                ViewBag.MaxHoursPerMonth = MaxHoursPerMonth;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SubmitClaim GET error: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the claim form.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // POST: Lecturer/SubmitClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(ClaimCreateViewModel viewModel, List<IFormFile> supportingDocuments)
        {
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");
            if (lecturerId == null)
            {
                TempData["ErrorMessage"] = "Please log in to submit a claim.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var lecturer = await FindLecturerByIdAsync(lecturerId.Value);

                if (lecturer == null)
                {
                    HttpContext.Session.Clear();
                    TempData["ErrorMessage"] = "Lecturer account not found.";
                    return RedirectToAction(nameof(Login));
                }

                // Validate monthly hours limit
                var currentMonth = viewModel.ClaimDate.Month;
                var currentYear = viewModel.ClaimDate.Year;
                var claims = ClaimController.GetAllClaims()
                    .Where(c => c.UserID == lecturerId.Value && 
                                c.ClaimDate.Month == currentMonth && 
                                c.ClaimDate.Year == currentYear)
                    .ToList();
                var monthlyHours = claims.Sum(c => c.HoursWorked);
                var totalHoursAfterSubmission = monthlyHours + viewModel.HoursWorked;

                if (totalHoursAfterSubmission > MaxHoursPerMonth)
                {
                    var remainingHours = MaxHoursPerMonth - monthlyHours;
                    ModelState.AddModelError("HoursWorked", 
                        $"This submission would exceed the monthly limit of {MaxHoursPerMonth} hours. You have {remainingHours:F1} hours remaining this month.");
                }

                // Validate uploaded files
                if (supportingDocuments != null && supportingDocuments.Any())
                {
                    foreach (var file in supportingDocuments)
                    {
                        if (file.Length > MaxFileSize)
                        {
                            ModelState.AddModelError("", $"File '{file.FileName}' exceeds the maximum size of 10MB.");
                        }

                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        if (!AllowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("", $"File '{file.FileName}' has an invalid file type. Allowed types: {string.Join(", ", AllowedExtensions)}");
                        }
                    }
                }

                // Ensure UserID matches logged-in lecturer
                viewModel.UserID = lecturer.UserID;
                viewModel.HourlyRate = lecturer.HourlyRate; // Always use lecturer's hourly rate from HR

                if (ModelState.IsValid)
                {
                    // Create the claim using ClaimController's Create method logic
                    // We'll add it directly to the static list and save
                    var allClaims = ClaimController.GetAllClaims();
                    var nextClaimId = allClaims.Any() ? allClaims.Max(c => c.ClaimID) + 1 : 1;

                    var claim = new Claim
                    {
                        ClaimID = nextClaimId,
                        ClaimDate = viewModel.ClaimDate,
                        HoursWorked = viewModel.HoursWorked,
                        HourlyRate = lecturer.HourlyRate, // Always use lecturer's hourly rate from HR
                        Notes = viewModel.Notes,
                        SubmissionDate = DateTime.Now,
                        ClaimStatus = "Pending",
                        UserID = lecturer.UserID,
                        TotalAmount = viewModel.HoursWorked * lecturer.HourlyRate
                    };

                    // Use ClaimController's static method to add and save
                    ClaimController.AddClaim(claim);
                    ClaimController.SaveClaimsToFile();

                    System.Diagnostics.Debug.WriteLine($"LecturerController: Claim #{claim.ClaimID} created for UserID {lecturer.UserID}");

                    // Handle file uploads if any
                    if (supportingDocuments != null && supportingDocuments.Any(f => f != null && f.Length > 0))
                    {
                        var uploadedCount = await SaveSupportingDocumentsAsync(supportingDocuments, claim.ClaimID);
                        TempData["SuccessMessage"] = $"Claim #{claim.ClaimID} submitted successfully with {uploadedCount} document(s)! Total Amount: R {claim.TotalAmount:N2}";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"Claim #{claim.ClaimID} submitted successfully! Total Amount: R {claim.TotalAmount:N2}";
                    }

                    return RedirectToAction(nameof(MyClaims));
                }
                else
                {
                    ViewBag.Lecturer = lecturer;
                    ViewBag.MonthlyHours = monthlyHours;
                    ViewBag.RemainingHours = MaxHoursPerMonth - monthlyHours;
                    ViewBag.MaxHoursPerMonth = MaxHoursPerMonth;
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SubmitClaim POST error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"An error occurred while submitting the claim: {ex.Message}";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // GET: Lecturer/MyClaims
        public IActionResult MyClaims()
        {
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");
            if (lecturerId == null)
            {
                TempData["ErrorMessage"] = "Please log in to view your claims.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var claims = ClaimController.GetAllClaims()
                    .Where(c => c.UserID == lecturerId.Value)
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToList();

                ViewBag.Claims = claims;
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MyClaims error: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your claims.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // GET: Lecturer/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Login));
        }

        // Helper methods to retrieve lecturers with graceful fallback to JSON storage
        private async Task<User?> FindLecturerByEmailAsync(string email)
        {
            try
            {
                if (_context != null)
                {
                    var lecturer = await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.RoleID == LecturerRoleID);
                    return NormalizeLecturerHourlyRate(lecturer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FindLecturerByEmailAsync DB error: {ex.Message}. Falling back to JSON data.");
            }

            var users = UserController.GetAllUsers();
            return NormalizeLecturerHourlyRate(users?.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.RoleID == LecturerRoleID));
        }

        private async Task<User?> FindLecturerByIdAsync(int userId)
        {
            try
            {
                if (_context != null)
                {
                    var lecturer = await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.UserID == userId && u.RoleID == LecturerRoleID);
                    return NormalizeLecturerHourlyRate(lecturer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FindLecturerByIdAsync DB error: {ex.Message}. Falling back to JSON data.");
            }

            var users = UserController.GetAllUsers();
            return NormalizeLecturerHourlyRate(users?.FirstOrDefault(u => u.UserID == userId && u.RoleID == LecturerRoleID));
        }

        private User? NormalizeLecturerHourlyRate(User? lecturer)
        {
            if (lecturer == null)
            {
                return null;
            }

            if (lecturer.HourlyRate <= 0)
            {
                var ensuredRate = UserController.EnsureUserHourlyRate(lecturer.UserID, DefaultLecturerHourlyRate);
                lecturer.HourlyRate = ensuredRate;
            }

            return lecturer;
        }

        // Helper method to verify password (simplified for demo)
        private bool VerifyPassword(string password, string hash)
        {
            // Simplified password verification for demo
            // In production, use proper password hashing (BCrypt, PBKDF2, etc.)
            if (string.IsNullOrEmpty(hash))
                return false;

            // For demo purposes, simple comparison
            // In production, implement proper hash verification
            return hash == HashPassword(password);
        }

        // Helper method to hash password (simplified for demo)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Helper method to save supporting documents
        private async Task<int> SaveSupportingDocumentsAsync(List<IFormFile> files, int claimId)
        {
            int uploadedCount = 0;
            var documents = LoadDocuments();

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                foreach (var file in files.Where(f => f != null && f.Length > 0))
                {
                    // Validate file size
                    if (file.Length > MaxFileSize)
                    {
                        System.Diagnostics.Debug.WriteLine($"File {file.FileName} exceeds maximum size");
                        continue;
                    }

                    // Validate file extension
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!AllowedExtensions.Contains(fileExtension))
                    {
                        System.Diagnostics.Debug.WriteLine($"File {file.FileName} has invalid extension");
                        continue;
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
                    uploadedCount++;
                }

                // Save all documents to file
                if (uploadedCount > 0)
                {
                    SaveDocuments(documents);
                    System.Diagnostics.Debug.WriteLine($"Saved {uploadedCount} documents for claim {claimId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving documents: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return uploadedCount;
        }

        private static List<SupportingDocument> LoadDocuments()
        {
            try
            {
                if (!System.IO.File.Exists(DocumentsDataFilePath))
                {
                    return new List<SupportingDocument>();
                }

                var json = System.IO.File.ReadAllText(DocumentsDataFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<SupportingDocument>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var documents = JsonSerializer.Deserialize<List<SupportingDocument>>(json, options) ?? new List<SupportingDocument>();
                return documents.Where(d => d != null && d.DocumentID > 0 && !string.IsNullOrWhiteSpace(d.FileName)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading documents: {ex.Message}");
                return new List<SupportingDocument>();
            }
        }

        private static void SaveDocuments(List<SupportingDocument> documents)
        {
            try
            {
                var dataDir = Path.GetDirectoryName(DocumentsDataFilePath);
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
                System.IO.File.WriteAllText(DocumentsDataFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving documents: {ex.Message}");
            }
        }
    }
}

