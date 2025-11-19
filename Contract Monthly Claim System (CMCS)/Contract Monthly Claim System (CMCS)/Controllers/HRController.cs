using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Models;
using System.Text.Json;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class HRController : Controller
    {
        // GET: HR/Index - HR Dashboard
        public IActionResult Index()
        {
            // Get statistics for HR dashboard
            var users = UserController.GetAllUsers();
            var claims = ClaimController.GetAllClaims();

            ViewBag.TotalUsers = users?.Count ?? 0;
            ViewBag.TotalClaims = claims?.Count ?? 0;
            ViewBag.PendingClaims = claims?.Count(c => c.ClaimStatus == "Pending") ?? 0;
            ViewBag.ApprovedClaims = claims?.Count(c => c.ClaimStatus == "Approved") ?? 0;
            ViewBag.RejectedClaims = claims?.Count(c => c.ClaimStatus == "Rejected") ?? 0;
            ViewBag.ProcessingClaims = claims?.Count(c => c.ClaimStatus == "Processing") ?? 0;

            return View();
        }

        // GET: HR/Users - Redirect to User Management
        public IActionResult Users()
        {
            return RedirectToAction("Index", "User");
        }

        // GET: HR/Reports - Reports and Invoices page
        public IActionResult Reports()
        {
            var claims = ClaimController.GetAllClaims();
            var users = UserController.GetAllUsers();

            ViewBag.Claims = claims;
            ViewBag.Users = users;

            return View();
        }

        // GET: HR/GenerateReport - Generate a report/invoice
        public IActionResult GenerateReport(int? userId = null, string status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var claims = ClaimController.GetAllClaims();
            var users = UserController.GetAllUsers();

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"HRController.GenerateReport - Total claims loaded: {claims?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"HRController.GenerateReport - Filters - userId: {userId}, status: {status}, startDate: {startDate}, endDate: {endDate}");

            // Ensure claims is not null
            if (claims == null || !claims.Any())
            {
                System.Diagnostics.Debug.WriteLine("HRController.GenerateReport - No claims found!");
                ViewBag.ReportData = new List<ClaimReportItem>();
                ViewBag.Users = users;
                ViewBag.SelectedUserId = userId;
                ViewBag.SelectedStatus = status;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                return View();
            }

            // Filter claims based on parameters
            var filteredClaims = claims.AsEnumerable();

            if (userId.HasValue && userId.Value > 0)
            {
                filteredClaims = filteredClaims.Where(c => c.UserID == userId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                filteredClaims = filteredClaims.Where(c => c.ClaimStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (startDate.HasValue)
            {
                filteredClaims = filteredClaims.Where(c => c.SubmissionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredClaims = filteredClaims.Where(c => c.SubmissionDate <= endDate.Value);
            }

            var reportData = filteredClaims
                .Where(c => c != null && c.ClaimID > 0) // Filter out null or invalid claims
                .Select(c => new ClaimReportItem
                {
                    ClaimID = c.ClaimID,
                    UserName = (users?.FirstOrDefault(u => u.UserID == c.UserID)?.FirstName ?? "") + " " + (users?.FirstOrDefault(u => u.UserID == c.UserID)?.LastName ?? ""),
                    ClaimDate = c.ClaimDate,
                    Status = c.ClaimStatus ?? "Unknown",
                    HoursWorked = c.HoursWorked,
                    HourlyRate = c.HourlyRate,
                    TotalAmount = c.TotalAmount,
                    SubmissionDate = c.SubmissionDate
                })
                .ToList();

            System.Diagnostics.Debug.WriteLine($"HRController.GenerateReport - Report data count: {reportData.Count}");

            ViewBag.ReportData = reportData;
            ViewBag.Users = users;
            ViewBag.SelectedUserId = userId;
            ViewBag.SelectedStatus = status;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View();
        }

        // GET: HR/ExportReport - Export report as JSON (for now, can be extended to PDF)
        public IActionResult ExportReport(int? userId = null, string status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var claims = ClaimController.GetAllClaims();
            var users = UserController.GetAllUsers();

            // Filter claims based on parameters
            var filteredClaims = claims.AsEnumerable();

            if (userId.HasValue && userId.Value > 0)
            {
                filteredClaims = filteredClaims.Where(c => c.UserID == userId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                filteredClaims = filteredClaims.Where(c => c.ClaimStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (startDate.HasValue)
            {
                filteredClaims = filteredClaims.Where(c => c.SubmissionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredClaims = filteredClaims.Where(c => c.SubmissionDate <= endDate.Value);
            }

            var reportData = filteredClaims
                .Select(c => new
                {
                    ClaimID = c.ClaimID,
                    UserName = users?.FirstOrDefault(u => u.UserID == c.UserID)?.FirstName + " " + users?.FirstOrDefault(u => u.UserID == c.UserID)?.LastName,
                    ClaimDate = c.ClaimDate.ToString("yyyy-MM-dd"),
                    Status = c.ClaimStatus,
                    HoursWorked = c.HoursWorked,
                    HourlyRate = c.HourlyRate,
                    TotalAmount = c.TotalAmount,
                    SubmissionDate = c.SubmissionDate.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(reportData, options);
            var fileName = $"ClaimsReport_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }
    }
}

