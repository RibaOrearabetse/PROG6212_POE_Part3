using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Monthly_Claim_System__CMCS_.Models
{
    public class Approval
    {
        [Key]
        public int ApprovalID { get; set; }
        public DateTime ApprovalDate { get; set; }
        public string Comments { get; set; } = string.Empty;

        [ForeignKey("Claim")]
        public int ClaimID { get; set; }
        public virtual Claim Claim { get; set; } = null!;

        [ForeignKey("User")]
        public int ApproverID { get; set; }
        public virtual User Approver { get; set; } = null!;
    }
}
