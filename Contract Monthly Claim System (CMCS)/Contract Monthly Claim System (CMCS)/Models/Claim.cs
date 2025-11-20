using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Monthly_Claim_System__CMCS_.Models
{
    public class Claim
    {
        [Key]
        public int ClaimID { get; set; }
        [Required]
        public DateTime ClaimDate { get; set; }
        public string ClaimStatus { get; set; } = string.Empty;
        [Required]
        [Range(0.1, 168, ErrorMessage = "Hours worked must be between 0.1 and 168")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hourly rate must be greater than 0")]
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? StatusNotes { get; set; }
        public string? Notes { get; set; }

        [ForeignKey("User")]
        [Required(ErrorMessage = "User is required")]
        [Range(1, int.MaxValue, ErrorMessage = "A valid user must be selected")]
        public int UserID { get; set; }

        [NotMapped]
        public virtual User User { get; set; } = null!;

        // Navigation properties - exclude from validation
        [NotMapped]
        public virtual ICollection<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();

        [NotMapped]
        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

        // Status tracking properties
        [NotMapped]
        public string StatusDisplayName => GetStatusDisplayName();

        [NotMapped]
        public string StatusBadgeClass => GetStatusBadgeClass();

        [NotMapped]
        public int StatusProgress => GetStatusProgress();

        private string GetStatusDisplayName()
        {
            if (string.IsNullOrEmpty(ClaimStatus))
                return string.Empty;

            return ClaimStatus switch
            {
                "Pending" => "Under Review",
                "Approved" => "Approved",
                "Rejected" => "Rejected",
                "Processing" => "Processing Payment",
                "Completed" => "Settled",
                _ => ClaimStatus
            };
        }

        private string GetStatusBadgeClass()
        {
            if (string.IsNullOrEmpty(ClaimStatus))
                return "bg-secondary";

            return ClaimStatus switch
            {
                "Pending" => "bg-warning text-dark",
                "Approved" => "bg-success",
                "Rejected" => "bg-danger",
                "Processing" => "bg-info",
                "Completed" => "bg-primary",
                _ => "bg-secondary"
            };
        }

        private int GetStatusProgress()
        {
            if (string.IsNullOrEmpty(ClaimStatus))
                return 0;

            return ClaimStatus switch
            {
                "Pending" => 25,
                "Approved" => 100,  // Changed from 75 to 100 for approved
                "Rejected" => 100,
                "Processing" => 90,
                "Completed" => 100,
                _ => 0
            };
        }
    }
}
