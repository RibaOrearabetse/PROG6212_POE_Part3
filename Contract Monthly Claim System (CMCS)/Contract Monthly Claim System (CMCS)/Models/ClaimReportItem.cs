namespace Contract_Monthly_Claim_System__CMCS_.Models
{
    public class ClaimReportItem
    {
        public int ClaimID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}

