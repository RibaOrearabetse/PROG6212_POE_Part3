using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System__CMCS_.Models
{
    public class AdminLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Coordinator";
    }
}

