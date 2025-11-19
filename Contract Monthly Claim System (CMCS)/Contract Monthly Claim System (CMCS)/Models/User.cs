using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace Contract_Monthly_Claim_System__CMCS_.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Hourly rate must be greater than 0")]
        [Display(Name = "Hourly Rate (ZAR)")]
        public decimal HourlyRate { get; set; }

        [ForeignKey("Role")]
        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public int RoleID { get; set; }
        public virtual Role Role { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    }
}
