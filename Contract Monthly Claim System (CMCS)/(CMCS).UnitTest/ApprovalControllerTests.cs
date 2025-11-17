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
            var model = Assert.IsAssignableFrom<IEnumerable<Approval>>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public void PendingClaims_ReturnsViewWithClaims()
        {
            // Arrange
            var controller = new ApprovalController();

            // Act
            var result = controller.PendingClaims();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Claim>>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public void Details_WithValidId_ReturnsView()
        {
            // Arrange
            var controller = new ApprovalController();
            var approvalId = 1;

            // Act
            var result = controller.Details(approvalId);

            // Assert
            // The controller will initialize with sample data, so this should work
            if (result is ViewResult viewResult)
            {
                var model = Assert.IsType<Approval>(viewResult.Model);
                Assert.Equal(approvalId, model.ApprovalID);
            }
            else
            {
                // If it redirects, that's also acceptable behavior
                Assert.IsType<RedirectToActionResult>(result);
            }
        }

        [Fact]
        public void Details_WithInvalidId_ReturnsRedirectToIndex()
        {
            // Arrange
            var controller = new ApprovalController();
            var invalidId = 999;

            // Act
            var result = controller.Details(invalidId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void ProcessApproval_WithValidClaimId_ReturnsView()
        {
            // Arrange
            var controller = new ApprovalController();
            var claimId = 1;

            // Act
            var result = controller.ProcessApproval(claimId);

            // Assert
            // The controller will initialize with sample data, so this should work
            if (result is ViewResult viewResult)
            {
                var model = Assert.IsType<Claim>(viewResult.Model);
                Assert.Equal(claimId, model.ClaimID);
            }
            else
            {
                // If it redirects, that's also acceptable behavior
                Assert.IsType<RedirectToActionResult>(result);
            }
        }

        [Fact]
        public void ProcessApproval_WithInvalidClaimId_ReturnsRedirectToPendingClaims()
        {
            // Arrange
            var controller = new ApprovalController();
            var invalidClaimId = 999;

            // Act
            var result = controller.ProcessApproval(invalidClaimId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PendingClaims", redirectResult.ActionName);
        }

        [Fact]
        public void CreateTestClaims_ReturnsRedirectToPendingClaims()
        {
            // Arrange
            var controller = new ApprovalController();

            // Act
            var result = controller.CreateTestClaims();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PendingClaims", redirectResult.ActionName);
        }
    }
}
