using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    public class ApprovalControllerTests
    {
        [Fact]
        public void Index_ReturnsViewWithApprovals()
        {
            // Arrange
            var controller = new ApprovalController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public void PendingClaims_ReturnsView()
        {
            // Arrange
            var controller = new ApprovalController();

            // Act
            var result = controller.PendingClaims();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public void Details_WithInvalidId_ReturnsRedirect()
        {
            // Arrange
            var controller = new ApprovalController();
            var invalidId = 99999;

            // Act
            var result = controller.Details(invalidId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void ProcessApproval_WithInvalidClaimId_ReturnsRedirect()
        {
            // Arrange
            var controller = new ApprovalController();
            var invalidClaimId = 99999;

            // Act
            var result = controller.ProcessApproval(invalidClaimId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void GetAllApprovals_ReturnsList()
        {
            // Act
            var result = ApprovalController.GetAllApprovals();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<Approval>>(result);
        }
    }
}
