using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Linq;

namespace Contract_Monthly_Claim_System__CMCS_.Controllers
{
    public class UserController : Controller
    {
        private static List<User> _users = new List<User>();
        private static int _nextUserId = 1;
        private static readonly string DataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");
        // GET: User/Index - Shows the list of users
        public IActionResult Index()
        {
            // Load data from file if not already loaded
            if (!_users.Any())
            {
                LoadUsersFromFile();
            }

            // Filter out corrupted/empty users (those with UserID = 0 or empty names/emails)
            var validUsers = _users.Where(u => u.UserID > 0 && !string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.Email)).ToList();
            
            // If we have corrupted data, clean it up
            if (_users.Count != validUsers.Count)
            {
                System.Diagnostics.Debug.WriteLine($"Filtered out {_users.Count - validUsers.Count} corrupted user entries");
                _users = validUsers;
                if (_users.Any())
                {
                    _nextUserId = _users.Max(u => u.UserID) + 1;
                    SaveUsersToFile();
                }
            }

            // If no valid users exist, or if we only have 1-2 users, add sample data for better display
            if (!_users.Any())
            {
                System.Diagnostics.Debug.WriteLine("No valid users found, initializing with sample data");
                _users.AddRange(GetSampleUsers());
                _nextUserId = _users.Max(u => u.UserID) + 1;
                SaveUsersToFile();
            }
            else if (_users.Count <= 2)
            {
                // Add sample users if we have very few users, but don't duplicate existing ones
                var sampleUsers = GetSampleUsers();
                var existingUserIds = _users.Select(u => u.UserID).ToHashSet();
                var newSampleUsers = sampleUsers.Where(su => !existingUserIds.Contains(su.UserID)).ToList();
                
                if (newSampleUsers.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"Adding {newSampleUsers.Count} sample users to enhance the dataset");
                    _users.AddRange(newSampleUsers);
                    _nextUserId = _users.Max(u => u.UserID) + 1;
                    SaveUsersToFile();
                }
            }

            var users = _users.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();
            System.Diagnostics.Debug.WriteLine($"UserController.Index - Returning {users.Count} valid users to view");
            return View(users);
        }

        // GET: User/Details/5 - Shows details for a single user
        public IActionResult Details(int id)
        {
            // Load data from file if not already loaded
            if (!_users.Any())
            {
                LoadUsersFromFile();

                // If still no users after loading, initialize with sample data
                if (!_users.Any())
                {
                    _users.AddRange(GetSampleUsers());
                    _nextUserId = _users.Max(u => u.UserID) + 1;
                    SaveUsersToFile();
                }
            }

            var user = _users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Create - Shows the empty create form
        public IActionResult Create()
        {
            System.Diagnostics.Debug.WriteLine("GET Create action called");
            ViewBag.Roles = GetSampleRoles();
            return View();
        }

        // GET: User/Edit/5 - Shows the edit form pre-populated with user data
        public IActionResult Edit(int id)
        {
            // Load data from file if not already loaded
            if (!_users.Any())
            {
                LoadUsersFromFile();

                // If still no users after loading, initialize with sample data
                if (!_users.Any())
                {
                    _users.AddRange(GetSampleUsers());
                    _nextUserId = _users.Max(u => u.UserID) + 1;
                    SaveUsersToFile();
                }
            }

            var user = _users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = GetSampleRoles();
            return View(user);
        }

        // POST: User/Create - Handles the form submission for creating a new user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePost()
        {
            try
            {
                // Get form values directly
                string firstName = Request.Form["FirstName"];
                string lastName = Request.Form["LastName"];
                string email = Request.Form["Email"];
                string contactNumber = Request.Form["ContactNumber"];
                string hourlyRateStr = Request.Form["HourlyRate"];
                string roleIdStr = Request.Form["RoleID"];

                // Simple validation
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(roleIdStr))
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return View();
                }

                // Parse RoleID
                if (!int.TryParse(roleIdStr, out int roleId))
                {
                    TempData["ErrorMessage"] = "Invalid role selected.";
                    return View();
                }

                // Parse HourlyRate (optional)
                decimal hourlyRate = 0;
                if (!string.IsNullOrEmpty(hourlyRateStr) && !decimal.TryParse(hourlyRateStr, out hourlyRate))
                {
                    TempData["ErrorMessage"] = "Invalid hourly rate format.";
                    return View();
                }

                // Check if email already exists
                if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["ErrorMessage"] = "A user with this email already exists.";
                    return View();
                }

                // Create new user
                var newUser = new User
                {
                    UserID = _nextUserId++,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    ContactNumber = contactNumber ?? "",
                    HourlyRate = hourlyRate,
                    RoleID = roleId,
                    PasswordHash = "default_password_hash"
                };

                // Add user to the list
                _users.Add(newUser);

                // Save to file
                SaveUsersToFile();

                TempData["SuccessMessage"] = $"User {newUser.FirstName} {newUser.LastName} created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View();
            }
        }

        // POST: User/Edit/5 - Handles the form submission for editing an existing user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPost(int id)
        {
            try
            {
                // Get form values directly
                string firstName = Request.Form["FirstName"];
                string lastName = Request.Form["LastName"];
                string email = Request.Form["Email"];
                string contactNumber = Request.Form["ContactNumber"];
                string hourlyRateStr = Request.Form["HourlyRate"];
                string roleIdStr = Request.Form["RoleID"];

                // Simple validation
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(roleIdStr))
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return RedirectToAction(nameof(Edit), new { id = id });
                }

                // Parse RoleID
                if (!int.TryParse(roleIdStr, out int roleId))
                {
                    TempData["ErrorMessage"] = "Invalid role selected.";
                    return RedirectToAction(nameof(Edit), new { id = id });
                }

                // Parse HourlyRate (optional)
                decimal hourlyRate = 0;
                if (!string.IsNullOrEmpty(hourlyRateStr) && !decimal.TryParse(hourlyRateStr, out hourlyRate))
                {
                    TempData["ErrorMessage"] = "Invalid hourly rate format.";
                    return RedirectToAction(nameof(Edit), new { id = id });
                }

                // Find the existing user
                var existingUser = _users.FirstOrDefault(u => u.UserID == id);
                if (existingUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if email already exists (excluding current user)
                if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.UserID != id))
                {
                    TempData["ErrorMessage"] = "A user with this email already exists.";
                    return RedirectToAction(nameof(Edit), new { id = id });
                }

                // Update the user
                existingUser.FirstName = firstName;
                existingUser.LastName = lastName;
                existingUser.Email = email;
                existingUser.ContactNumber = contactNumber ?? "";
                existingUser.HourlyRate = hourlyRate;
                existingUser.RoleID = roleId;
                // Keep existing password hash

                // Save to file
                SaveUsersToFile();

                TempData["SuccessMessage"] = $"User {existingUser.FirstName} {existingUser.LastName} updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
        }

        private static List<User> GetSampleUsers()
        {
            return new List<User>
            {
                new User { UserID = 1, FirstName = "Sizwe", LastName = "Mahlangu", Email = "sizwe.m@university.edu", ContactNumber = "123-456-7890", RoleID = 1 },
                new User { UserID = 2, FirstName = "Khumo", LastName = "Thato", Email = "khumo.t@university.edu", ContactNumber = "098-765-4321", RoleID = 2 },
                new User { UserID = 3, FirstName = "Mike", LastName = "Johnson", Email = "mike.johnson@university.edu", ContactNumber = "555-123-4567", RoleID = 1 },
                new User { UserID = 4, FirstName = "Sarah", LastName = "Williams", Email = "sarah.williams@university.edu", ContactNumber = "444-987-6543", RoleID = 3 }
            };
        }

        private List<Role> GetSampleRoles()
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
            _users.Clear();
            _nextUserId = 1;
        }

        // Static method to get all users for use by other controllers
        public static List<User> GetAllUsers()
        {
            // Ensure data is loaded
            if (!_users.Any())
            {
                LoadUsersFromFile();
                if (!_users.Any())
                {
                    _users.AddRange(GetSampleUsers());
                    _nextUserId = _users.Max(u => u.UserID) + 1;
                    SaveUsersToFile();
                }
            }
            return _users;
        }

        // File persistence methods
        private static void SaveUsersToFile()
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

                var json = JsonSerializer.Serialize(_users, options);
                System.IO.File.WriteAllText(DataFilePath, json);

                // Log successful save
                System.Diagnostics.Debug.WriteLine($"Successfully saved {_users.Count} users to file: {DataFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving users to file: {ex.Message}");
                throw; // Re-throw to be caught by calling method
            }
        }

        private static void LoadUsersFromFile()
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
                        var users = JsonSerializer.Deserialize<List<User>>(json, options);
                        if (users != null && users.Any())
                        {
                            _users = users;
                            _nextUserId = _users.Max(u => u.UserID) + 1;
                            System.Diagnostics.Debug.WriteLine($"Loaded {_users.Count} users from file: {DataFilePath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users from file: {ex.Message}");
            }
        }
    }
}