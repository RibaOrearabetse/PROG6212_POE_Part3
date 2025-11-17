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
            // Load data from file if not already loaded
            if (!_approvals.Any())
            {
                LoadApprovalsFromFile();

                // If still no approvals after loading, initialize with sample data
                if (!_approvals.Any())
                {
                    _approvals.AddRange(GetSampleApprovals());
                    _nextApprovalId = _approvals.Max(a => a.ApprovalID) + 1;
                    SaveApprovalsToFile();
                }
            }

            var approvals = _approvals.OrderByDescending(a => a.ApprovalDate).ToList();
            return View(approvals);
        }

        public IActionResult PendingClaims()
        {
            try
            {
                // Get all pending claims for coordinators and managers
                var pendingClaims = GetPendingClaims();
                System.Diagnostics.Debug.WriteLine($"Found {pendingClaims.Count} pending claims");
                return View("PendingClaims", pendingClaims);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PendingClaims: {ex.Message}");
                return View("PendingClaims", new List<Claim>());
            }
        }
        public IActionResult Details(int id)
        {
            // Load approvals from file if not already loaded
            if (!_approvals.Any())
            {
                LoadApprovalsFromFile();

                // If still no approvals after loading, initialize with sample data
                if (!_approvals.Any())
                {
                    _approvals.AddRange(GetSampleApprovals());
                    _nextApprovalId = _approvals.Max(a => a.ApprovalID) + 1;
                    SaveApprovalsToFile();
                }
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
            // Load approvals from file if not already loaded
            if (!_approvals.Any())
            {
                LoadApprovalsFromFile();

                // If still no approvals after loading, initialize with sample data
                if (!_approvals.Any())
                {
                    _approvals.AddRange(GetSampleApprovals());
                    _nextApprovalId = _approvals.Max(a => a.ApprovalID) + 1;
                    SaveApprovalsToFile();
                }
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

                // Load current data
                LoadClaimsFromFile();
                LoadApprovalsFromFile();

                var claim = GetClaimById(claimId);
                if (claim != null)
                {
                    // Update claim status in the shared claim storage
                    claim.ClaimStatus = "Approved";
                    claim.LastUpdated = DateTime.Now;
                    claim.StatusNotes = comments ?? "Approved by coordinator/manager";

                    // Create approval record
                    var approval = new Approval
                    {
                        ApprovalID = _nextApprovalId++,
                        ClaimID = claimId,
                        ApprovalDate = DateTime.Now,
                        Comments = comments ?? "Approved by coordinator/manager",
                        ApproverID = 1 // For demo purposes - in real app, get from session/auth
                    };

                    _approvals.Add(approval);

                    // Save to files
                    SaveClaimsToFile();
                    SaveApprovalsToFile();

                    System.Diagnostics.Debug.WriteLine($"Claim {claimId} approved successfully");
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

                // Load current data
                LoadClaimsFromFile();
                LoadApprovalsFromFile();

                var claim = GetClaimById(claimId);
                if (claim != null)
                {
                    // Update claim status in the shared claim storage
                    claim.ClaimStatus = "Rejected";
                    claim.LastUpdated = DateTime.Now;
                    claim.StatusNotes = comments ?? "Rejected by coordinator/manager";

                    // Create approval record
                    var approval = new Approval
                    {
                        ApprovalID = _nextApprovalId++,
                        ClaimID = claimId,
                        ApprovalDate = DateTime.Now,
                        Comments = comments ?? "Rejected by coordinator/manager",
                        ApproverID = 1 // For demo purposes - in real app, get from session/auth
                    };

                    _approvals.Add(approval);

                    // Save to files
                    SaveClaimsToFile();
                    SaveApprovalsToFile();

                    System.Diagnostics.Debug.WriteLine($"Claim {claimId} rejected successfully");
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
                return Json(new { success = false, message = "An error occurred while rejecting the claim." });
            }
        }
        private List<Claim> GetPendingClaims()
        {
            // Get all claims with "Pending", "Approved", or "Rejected" status from the shared claim storage
            // This shows pending claims plus recently processed ones
            var allClaims = GetSharedClaims();
            System.Diagnostics.Debug.WriteLine($"GetPendingClaims: Found {allClaims.Count} total claims");

            var relevantClaims = allClaims.Where(c =>
                c.ClaimStatus == "Pending" ||
                c.ClaimStatus == "Approved" ||
                c.ClaimStatus == "Rejected"
            ).OrderByDescending(c => c.SubmissionDate).ToList();

            System.Diagnostics.Debug.WriteLine($"GetPendingClaims: Found {relevantClaims.Count} relevant claims");
            foreach (var claim in relevantClaims)
            {
                System.Diagnostics.Debug.WriteLine($"Claim {claim.ClaimID}: Status={claim.ClaimStatus}, Amount={claim.TotalAmount}");
            }

            return relevantClaims;
        }

        private Claim GetClaimById(int claimId)
        {
            var allClaims = GetSharedClaims();
            return allClaims.FirstOrDefault(c => c.ClaimID == claimId);
        }

        private List<Claim> GetSharedClaims()
        {
            // If no claims loaded yet, try to load from file or initialize with sample data
            if (!_claims.Any())
            {
                System.Diagnostics.Debug.WriteLine("No claims in memory, loading from file...");
                LoadClaimsFromFile();

                // If still no claims after loading, initialize with sample data
                if (!_claims.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No claims found in file, initializing with sample data...");
                    _claims.AddRange(GetSampleClaims());
                    SaveClaimsToFile();
                    System.Diagnostics.Debug.WriteLine($"Initialized with {_claims.Count} sample claims");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Loaded {_claims.Count} claims from file");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Using {_claims.Count} claims from memory");
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

        private List<Approval> GetSampleApprovals()
        {
            return new List<Approval>
            {
                new Approval { ApprovalID = 1, ApprovalDate = DateTime.Now.AddDays(-3), Comments = "Approved - documentation complete", ClaimID = 2, ApproverID = 4 },
                new Approval { ApprovalID = 2, ApprovalDate = DateTime.Now.AddDays(-10), Comments = "Rejected - insufficient documentation", ClaimID = 3, ApproverID = 3 },
                new Approval { ApprovalID = 3, ApprovalDate = DateTime.Now.AddDays(-1), Comments = "Approved with conditions", ClaimID = 1, ApproverID = 4 }
            };
        }

        private void LoadApprovalsFromFile()
        {
            try
            {
                if (System.IO.File.Exists(ApprovalsFilePath))
                {
                    var json = System.IO.File.ReadAllText(ApprovalsFilePath);
                    if (!string.IsNullOrEmpty(json))
                    {
                        _approvals = JsonSerializer.Deserialize<List<Approval>>(json) ?? new List<Approval>();
                        if (_approvals.Any())
                        {
                            _nextApprovalId = _approvals.Max(a => a.ApprovalID) + 1;
                        }
                        System.Diagnostics.Debug.WriteLine($"Loaded {_approvals.Count} approvals from file");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading approvals from file: {ex.Message}");
                _approvals = new List<Approval>();
            }
        }

        private void SaveApprovalsToFile()
        {
            try
            {
                var directory = Path.GetDirectoryName(ApprovalsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_approvals, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(ApprovalsFilePath, json);
                System.Diagnostics.Debug.WriteLine($"Saved {_approvals.Count} approvals to file");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving approvals to file: {ex.Message}");
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
                        _claims = JsonSerializer.Deserialize<List<Claim>>(json) ?? new List<Claim>();
                        System.Diagnostics.Debug.WriteLine($"Loaded {_claims.Count} claims from file");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No claims file found, will initialize with sample data if needed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading claims from file: {ex.Message}");
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