using Contract_Monthly_Claim_System__CMCS_.Models;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.Tests
{
    public class BusinessLogicTests
    {
        [Fact]
        public void Claim_StatusDisplayName_ShouldReturnCorrectDisplayName()
        {
            // Arrange
            var claim = new Claim { ClaimStatus = "Pending" };

            // Act
            var displayName = claim.StatusDisplayName;

            // Assert
            Assert.Equal("Under Review", displayName);
        }

        [Fact]
        public void Claim_StatusBadgeClass_ShouldReturnCorrectClass()
        {
            // Arrange
            var claim = new Claim { ClaimStatus = "Approved" };

            // Act
            var badgeClass = claim.StatusBadgeClass;

            // Assert
            Assert.Equal("bg-success", badgeClass);
        }

        [Fact]
        public void Claim_StatusProgress_ShouldReturnCorrectProgress()
        {
            // Arrange
            var claim = new Claim { ClaimStatus = "Pending" };

            // Act
            var progress = claim.StatusProgress;

            // Assert
            Assert.Equal(25, progress);
        }

        [Theory]
        [InlineData("Pending", "Under Review", "bg-warning text-dark", 25)]
        [InlineData("Approved", "Approved", "bg-success", 100)]
        [InlineData("Rejected", "Rejected", "bg-danger", 100)]
        [InlineData("Processing", "Processing Payment", "bg-info", 90)]
        [InlineData("Completed", "Settled", "bg-primary", 100)]
        public void Claim_StatusProperties_ShouldReturnCorrectValues(
            string status,
            string expectedDisplayName,
            string expectedBadgeClass,
            int expectedProgress)
        {
            // Arrange
            var claim = new Claim { ClaimStatus = status };

            // Act & Assert
            Assert.Equal(expectedDisplayName, claim.StatusDisplayName);
            Assert.Equal(expectedBadgeClass, claim.StatusBadgeClass);
            Assert.Equal(expectedProgress, claim.StatusProgress);
        }

        [Fact]
        public void Claim_TotalAmountCalculation_ShouldBeCorrect()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 40,
                HourlyRate = 25.00m
            };

            // Act
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;

            // Assert
            Assert.Equal(1000.00m, claim.TotalAmount);
        }

        [Fact]
        public void User_FullName_ShouldBeConcatenatedCorrectly()
        {
            // Arrange
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var fullName = $"{user.FirstName} {user.LastName}";

            // Assert
            Assert.Equal("John Doe", fullName);
        }

        [Fact]
        public void Approval_WithValidData_ShouldBeCreated()
        {
            // Arrange
            var approval = new Approval
            {
                ApprovalID = 1,
                ClaimID = 1,
                ApprovalDate = DateTime.Now,
                Comments = "Approved",
                ApproverID = 1
            };

            // Act & Assert
            Assert.Equal(1, approval.ApprovalID);
            Assert.Equal(1, approval.ClaimID);
            Assert.Equal("Approved", approval.Comments);
            Assert.Equal(1, approval.ApproverID);
        }

        [Fact]
        public void SupportingDocument_WithValidData_ShouldBeCreated()
        {
            // Arrange
            var document = new SupportingDocument
            {
                DocumentID = 1,
                FileName = "Invoice.pdf",
                FilePath = "/documents/invoice.pdf",
                FileSize = 1024,
                ContentType = "application/pdf",
                UploadDate = DateTime.Now,
                ClaimID = 1
            };

            // Act & Assert
            Assert.Equal(1, document.DocumentID);
            Assert.Equal("Invoice.pdf", document.FileName);
            Assert.Equal("application/pdf", document.ContentType);
            Assert.Equal("/documents/invoice.pdf", document.FilePath);
            Assert.Equal(1024, document.FileSize);
            Assert.Equal(1, document.ClaimID);
        }
    }
}