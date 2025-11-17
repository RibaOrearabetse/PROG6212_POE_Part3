using System.Diagnostics;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Get actual data counts
            var users = UserController.GetAllUsers() ?? new List<User>();
            var claims = ClaimController.GetAllClaims() ?? new List<Claim>();

            // Ensure we have counts, default to 0 if null
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalClaims = claims.Count;
            ViewBag.ApprovedClaims = claims.Count(c => c.ClaimStatus == "Approved");
            ViewBag.PendingClaims = claims.Count(c => c.ClaimStatus == "Pending");

            // Debug logging
            _logger.LogInformation($"Index - Total Users: {ViewBag.TotalUsers}, Total Claims: {ViewBag.TotalClaims}, Approved: {ViewBag.ApprovedClaims}, Pending: {ViewBag.PendingClaims}");

            return View();
        }

        public IActionResult Dashboard()
        {
            // Get actual data counts
            var claims = ClaimController.GetAllClaims() ?? new List<Claim>();

            // Ensure we have counts, default to 0 if null
            ViewBag.TotalClaims = claims.Count;
            ViewBag.PendingClaims = claims.Count(c => c.ClaimStatus == "Pending");
            ViewBag.ApprovedClaims = claims.Count(c => c.ClaimStatus == "Approved");
            ViewBag.RejectedClaims = claims.Count(c => c.ClaimStatus == "Rejected");
            
            // Get recent claims (last 5, ordered by submission date)
            // Use a safe ordering that handles any edge cases
            var recentClaims = claims
                .Where(c => c != null)
                .OrderByDescending(c => c.SubmissionDate)
                .Take(5)
                .ToList();
            ViewBag.RecentClaims = recentClaims;

            // Debug logging
            _logger.LogInformation($"Dashboard - Total Claims: {ViewBag.TotalClaims}, Pending: {ViewBag.PendingClaims}, Approved: {ViewBag.ApprovedClaims}, Rejected: {ViewBag.RejectedClaims}");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}