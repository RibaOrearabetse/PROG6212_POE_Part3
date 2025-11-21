using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class CoordinatorController : Controller
    {
        private const string SessionKey = "CoordinatorID";
        private const string NameKey = "CoordinatorName";

        public IActionResult Dashboard()
        {
            var coordinatorId = HttpContext.Session.GetInt32(SessionKey);
            if (coordinatorId == null)
            {
                TempData["ErrorMessage"] = "Please log in as Programme Coordinator.";
                return RedirectToAction("Login", "Admin", new { role = "Coordinator" });
            }

            var claims = ClaimController.GetAllClaims() ?? new List<Claim>();
            var pendingClaims = claims.Where(c => c.ClaimStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase)).ToList();
            var approvedClaims = claims.Where(c => c.ClaimStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase)).ToList();
            var rejectedClaims = claims.Where(c => c.ClaimStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.CoordinatorName = HttpContext.Session.GetString(NameKey) ?? "Coordinator";
            ViewBag.TotalClaims = claims.Count;
            ViewBag.PendingClaims = pendingClaims.Count;
            ViewBag.ApprovedClaims = approvedClaims.Count;
            ViewBag.RejectedClaims = rejectedClaims.Count;
            ViewBag.AllClaims = claims
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();
            ViewBag.RecentClaims = claims
                .OrderByDescending(c => c.SubmissionDate)
                .Take(5)
                .ToList();

            return View();
        }
    }
}

