using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System__CMCS_.Models
{
    public class ClaimCreateViewModel
    {
        [Required]
        public DateTime ClaimDate { get; set; }

        [Required]
        [Range(0.1, 168, ErrorMessage = "Hours worked must be between 0.1 and 168")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hourly rate must be greater than 0")]
        public decimal HourlyRate { get; set; }

        public string? Notes { get; set; }
    }
}
