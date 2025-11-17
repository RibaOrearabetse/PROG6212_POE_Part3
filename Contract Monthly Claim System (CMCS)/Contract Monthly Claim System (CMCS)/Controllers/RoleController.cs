using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Linq;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class RoleController : Controller
    {
        private static List<Role> _roles = new List<Role>();
        private static int _nextRoleId = 1;
        private static readonly string DataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "roles.json");

        public IActionResult Index()
        {
            // Load data from file if not already loaded
            if (!_roles.Any())
            {
                LoadRolesFromFile();

                // If still no roles after loading, initialize with sample data
                if (!_roles.Any())
                {
                    _roles.AddRange(GetSampleRoles());
                    _nextRoleId = _roles.Max(r => r.RoleID) + 1;
                    SaveRolesToFile();
                }
            }

            // Filter out any roles with null or empty names and order by name
            var roles = _roles.Where(r => !string.IsNullOrEmpty(r.RoleName)).OrderBy(r => r.RoleName).ToList();
            return View(roles);
        }

        public IActionResult Details(int id)
        {
            // Load data from file if not already loaded
            if (!_roles.Any())
            {
                LoadRolesFromFile();

                // If still no roles after loading, initialize with sample data
                if (!_roles.Any())
                {
                    _roles.AddRange(GetSampleRoles());
                    _nextRoleId = _roles.Max(r => r.RoleID) + 1;
                    SaveRolesToFile();
                }
            }

            var role = _roles.FirstOrDefault(r => r.RoleID == id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePost()
        {
            try
            {
                // Get form values directly
                string roleName = Request.Form["RoleName"];

                // Simple validation
                if (string.IsNullOrEmpty(roleName))
                {
                    TempData["ErrorMessage"] = "Please enter a role name.";
                    return View("Create");
                }

                // Check if role already exists
                if (_roles.Any(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["ErrorMessage"] = "A role with this name already exists.";
                    return View("Create");
                }

                // Create new role
                var newRole = new Role
                {
                    RoleID = _nextRoleId++,
                    RoleName = roleName
                };

                // Add role to the list
                _roles.Add(newRole);

                // Save to file
                SaveRolesToFile();

                TempData["SuccessMessage"] = $"Role '{newRole.RoleName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View("Create");
            }
        }

        public IActionResult Edit(int id)
        {
            // Load data from file if not already loaded
            if (!_roles.Any())
            {
                LoadRolesFromFile();

                // If still no roles after loading, initialize with sample data
                if (!_roles.Any())
                {
                    _roles.AddRange(GetSampleRoles());
                    _nextRoleId = _roles.Max(r => r.RoleID) + 1;
                    SaveRolesToFile();
                }
            }

            var role = _roles.FirstOrDefault(r => r.RoleID == id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPost(int id)
        {
            try
            {
                // Get form values directly
                string roleName = Request.Form["RoleName"];

                // Simple validation
                if (string.IsNullOrEmpty(roleName))
                {
                    TempData["ErrorMessage"] = "Please enter a role name.";
                    return RedirectToAction(nameof(Edit), new { id = id });
                }

                // Find the existing role
                var existingRole = _roles.FirstOrDefault(r => r.RoleID == id);
                if (existingRole == null)
                {
                    TempData["ErrorMessage"] = "Role not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if role name already exists (excluding current role)
                if (_roles.Any(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase) && r.RoleID != id))
                {
                    TempData["ErrorMessage"] = "A role with this name already exists.";
                    return RedirectToAction(nameof(Edit), new { id = id });
                }

                // Update the role
                existingRole.RoleName = roleName;

                // Save to file
                SaveRolesToFile();

                TempData["SuccessMessage"] = $"Role '{existingRole.RoleName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var role = _roles.FirstOrDefault(r => r.RoleID == id);
                if (role == null)
                {
                    TempData["ErrorMessage"] = "Role not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if role is being used by any users
                // Note: In a real application, you'd check foreign key constraints
                var roleName = role.RoleName;
                _roles.Remove(role);

                // Save to file
                SaveRolesToFile();

                TempData["SuccessMessage"] = $"Role '{roleName}' deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private static List<Role> GetSampleRoles()
        {
            return new List<Role>
            {
                new Role { RoleID = 1, RoleName = "Lecturer" },
                new Role { RoleID = 2, RoleName = "Coordinator" },
                new Role { RoleID = 3, RoleName = "Manager" },
                new Role { RoleID = 4, RoleName = "Administrator" }
            };
        }

        // Method to reset static data for testing
        public static void ResetStaticData()
        {
            _roles.Clear();
            _nextRoleId = 1;
        }

        // Static method to get all roles for use by other controllers
        public static List<Role> GetAllRoles()
        {
            // Ensure data is loaded
            if (!_roles.Any())
            {
                LoadRolesFromFile();
                if (!_roles.Any())
                {
                    _roles.AddRange(GetSampleRoles());
                    _nextRoleId = _roles.Max(r => r.RoleID) + 1;
                    SaveRolesToFile();
                }
            }
            return _roles;
        }

        // File persistence methods
        private static void SaveRolesToFile()
        {
            try
            {
                // Ensure the Data directory exists
                var dataDir = Path.GetDirectoryName(DataFilePath);
                if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_roles, options);
                System.IO.File.WriteAllText(DataFilePath, json);

                // Log successful save
                System.Diagnostics.Debug.WriteLine($"Successfully saved {_roles.Count} roles to file: {DataFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving roles to file: {ex.Message}");
                throw;
            }
        }

        private static void LoadRolesFromFile()
        {
            try
            {
                if (System.IO.File.Exists(DataFilePath))
                {
                    var json = System.IO.File.ReadAllText(DataFilePath);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var roles = JsonSerializer.Deserialize<List<Role>>(json);
                        if (roles != null && roles.Any())
                        {
                            // Filter out any roles with null or empty names
                            _roles = roles.Where(r => !string.IsNullOrEmpty(r.RoleName)).ToList();
                            if (_roles.Any())
                            {
                                _nextRoleId = _roles.Max(r => r.RoleID) + 1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading roles from file: {ex.Message}");
            }
        }
    }
}