using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    public class ErrorHandlingTests
    {
        [Fact]
        public void UserController_Details_WithInvalidId_ShouldHandleGracefully()
        {
            // Arrange
            var controller = new UserController();
            var invalidId = 99999;

            // Act
            var result = controller.Details(invalidId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void ClaimController_Details_WithInvalidId_ShouldHandleGracefully()
        {
            // Arrange
            var controller = new ClaimController();
            var invalidId = 99999;

            // Act
            var result = controller.Details(invalidId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void ApprovalController_Details_WithInvalidId_ShouldHandleGracefully()
        {
            // Arrange
            var controller = new ApprovalController();
            var invalidId = 99999;

            // Act
            var result = controller.Details(invalidId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void RoleController_Details_WithInvalidId_ShouldHandleGracefully()
        {
            // Arrange
            var controller = new RoleController();
            var invalidId = 99999;

            // Act
            var result = controller.Details(invalidId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void User_WithNullProperties_ShouldNotThrowException()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.NotNull(user);
            Assert.Equal(string.Empty, user.FirstName);
            Assert.Equal(string.Empty, user.LastName);
            Assert.Equal(string.Empty, user.Email);
        }

        [Fact]
        public void Claim_WithNullProperties_ShouldNotThrowException()
        {
            // Arrange & Act
            var claim = new Claim();

            // Assert
            Assert.NotNull(claim);
            Assert.Equal(string.Empty, claim.ClaimStatus);
        }

        [Fact]
        public void Role_WithNullProperties_ShouldNotThrowException()
        {
            // Arrange & Act
            var role = new Role();

            // Assert
            Assert.NotNull(role);
            Assert.Equal(string.Empty, role.RoleName);
        }

        [Fact]
        public void Approval_WithNullProperties_ShouldNotThrowException()
        {
            // Arrange & Act
            var approval = new Approval();

            // Assert
            Assert.NotNull(approval);
            Assert.Equal(string.Empty, approval.Comments);
        }

        [Fact]
        public void SupportingDocument_WithNullProperties_ShouldNotThrowException()
        {
            // Arrange & Act
            var document = new SupportingDocument();

            // Assert
            Assert.NotNull(document);
            Assert.Equal(string.Empty, document.FileName);
            Assert.Equal(string.Empty, document.ContentType);
            Assert.Equal(string.Empty, document.FilePath);
        }

        [Fact]
        public void Claim_StatusDisplayName_WithEmptyStatus_ShouldReturnOriginalStatus()
        {
            // Arrange
            var claim = new Claim { ClaimStatus = "" };

            // Act
            var displayName = claim.StatusDisplayName;

            // Assert
            Assert.Equal("", displayName);
        }

        [Fact]
        public void Claim_StatusBadgeClass_WithEmptyStatus_ShouldReturnDefaultClass()
        {
            // Arrange
            var claim = new Claim { ClaimStatus = "" };

            // Act
            var badgeClass = claim.StatusBadgeClass;

            // Assert
            Assert.Equal("bg-secondary", badgeClass);
        }

        [Fact]
        public void Claim_StatusProgress_WithEmptyStatus_ShouldReturnZero()
        {
            // Arrange
            var claim = new Claim { ClaimStatus = "" };

            // Act
            var progress = claim.StatusProgress;

            // Assert
            Assert.Equal(0, progress);
        }
    }
}
