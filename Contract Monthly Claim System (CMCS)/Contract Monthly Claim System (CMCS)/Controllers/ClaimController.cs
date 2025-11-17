using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Models;
using System.Text.Json;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class ClaimController : Controller
    {
        private static List<Claim> _claims = new List<Claim>();
        private static int _nextClaimId = 1;
        private static readonly string DataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "claims.json");
        public IActionResult Index()
        {
            // Always load from file first to get the latest persisted data
            if (!_claims.Any())
            {
                LoadClaimsFromFile();
            }

            // Filter out corrupted/empty claims (those with ClaimID = 0, default dates, or zero amounts)
            var validClaims = _claims.Where(c => 
                c.ClaimID > 0 && 
                c.ClaimDate != default(DateTime) && 
                c.ClaimDate.Year > 2000 && // Valid date check
                c.SubmissionDate != default(DateTime) &&
                c.SubmissionDate.Year > 2000 &&
                c.HoursWorked > 0 &&
                c.HourlyRate > 0 &&
                c.TotalAmount > 0
            ).ToList();

            // If we have corrupted data, clean it up
            if (_claims.Count != validClaims.Count)
            {
                System.Diagnostics.Debug.WriteLine($"Filtered out {_claims.Count - validClaims.Count} corrupted claim entries");
                _claims = validClaims;
                if (_claims.Any())
                {
                    _nextClaimId = _claims.Max(c => c.ClaimID) + 1;
                    SaveClaimsToFile();
                }
            }

            // Only initialize with sample data if file doesn't exist or is completely empty
            // This ensures we don't overwrite user-created claims
            if (!_claims.Any())
            {
                // Check if file exists - if it does but is empty, don't add sample data
                bool fileExists = System.IO.File.Exists(DataFilePath);
                if (!fileExists)
                {
                    // File doesn't exist, so initialize with sample data for first-time users
                    _claims.AddRange(GetSampleClaims());
                    _nextClaimId = _claims.Max(c => c.ClaimID) + 1;
                    System.Diagnostics.Debug.WriteLine($"ClaimController.Index - Initialized with {_claims.Count} sample claims (first time)");
                    SaveClaimsToFile();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ClaimController.Index - File exists but no valid claims found.");
                }
            }

            var claims = _claims.OrderByDescending(c => c.SubmissionDate).ToList();
            System.Diagnostics.Debug.WriteLine($"ClaimController.Index - Returning {claims.Count} valid claims to view");
            return View(claims);
        }
        public IActionResult Details(int id)
        {
            // Load data from file if not already loaded
            if (!_claims.Any())
            {
                LoadClaimsFromFile();

                // If still no claims after loading, initialize with sample data
                if (!_claims.Any())
                {
                    _claims.AddRange(GetSampleClaims());
                    _nextClaimId = _claims.Max(c => c.ClaimID) + 1;
                    SaveClaimsToFile();
                }
            }

            var claim = _claims.FirstOrDefault(c => c.ClaimID == id);
            if (claim == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(claim);
        }
        public IActionResult Create()
        {
            var viewModel = new ClaimCreateViewModel
            {
                ClaimDate = DateTime.Now
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClaimCreateViewModel viewModel)
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Create POST called - Hours: {viewModel.HoursWorked}, Rate: {viewModel.HourlyRate}");

            // Always load from file first to ensure we have the latest data and correct next ID
            if (!_claims.Any())
            {
                LoadClaimsFromFile();
            }

            if (ModelState.IsValid)
            {
                // Create a new Claim object from the ViewModel
                var claim = new Claim
                {
                    ClaimID = _nextClaimId++,
                    ClaimDate = viewModel.ClaimDate,
                    HoursWorked = viewModel.HoursWorked,
                    HourlyRate = viewModel.HourlyRate,
                    Notes = viewModel.Notes,
                    SubmissionDate = DateTime.Now,
                    ClaimStatus = "Pending",
                    UserID = 1, // For demo purposes - in real app, get from session/auth
                    TotalAmount = viewModel.HoursWorked * viewModel.HourlyRate
                };

                // Save the claim to our in-memory storage
                _claims.Add(claim);
                System.Diagnostics.Debug.WriteLine($"ClaimController.Create - Added claim #{claim.ClaimID} to _claims. Total claims: {_claims.Count}");

                // Save to file immediately
                SaveClaimsToFile();
                System.Diagnostics.Debug.WriteLine($"Claim #{claim.ClaimID} saved to file: {DataFilePath}");

                TempData["SuccessMessage"] = $"Claim #{claim.ClaimID} submitted successfully! Total Amount: R {claim.TotalAmount:N2}";
                TempData["NewClaimId"] = claim.ClaimID; // Pass the new claim ID for file upload
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
            }

            return View(viewModel);
        }
        public IActionResult Edit(int id)
        {
            // Load data from file if not already loaded
            if (!_claims.Any())
            {
                LoadClaimsFromFile();

                // If still no claims after loading, initialize with sample data
                if (!_claims.Any())
                {
                    _claims.AddRange(GetSampleClaims());
                    _nextClaimId = _claims.Max(c => c.ClaimID) + 1;
                    SaveClaimsToFile();
                }
            }

            var claim = _claims.FirstOrDefault(c => c.ClaimID == id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = $"Claim #{id} not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Claim claim)
        {
            try
            {
                // Load data from file if not already loaded
                if (!_claims.Any())
                {
                    LoadClaimsFromFile();
                }

                var existingClaim = _claims.FirstOrDefault(c => c.ClaimID == id);
                if (existingClaim == null)
                {
                    TempData["ErrorMessage"] = $"Claim #{id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate input
                if (claim.HoursWorked < 0.1m || claim.HoursWorked > 168m)
                {
                    ModelState.AddModelError("HoursWorked", "Hours worked must be between 0.1 and 168");
                    return View(existingClaim);
                }

                if (claim.HourlyRate <= 0)
                {
                    ModelState.AddModelError("HourlyRate", "Hourly rate must be greater than 0");
                    return View(existingClaim);
                }

                // Update claim properties
                existingClaim.ClaimDate = claim.ClaimDate;
                existingClaim.HoursWorked = claim.HoursWorked;
                existingClaim.HourlyRate = claim.HourlyRate;
                existingClaim.TotalAmount = claim.HoursWorked * claim.HourlyRate; // Recalculate total
                existingClaim.ClaimStatus = claim.ClaimStatus ?? existingClaim.ClaimStatus;
                existingClaim.Notes = claim.Notes;
                existingClaim.LastUpdated = DateTime.Now;

                // Save to file
                SaveClaimsToFile();

                TempData["SuccessMessage"] = $"Claim #{id} has been updated successfully!";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating claim: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while updating the claim.";
                
                // Reload the claim for the view
                if (!_claims.Any())
                {
                    LoadClaimsFromFile();
                }
                var existingClaim = _claims.FirstOrDefault(c => c.ClaimID == id);
                return View(existingClaim ?? new Claim { ClaimID = id });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id, string comments = null)
        {
            try
            {
                // Load data from file if not already loaded
                if (!_claims.Any())
                {
                    LoadClaimsFromFile();
                }

                var claim = _claims.FirstOrDefault(c => c.ClaimID == id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = $"Claim #{id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update claim status
                claim.ClaimStatus = "Approved";
                claim.LastUpdated = DateTime.Now;
                claim.StatusNotes = comments ?? "Approved by coordinator/manager";

                // Save claim to file
                SaveClaimsToFile();

                // Create approval record using ApprovalController
                ApprovalController.CreateApprovalRecord(id, comments ?? "Approved by coordinator/manager", 1);

                TempData["SuccessMessage"] = $"Claim #{id} has been approved successfully!";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error approving claim: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while approving the claim.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }
        private static List<Claim> GetSampleClaims()
        {
            return new List<Claim>
            {
                new Claim { ClaimID = 1, ClaimDate = DateTime.Now.AddDays(-7), ClaimStatus = "Pending", HoursWorked = 40, HourlyRate = 25.00m, TotalAmount = 1000.00m, SubmissionDate = DateTime.Now.AddDays(-5), UserID = 1 },
                new Claim { ClaimID = 2, ClaimDate = DateTime.Now.AddDays(-14), ClaimStatus = "Approved", HoursWorked = 35, HourlyRate = 30.00m, TotalAmount = 1050.00m, SubmissionDate = DateTime.Now.AddDays(-12), UserID = 2 },
                new Claim { ClaimID = 3, ClaimDate = DateTime.Now.AddDays(-21), ClaimStatus = "Rejected", HoursWorked = 20, HourlyRate = 28.00m, TotalAmount = 560.00m, SubmissionDate = DateTime.Now.AddDays(-18), UserID = 3 },
                new Claim { ClaimID = 4, ClaimDate = DateTime.Now.AddDays(-3), ClaimStatus = "Pending", HoursWorked = 45, HourlyRate = 32.00m, TotalAmount = 1440.00m, SubmissionDate = DateTime.Now.AddDays(-1), UserID = 1 }
            };
        }

        // Helper method to get claim count for debugging
        public int GetClaimCount()
        {
            return _claims.Count;
        }

        // Static method to get all claims for use by other controllers
        public static List<Claim> GetAllClaims()
        {
            // Ensure data is loaded
            if (!_claims.Any())
            {
                LoadClaimsFromFile();
            }

            // Filter out corrupted claims
            var validClaims = _claims.Where(c => 
                c.ClaimID > 0 && 
                c.ClaimDate.Year > 2000 && 
                c.SubmissionDate.Year > 2000 &&
                c.HoursWorked > 0 &&
                c.HourlyRate > 0 &&
                c.TotalAmount > 0
            ).ToList();

            // Clean up if needed
            if (_claims.Count != validClaims.Count)
            {
                _claims = validClaims;
                if (_claims.Any())
                {
                    _nextClaimId = _claims.Max(c => c.ClaimID) + 1;
                }
            }

            // Only add sample data if file doesn't exist (first time)
            if (!_claims.Any() && !System.IO.File.Exists(DataFilePath))
            {
                _claims.AddRange(GetSampleClaims());
                _nextClaimId = _claims.Max(c => c.ClaimID) + 1;
                SaveClaimsToFile();
            }

            System.Diagnostics.Debug.WriteLine($"ClaimController.GetAllClaims - Returning {_claims.Count} valid claims");
            return _claims;
        }

        // Static method to update a claim
        public static void UpdateClaim(Claim updatedClaim)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.ClaimID == updatedClaim.ClaimID);
            if (existingClaim != null)
            {
                existingClaim.ClaimStatus = updatedClaim.ClaimStatus;
                existingClaim.Notes = updatedClaim.Notes;
                // Update other fields as needed
            }
        }

        // Static method to add a claim
        public static void AddClaim(Claim claim)
        {
            _claims.Add(claim);
        }

        // Method to reset static data for testing
        public static void ResetStaticData()
        {
            _claims.Clear();
            _nextClaimId = 1;
        }

        // File persistence methods
        public static void SaveClaimsToFile()
        {
            try
            {
                // Ensure the Data directory exists
                var dataDir = Path.GetDirectoryName(DataFilePath);
                if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                    System.Diagnostics.Debug.WriteLine($"Created Data directory: {dataDir}");
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_claims, options);
                System.IO.File.WriteAllText(DataFilePath, json);
                System.Diagnostics.Debug.WriteLine($"Successfully saved {_claims.Count} claims to file: {DataFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving claims to file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to ensure caller knows save failed
            }
        }

        private static void LoadClaimsFromFile()
        {
            try
            {
                if (System.IO.File.Exists(DataFilePath))
                {
                    var json = System.IO.File.ReadAllText(DataFilePath);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        var claims = JsonSerializer.Deserialize<List<Claim>>(json, options);
                        if (claims != null && claims.Any())
                        {
                            _claims = claims;
                            _nextClaimId = _claims.Max(c => c.ClaimID) + 1;
                            System.Diagnostics.Debug.WriteLine($"Loaded {_claims.Count} claims from file: {DataFilePath}. Next Claim ID will be: {_nextClaimId}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"File exists but contains no valid claims: {DataFilePath}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"File exists but is empty: {DataFilePath}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Claims file does not exist: {DataFilePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading claims from file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}