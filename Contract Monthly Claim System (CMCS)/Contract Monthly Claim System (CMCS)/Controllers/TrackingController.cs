using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Models;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class TrackingController : Controller
    {
        public IActionResult Index()
        {
            // Get all claims for tracking from shared data
            var claims = GetSharedClaims();

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"TrackingController.Index - Found {claims.Count} claims");
            foreach (var claim in claims)
            {
                System.Diagnostics.Debug.WriteLine($"Claim {claim.ClaimID}: {claim.ClaimStatus} - {claim.TotalAmount:C}");
            }

            return View(claims);
        }

        public IActionResult Details(int id)
        {
            var claim = GetSharedClaims().FirstOrDefault(c => c.ClaimID == id);
            if (claim == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(claim);
        }

        public IActionResult GetClaimStatus(int claimId)
        {
            var claim = GetSharedClaims().FirstOrDefault(c => c.ClaimID == claimId);
            if (claim == null)
            {
                return Json(new { error = "Claim not found" });
            }

            return Json(new
            {
                claimId = claim.ClaimID,
                status = claim.ClaimStatus,
                statusDisplayName = claim.StatusDisplayName,
                statusBadgeClass = claim.StatusBadgeClass,
                statusProgress = claim.StatusProgress,
                lastUpdated = claim.LastUpdated?.ToString("yyyy-MM-dd HH:mm") ?? claim.SubmissionDate.ToString("yyyy-MM-dd HH:mm"),
                statusNotes = claim.StatusNotes
            });
        }

        public IActionResult UpdateStatus(int claimId, string newStatus, string notes = null)
        {
            try
            {
                // Get the claim from the shared ClaimController data
                var claims = ClaimController.GetAllClaims();
                var claim = claims.FirstOrDefault(c => c.ClaimID == claimId);

                if (claim == null)
                {
                    return Json(new { success = false, message = "Claim not found" });
                }

                // Update claim status in the shared data
                claim.ClaimStatus = newStatus;
                claim.LastUpdated = DateTime.Now;
                claim.StatusNotes = notes;

                // Save the updated data to file
                ClaimController.SaveClaimsToFile();

                System.Diagnostics.Debug.WriteLine($"TrackingController.UpdateStatus - Updated claim {claimId} to {newStatus}");

                return Json(new
                {
                    success = true,
                    message = $"Claim status updated to {newStatus}",
                    statusDisplayName = claim.StatusDisplayName,
                    statusBadgeClass = claim.StatusBadgeClass,
                    statusProgress = claim.StatusProgress
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TrackingController.UpdateStatus - Error: {ex.Message}");
                return Json(new { success = false, message = $"Error updating status: {ex.Message}" });
            }
        }

        private List<Claim> GetSharedClaims()
        {
            // Get claims from the shared ClaimController data
            var claims = ClaimController.GetAllClaims();

            // If no claims exist, initialize with sample data
            if (!claims.Any())
            {
                System.Diagnostics.Debug.WriteLine("TrackingController - No claims found, initializing with sample data");
                // Initialize the ClaimController with sample data
                var sampleClaims = GetSampleClaims();
                foreach (var claim in sampleClaims)
                {
                    ClaimController.AddClaim(claim);
                }
                claims = ClaimController.GetAllClaims();
            }

            System.Diagnostics.Debug.WriteLine($"TrackingController.GetSharedClaims - Returning {claims.Count} claims");
            return claims;
        }

        private List<Claim> GetSampleClaims()
        {
            return new List<Claim>
            {
                new Claim
                {
                    ClaimID = 1,
                    ClaimDate = DateTime.Now.AddDays(-7),
                    ClaimStatus = "Pending",
                    HoursWorked = 40,
                    HourlyRate = 25.00m,
                    TotalAmount = 1000.00m,
                    SubmissionDate = DateTime.Now.AddDays(-5),
                    LastUpdated = DateTime.Now.AddDays(-5),
                    StatusNotes = "Submitted for review",
                    UserID = 1,
                    Notes = "Sample claim 1"
                },
                new Claim
                {
                    ClaimID = 2,
                    ClaimDate = DateTime.Now.AddDays(-14),
                    ClaimStatus = "Approved",
                    HoursWorked = 35,
                    HourlyRate = 30.00m,
                    TotalAmount = 1050.00m,
                    SubmissionDate = DateTime.Now.AddDays(-12),
                    LastUpdated = DateTime.Now.AddDays(-10),
                    StatusNotes = "Approved by coordinator",
                    UserID = 2,
                    Notes = "Sample claim 2"
                },
                new Claim
                {
                    ClaimID = 3,
                    ClaimDate = DateTime.Now.AddDays(-21),
                    ClaimStatus = "Rejected",
                    HoursWorked = 20,
                    HourlyRate = 28.00m,
                    TotalAmount = 560.00m,
                    SubmissionDate = DateTime.Now.AddDays(-18),
                    LastUpdated = DateTime.Now.AddDays(-15),
                    StatusNotes = "Insufficient documentation",
                    UserID = 3,
                    Notes = "Sample claim 3"
                },
                new Claim
                {
                    ClaimID = 4,
                    ClaimDate = DateTime.Now.AddDays(-3),
                    ClaimStatus = "Processing",
                    HoursWorked = 45,
                    HourlyRate = 32.00m,
                    TotalAmount = 1440.00m,
                    SubmissionDate = DateTime.Now.AddDays(-1),
                    LastUpdated = DateTime.Now.AddDays(-1),
                    StatusNotes = "Payment being processed",
                    UserID = 1,
                    Notes = "Sample claim 4"
                },
                new Claim
                {
                    ClaimID = 5,
                    ClaimDate = DateTime.Now.AddDays(-30),
                    ClaimStatus = "Completed",
                    HoursWorked = 38,
                    HourlyRate = 28.00m,
                    TotalAmount = 1064.00m,
                    SubmissionDate = DateTime.Now.AddDays(-28),
                    LastUpdated = DateTime.Now.AddDays(-5),
                    StatusNotes = "Payment completed",
                    UserID = 2,
                    Notes = "Sample claim 5"
                }
            };
        }
    }
}
