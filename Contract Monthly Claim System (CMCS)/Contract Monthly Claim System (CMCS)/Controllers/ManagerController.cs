using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class ManagerController : Controller
    {
        private const string SessionKey = "ManagerID";
        private const string NameKey = "ManagerName";

        public IActionResult Dashboard()
        {
            var managerId = HttpContext.Session.GetInt32(SessionKey);
            if (managerId == null)
            {
                TempData["ErrorMessage"] = "Please log in as Academic Manager.";
                return RedirectToAction("Login", "Admin", new { role = "Manager" });
            }

            var claims = ClaimController.GetAllClaims() ?? new List<Claim>();
            var approvals = ApprovalController.GetAllApprovals() ?? new List<Approval>();

            ViewBag.ManagerName = HttpContext.Session.GetString(NameKey) ?? "Academic Manager";
            ViewBag.TotalClaims = claims.Count;
            ViewBag.ProcessingClaims = claims.Count(c => c.ClaimStatus.Equals("Processing", StringComparison.OrdinalIgnoreCase));
            ViewBag.CompletedClaims = claims.Count(c => c.ClaimStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) || c.ClaimStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase));
            ViewBag.RejectedClaims = claims.Count(c => c.ClaimStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase));
            ViewBag.AllClaims = claims
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();
            ViewBag.RecentApprovals = approvals
                .OrderByDescending(a => a.ApprovalDate)
                .Take(5)
                .ToList();

            return View();
        }
    }
}

