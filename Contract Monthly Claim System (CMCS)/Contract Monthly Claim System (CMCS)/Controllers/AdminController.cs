using Contract_Monthly_Claim_System__CMCS_.Data;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class AdminController : Controller
    {
        private readonly CmcsDbContext _context;

        private const int CoordinatorRoleId = 2;
        private const int ManagerRoleId = 3;

        private const string CoordinatorSessionKey = "CoordinatorID";
        private const string CoordinatorNameKey = "CoordinatorName";
        private const string ManagerSessionKey = "ManagerID";
        private const string ManagerNameKey = "ManagerName";

        public AdminController(CmcsDbContext context)
        {
            _context = context;
        }

        public IActionResult Login(string role = "Coordinator")
        {
            var normalizedRole = NormalizeRole(role);
            var model = new AdminLoginViewModel
            {
                Role = normalizedRole
            };
            ViewBag.RoleDisplayName = GetRoleDisplayName(normalizedRole);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            var normalizedRole = NormalizeRole(model.Role);
            ViewBag.RoleDisplayName = GetRoleDisplayName(normalizedRole);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var roleId = normalizedRole == "Manager" ? ManagerRoleId : CoordinatorRoleId;
            var user = await FindAdminByEmailAsync(model.Email, roleId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid email or password.";
                return View(model);
            }

            if (!IsPasswordValid(model.Password, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Invalid email or password.";
                return View(model);
            }

            if (normalizedRole == "Manager")
            {
                HttpContext.Session.SetInt32(ManagerSessionKey, user.UserID);
                HttpContext.Session.SetString(ManagerNameKey, $"{user.FirstName} {user.LastName}");
                return RedirectToAction("Dashboard", "Manager");
            }
            else
            {
                HttpContext.Session.SetInt32(CoordinatorSessionKey, user.UserID);
                HttpContext.Session.SetString(CoordinatorNameKey, $"{user.FirstName} {user.LastName}");
                return RedirectToAction("Dashboard", "Coordinator");
            }
        }

        public IActionResult Logout(string role = "Coordinator")
        {
            var normalizedRole = NormalizeRole(role);
            if (normalizedRole == "Manager")
            {
                HttpContext.Session.Remove(ManagerSessionKey);
                HttpContext.Session.Remove(ManagerNameKey);
            }
            else
            {
                HttpContext.Session.Remove(CoordinatorSessionKey);
                HttpContext.Session.Remove(CoordinatorNameKey);
            }

            TempData["SuccessMessage"] = $"{GetRoleDisplayName(normalizedRole)} logged out successfully.";
            return RedirectToAction(nameof(Login), new { role = normalizedRole });
        }

        private async Task<User?> FindAdminByEmailAsync(string email, int roleId)
        {
            try
            {
                if (_context != null)
                {
                    return await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.RoleID == roleId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminController.FindAdminByEmailAsync DB error: {ex.Message}. Falling back to JSON data.");
            }

            var users = UserController.GetAllUsers();
            return users?.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.RoleID == roleId);
        }

        private bool IsPasswordValid(string providedPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || storedHash == "default_password_hash")
            {
                // Allow any password if hash not set
                return true;
            }

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(providedPassword));
                var hash = Convert.ToBase64String(hashedBytes);
                return string.Equals(hash, storedHash, StringComparison.Ordinal);
            }
        }

        private string NormalizeRole(string role)
        {
            if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "AcademicManager", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Academic Manager", StringComparison.OrdinalIgnoreCase))
            {
                return "Manager";
            }
            return "Coordinator";
        }

        private string GetRoleDisplayName(string role)
        {
            return role == "Manager" ? "Academic Manager" : "Programme Coordinator";
        }
    }
}

