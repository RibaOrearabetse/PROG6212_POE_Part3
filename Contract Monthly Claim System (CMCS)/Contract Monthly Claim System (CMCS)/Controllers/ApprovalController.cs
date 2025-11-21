using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Models;
using System.Text.Json;
using System.Linq;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class ApprovalController : Controller
    {
        private static List<Approval> _approvals = new List<Approval>();
        private static int _nextApprovalId = 1;
        private static List<Claim> _claims = new List<Claim>();
        private static readonly string ApprovalsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "approvals.json");
        private static readonly string ClaimsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "claims.json");
        public IActionResult Index()
        {
            // Always load from file first to get the latest persisted data
            if (!_approvals.Any())
            {
                LoadApprovalsFromFile();
            }

            // Filter out corrupted/empty approvals
            var validApprovals = _approvals.Where(a =>
                a.ApprovalID > 0 &&
                a.ClaimID > 0 &&
                a.ApprovalDate != default(DateTime) &&
                a.ApprovalDate.Year > 2000
            ).ToList();

            // If we have corrupted data, clean it up
            if (_approvals.Count != validApprovals.Count)
            {
                System.Diagnostics.Debug.WriteLine($"Filtered out {_approvals.Count - validApprovals.Count} corrupted approval entries");
                _approvals = validApprovals;
                if (_approvals.Any())
                {
                    _nextApprovalId = _approvals.Max(a => a.ApprovalID) + 1;
                    SaveApprovalsToFile();
                }
            }

            // Only initialize with sample data if file doesn't exist (first time)
            if (!_approvals.Any())
            {
                bool fileExists = System.IO.File.Exists(ApprovalsFilePath);
                if (!fileExists)
                {
                    _approvals.AddRange(GetSampleApprovals());
                    _nextApprovalId = _approvals.Max(a => a.ApprovalID) + 1;
                    System.Diagnostics.Debug.WriteLine($"ApprovalController.Index - Initialized with {_approvals.Count} sample approvals (first time)");
                    SaveApprovalsToFile();
                }
            }

            // Always get fresh claims data to ensure we have the latest statuses
            var allClaims = ClaimController.GetAllClaims();
            if (allClaims == null || !allClaims.Any())
            {
                LoadClaimsFromFile();
                allClaims = ClaimController.GetAllClaims();
            }

            // Force reload to get absolute latest data
            ClaimController.GetAllClaims(); // This ensures data is loaded
            allClaims = ClaimController.GetAllClaims();

            // Filter to only show approvals for claims that are "Approved" (case-insensitive)
            var approvedClaims = allClaims
                .Where(c => c != null &&
                           !string.IsNullOrEmpty(c.ClaimStatus) &&
                           c.ClaimStatus.Trim().Equals("Approved", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.ClaimID)
                .ToHashSet();

            // Remove any approvals that no longer belong to approved claims
            var removedCount = _approvals.RemoveAll(a => !approvedClaims.Contains(a.ClaimID));
            if (removedCount > 0)
            {
                System.Diagnostics.Debug.WriteLine($"ApprovalController.Index - Removed {removedCount} approvals for non-approved claims");
                if (_approvals.Any())
                {
                    _nextApprovalId = _approvals.Max(a => a.ApprovalID) + 1;
                }
                SaveApprovalsToFile();
            }

            System.Diagnostics.Debug.WriteLine($"ApprovalController.Index - Total claims loaded: {allClaims?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"ApprovalController.Index - Approved claims found: {approvedClaims.Count}");
            foreach (var claimId in approvedClaims)
            {
                var claim = allClaims.FirstOrDefault(c => c.ClaimID == claimId);
                System.Diagnostics.Debug.WriteLine($"  - Claim ID {claimId}: Status = '{claim?.ClaimStatus}'");
            }

            var approvals = _approvals
                .Where(a => a != null && approvedClaims.Contains(a.ClaimID)) // Only approvals for approved claims
                .OrderByDescending(a => a.ApprovalDate)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"ApprovalController.Index - Total approvals: {_approvals.Count}");
            System.Diagnostics.Debug.WriteLine($"ApprovalController.Index - Returning {approvals.Count} approvals for approved claims");
            foreach (var approval in approvals)
            {
                var claim = allClaims.FirstOrDefault(c => c.ClaimID == approval.ClaimID);
                System.Diagnostics.Debug.WriteLine($"  - Approval ID {approval.ApprovalID} for Claim ID {approval.ClaimID}: Claim Status = '{claim?.ClaimStatus}'");
            }

            return View(approvals);
        }

        // Static method to create an approval record (can be called from other controllers)
        public static void CreateApprovalRecord(int claimId, string comments, int approverId = 1)
        {
            try
            {
                // Load approvals if not already loaded
                if (!_approvals.Any())
                {
                    LoadApprovalsFromFile();
                }

                // Ensure claim is actually approved before creating record
                var claim = ClaimController.GetAllClaims().FirstOrDefault(c => c.ClaimID == claimId);
                if (claim == null || !string.Equals(claim.ClaimStatus, "Approved", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"CreateApprovalRecord skipped: Claim {claimId} is not approved.");
                    return;
                }

                // Check if approval already exists for this claim
                var existingApproval = _approvals.FirstOrDefault(a => a.ClaimID == claimId);
                if (existingApproval != null)
                {
                    // Update existing approval
                    existingApproval.ApprovalDate = DateTime.Now;
                    existingApproval.Comments = comments;
                    existingApproval.ApproverID = approverId;
                    System.Diagnostics.Debug.WriteLine($"Updated existing approval for claim {claimId}");
                }
                else
                {
                    // Create new approval record
                    var approval = new Approval
                    {
                        ApprovalID = _nextApprovalId++,
                        ClaimID = claimId,
                        ApprovalDate = DateTime.Now,
                        Comments = comments,
                        ApproverID = approverId
                    };
                    _approvals.Add(approval);
                    System.Diagnostics.Debug.WriteLine($"Created new approval record for claim {claimId}");
                }

                // Save to file immediately
                SaveApprovalsToFile();
                System.Diagnostics.Debug.WriteLine($"Approval record saved to file: {ApprovalsFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating approval record: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public IActionResult PendingClaims()
        {
            try
            {
                // Get all claims regardless of status for review
                var allClaims = GetPendingClaims();
                System.Diagnostics.Debug.WriteLine($"PendingClaims action: Returning {allClaims.Count} total claims (all statuses)");
                return View("PendingClaims", allClaims);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PendingClaims: {ex.Message}");
                return View("PendingClaims", new List<Claim>());
            }
        }
        public IActionResult Details(int id)
        {
            // Always load from file first to get the latest persisted data
            if (!_approvals.Any())
            {
                LoadApprovalsFromFile();
            }

            var approval = _approvals.FirstOrDefault(a => a.ApprovalID == id);
            if (approval == null)
            {
                TempData["ErrorMessage"] = "Approval not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(approval);
        }
        public IActionResult ProcessApproval(int claimId)
        {
            var claim = GetClaimById(claimId);
            if (claim == null)
            {
                return RedirectToAction(nameof(PendingClaims));
            }
            return View(claim);
        }

        // GET: Approval/Edit/5
        public IActionResult Edit(int id)
        {
            // Always load from file first to get the latest persisted data
            if (!_approvals.Any())
            {
                LoadApprovalsFromFile();
            }

            var approval = _approvals.FirstOrDefault(a => a.ApprovalID == id);
            if (approval == null)
            {
                TempData["ErrorMessage"] = "Approval not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(approval);
        }

        // POST: Approval/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string comments)
        {
            try
            {
                // Load current data
                LoadApprovalsFromFile();

                var approval = _approvals.FirstOrDefault(a => a.ApprovalID == id);
                if (approval == null)
                {
                    TempData["ErrorMessage"] = "Approval not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update the approval
                approval.Comments = comments ?? approval.Comments;

                // Save to file
                SaveApprovalsToFile();

                TempData["SuccessMessage"] = $"Approval #{id} updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
        }

        // Test method to create sample claims for testing
        public IActionResult CreateTestClaims()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("CreateTestClaims: Starting...");

                // Clear existing claims and force recreation
                _claims.Clear();
                _claims.AddRange(GetSampleClaims());
                SaveClaimsToFile();

                System.Diagnostics.Debug.WriteLine($"CreateTestClaims: Created {_claims.Count} test claims");
                TempData["SuccessMessage"] = $"Test claims created successfully! Added {_claims.Count} sample claims.";

                return RedirectToAction(nameof(PendingClaims));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateTestClaims error: {ex.Message}");
                TempData["ErrorMessage"] = $"Error creating test claims: {ex.Message}";
                return RedirectToAction(nameof(PendingClaims));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveClaim(int claimId, string comments)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ApproveClaim called with claimId: {claimId}, comments: {comments}");

                // Always get fresh data from ClaimController to ensure we have the latest claims
                var allClaims = ClaimController.GetAllClaims();
                if (allClaims == null || !allClaims.Any())
                {
                    LoadClaimsFromFile();
                    allClaims = ClaimController.GetAllClaims();
                }
                LoadApprovalsFromFile();

                // Get the claim from ClaimController's list to ensure we're updating the right instance
                var claim = allClaims?.FirstOrDefault(c => c.ClaimID == claimId);

                if (claim != null)
                {
                    // Update claim status in the shared claim storage
                    claim.ClaimStatus = "Approved";
                    claim.LastUpdated = DateTime.Now;
                    claim.StatusNotes = comments ?? "Approved by coordinator/manager";

                    // Save claim using ClaimController to persist the changes
                    ClaimController.SaveClaimsToFile();

                    // Create approval record using the static method
                    CreateApprovalRecord(claimId, comments ?? "Approved by coordinator/manager", 1);

                    System.Diagnostics.Debug.WriteLine($"Claim {claimId} approved successfully and approval record created");
                    return Json(new { success = true, message = $"Claim #{claimId} has been approved successfully!" });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Claim {claimId} not found");
                    return Json(new { success = false, message = $"Claim #{claimId} not found." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ApproveClaim: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while approving the claim." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectClaim(int claimId, string comments)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"RejectClaim called with claimId: {claimId}, comments: {comments}");

                // Always get fresh data from ClaimController to ensure we have the latest claims
                var allClaims = ClaimController.GetAllClaims();
                if (allClaims == null || !allClaims.Any())
                {
                    LoadClaimsFromFile();
                    allClaims = ClaimController.GetAllClaims();
                }
                LoadApprovalsFromFile();

                // Get the claim from ClaimController's list to ensure we're updating the right instance
                var claim = allClaims?.FirstOrDefault(c => c.ClaimID == claimId);

                if (claim != null)
                {
                    // Update claim status in the shared claim storage
                    claim.ClaimStatus = "Rejected";
                    claim.LastUpdated = DateTime.Now;
                    claim.StatusNotes = comments ?? "Rejected by coordinator/manager";

                    // Save claim using ClaimController to persist the changes
                    ClaimController.SaveClaimsToFile();

                    // Create approval record using the static method
                    CreateApprovalRecord(claimId, comments ?? "Rejected by coordinator/manager", 1);

                    System.Diagnostics.Debug.WriteLine($"Claim {claimId} rejected successfully and approval record created");
                    return Json(new { success = true, message = $"Claim #{claimId} has been rejected." });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Claim {claimId} not found");
                    return Json(new { success = false, message = $"Claim #{claimId} not found." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RejectClaim: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while rejecting the claim." });
            }
        }
        private List<Claim> GetPendingClaims()
        {
            // Get all actual claims from the shared claim storage (regardless of status)
            var allClaims = GetSharedClaims();

            // Filter out only corrupted/empty claims (NOT by status - we want all statuses)
            var validClaims = allClaims.Where(c =>
                c.ClaimID > 0 &&
                c.ClaimDate.Year > 2000 &&
                c.HoursWorked > 0 &&
                c.HourlyRate > 0
            ).OrderByDescending(c => c.SubmissionDate).ToList();

            System.Diagnostics.Debug.WriteLine($"GetPendingClaims: Found {validClaims.Count} valid claims (all statuses) out of {allClaims.Count} total claims");
            return validClaims;
        }

        private Claim GetClaimById(int claimId)
        {
            // Always get fresh data from ClaimController to ensure we have the latest claims
            var allClaims = ClaimController.GetAllClaims();
            if (allClaims == null || !allClaims.Any())
            {
                // Fallback to local method if ClaimController returns empty
                allClaims = GetSharedClaims();
            }
            return allClaims?.FirstOrDefault(c => c.ClaimID == claimId);
        }

        private List<Claim> GetSharedClaims()
        {
            // Always load fresh data from ClaimController to ensure we get all created claims
            // This ensures synchronization between ClaimController and ApprovalController
            var allClaims = ClaimController.GetAllClaims();

            if (allClaims != null && allClaims.Any())
            {
                System.Diagnostics.Debug.WriteLine($"GetSharedClaims: Loaded {allClaims.Count} claims from ClaimController");
                return allClaims;
            }

            // Fallback: if ClaimController returns empty, try loading directly from file
            LoadClaimsFromFile();
            if (_claims.Any())
            {
                System.Diagnostics.Debug.WriteLine($"GetSharedClaims: Loaded {_claims.Count} claims from file directly");
                return _claims;
            }

            // Only initialize with sample data if file doesn't exist (first time)
            bool fileExists = System.IO.File.Exists(ClaimsFilePath);
            if (!fileExists)
            {
                System.Diagnostics.Debug.WriteLine("GetSharedClaims: No claims file exists, initializing with sample data...");
                _claims.AddRange(GetSampleClaims());
                SaveClaimsToFile();
                System.Diagnostics.Debug.WriteLine($"GetSharedClaims: Initialized with {_claims.Count} sample claims");
            }

            return _claims;
        }

        private List<Claim> GetSampleClaims()
        {
            return new List<Claim>
            {
                new Claim { ClaimID = 1, ClaimDate = DateTime.Now.AddDays(-7), ClaimStatus = "Pending", HoursWorked = 40, HourlyRate = 25.00m, TotalAmount = 1000.00m, SubmissionDate = DateTime.Now.AddDays(-5), UserID = 1, Notes = "Sample claim 1" },
                new Claim { ClaimID = 2, ClaimDate = DateTime.Now.AddDays(-14), ClaimStatus = "Approved", HoursWorked = 35, HourlyRate = 30.00m, TotalAmount = 1050.00m, SubmissionDate = DateTime.Now.AddDays(-12), UserID = 2, Notes = "Sample claim 2" },
                new Claim { ClaimID = 3, ClaimDate = DateTime.Now.AddDays(-21), ClaimStatus = "Rejected", HoursWorked = 20, HourlyRate = 28.00m, TotalAmount = 560.00m, SubmissionDate = DateTime.Now.AddDays(-18), UserID = 3, Notes = "Sample claim 3" },
                new Claim { ClaimID = 4, ClaimDate = DateTime.Now.AddDays(-3), ClaimStatus = "Pending", HoursWorked = 45, HourlyRate = 32.00m, TotalAmount = 1440.00m, SubmissionDate = DateTime.Now.AddDays(-1), UserID = 1, Notes = "Sample claim 4" }
            };
        }

        private List<Approval> GetSharedApprovals()
        {
            // Initialize with sample data if no approvals exist
            if (!_approvals.Any())
            {
                _approvals.AddRange(GetSampleApprovals());
                _nextApprovalId = _approvals.Max(a => a.ApprovalID) + 1;
                System.Diagnostics.Debug.WriteLine($"ApprovalController - Initialized with {_approvals.Count} sample approvals");
            }

            System.Diagnostics.Debug.WriteLine($"ApprovalController.GetSharedApprovals - Returning {_approvals.Count} approvals");
            return _approvals;
        }

        private static List<Approval> GetSampleApprovals()
        {
            return new List<Approval>
            {
                new Approval { ApprovalID = 1, ApprovalDate = DateTime.Now.AddDays(-3), Comments = "Approved - documentation complete", ClaimID = 2, ApproverID = 4 },
                new Approval { ApprovalID = 2, ApprovalDate = DateTime.Now.AddDays(-10), Comments = "Rejected - insufficient documentation", ClaimID = 3, ApproverID = 3 },
                new Approval { ApprovalID = 3, ApprovalDate = DateTime.Now.AddDays(-1), Comments = "Approved with conditions", ClaimID = 1, ApproverID = 4 }
            };
        }

        private static void LoadApprovalsFromFile()
        {
            try
            {
                if (System.IO.File.Exists(ApprovalsFilePath))
                {
                    var json = System.IO.File.ReadAllText(ApprovalsFilePath);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        _approvals = JsonSerializer.Deserialize<List<Approval>>(json, options) ?? new List<Approval>();
                        if (_approvals.Any())
                        {
                            _nextApprovalId = _approvals.Max(a => a.ApprovalID) + 1;
                            System.Diagnostics.Debug.WriteLine($"Loaded {_approvals.Count} approvals from file: {ApprovalsFilePath}. Next Approval ID will be: {_nextApprovalId}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"File exists but contains no valid approvals: {ApprovalsFilePath}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"File exists but is empty: {ApprovalsFilePath}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Approvals file does not exist: {ApprovalsFilePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading approvals from file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                _approvals = new List<Approval>();
            }
        }

        private static void SaveApprovalsToFile()
        {
            try
            {
                var directory = Path.GetDirectoryName(ApprovalsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"Created Data directory: {directory}");
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_approvals, options);
                System.IO.File.WriteAllText(ApprovalsFilePath, json);
                System.Diagnostics.Debug.WriteLine($"Successfully saved {_approvals.Count} approvals to file: {ApprovalsFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving approvals to file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to ensure caller knows save failed
            }
        }

        private void LoadClaimsFromFile()
        {
            try
            {
                if (System.IO.File.Exists(ClaimsFilePath))
                {
                    var json = System.IO.File.ReadAllText(ClaimsFilePath);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        _claims = JsonSerializer.Deserialize<List<Claim>>(json, options) ?? new List<Claim>();
                        System.Diagnostics.Debug.WriteLine($"ApprovalController.LoadClaimsFromFile: Loaded {_claims.Count} claims from file");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ApprovalController.LoadClaimsFromFile: No claims file found");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading claims from file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                _claims = new List<Claim>();
            }
        }

        private void SaveClaimsToFile()
        {
            try
            {
                var directory = Path.GetDirectoryName(ClaimsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_claims, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(ClaimsFilePath, json);
                System.Diagnostics.Debug.WriteLine($"Saved {_claims.Count} claims to file");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving claims to file: {ex.Message}");
            }
        }

        // Method to reset static data for testing
        public static void ResetStaticData()
        {
            _approvals.Clear();
            _nextApprovalId = 1;
            _claims.Clear();
        }
    }
}