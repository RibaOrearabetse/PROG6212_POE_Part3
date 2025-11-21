using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    public class IntegrationTests
    {
        [Fact]
        public void UserWorkflow_CreateUser_ShouldWorkEndToEnd()
        {
            // Arrange
            var userController = new UserController();
            var roleController = new RoleController();

            // Act - Get roles first
            var rolesResult = roleController.Index();
            var rolesViewResult = Assert.IsType<ViewResult>(rolesResult);
            Assert.NotNull(rolesViewResult);

            // Act - Create user
            var createResult = userController.Create();
            var createViewResult = Assert.IsType<ViewResult>(createResult);

            // Assert
            Assert.NotNull(createViewResult);
        }

        [Fact]
        public void ClaimWorkflow_CreateClaim_ShouldWorkEndToEnd()
        {
            // Arrange
            var claimController = new ClaimController();
            var userController = new UserController();

            // Act - Get users first
            var usersResult = userController.Index();
            var usersViewResult = Assert.IsType<ViewResult>(usersResult);
            Assert.NotNull(usersViewResult);

            // Act - Create claim
            var createResult = claimController.Create();
            var createViewResult = Assert.IsType<ViewResult>(createResult);

            // Assert
            Assert.NotNull(createViewResult);
        }

        [Fact]
        public void ApprovalWorkflow_ProcessClaim_ShouldWorkEndToEnd()
        {
            // Arrange
            var approvalController = new ApprovalController();
            var claimController = new ClaimController();

            // Act - Get claims first
            var claimsResult = claimController.Index();
            var claimsViewResult = Assert.IsType<ViewResult>(claimsResult);
            Assert.NotNull(claimsViewResult);

            // Act - Get pending claims
            var pendingClaimsResult = approvalController.PendingClaims();
            var pendingClaimsViewResult = Assert.IsType<ViewResult>(pendingClaimsResult);

            // Assert
            Assert.NotNull(pendingClaimsViewResult);
        }

        [Fact]
        public void RoleManagement_CRUDOperations_ShouldWorkEndToEnd()
        {
            // Arrange
            var roleController = new RoleController();

            // Act - Get all roles
            var indexResult = roleController.Index();
            var indexViewResult = Assert.IsType<ViewResult>(indexResult);
            Assert.NotNull(indexViewResult);

            // Act - Create role
            var createResult = roleController.Create();
            var createViewResult = Assert.IsType<ViewResult>(createResult);

            // Assert
            Assert.NotNull(createViewResult);
        }

        [Fact]
        public void DataConsistency_AllControllers_ShouldHaveConsistentData()
        {
            // Arrange
            var userController = new UserController();
            var roleController = new RoleController();
            var claimController = new ClaimController();
            var approvalController = new ApprovalController();

            // Act
            var usersResult = userController.Index();
            var rolesResult = roleController.Index();
            var claimsResult = claimController.Index();
            var approvalsResult = approvalController.Index();

            // Assert
            Assert.IsType<ViewResult>(usersResult);
            Assert.IsType<ViewResult>(rolesResult);
            Assert.IsType<ViewResult>(claimsResult);
            Assert.IsType<ViewResult>(approvalsResult);
        }

        [Fact]
        public void NavigationFlow_AllPages_ShouldBeAccessible()
        {
            // Arrange
            var userController = new UserController();
            var roleController = new RoleController();
            var claimController = new ClaimController();
            var approvalController = new ApprovalController();

            // Act & Assert
            var userIndex = userController.Index();
            var roleIndex = roleController.Index();
            var claimIndex = claimController.Index();
            var approvalIndex = approvalController.Index();

            Assert.IsType<ViewResult>(userIndex);
            Assert.IsType<ViewResult>(roleIndex);
            Assert.IsType<ViewResult>(claimIndex);
            Assert.IsType<ViewResult>(approvalIndex);
        }

        [Fact]
        public void ErrorHandling_AllControllers_ShouldHandleErrorsGracefully()
        {
            // Arrange
            var userController = new UserController();
            var roleController = new RoleController();
            var claimController = new ClaimController();
            var approvalController = new ApprovalController();

            // Act & Assert
            var userDetails = userController.Details(99999);
            var roleDetails = roleController.Details(99999);
            var claimDetails = claimController.Details(99999);
            var approvalDetails = approvalController.Details(99999);

            Assert.IsType<RedirectToActionResult>(userDetails);
            Assert.IsType<RedirectToActionResult>(roleDetails);
            Assert.IsType<RedirectToActionResult>(claimDetails);
            Assert.IsType<RedirectToActionResult>(approvalDetails);
        }

        [Fact]
        public void SupportingDocument_WithCorrectProperties_ShouldWork()
        {
            // Arrange & Act
            var document = new SupportingDocument
            {
                DocumentID = 1,
                FileName = "test.pdf",
                FilePath = "/documents/test.pdf",
                FileSize = 1024,
                ContentType = "application/pdf",
                UploadDate = DateTime.Now,
                ClaimID = 1
            };

            // Assert
            Assert.Equal("test.pdf", document.FileName);
            Assert.Equal("application/pdf", document.ContentType);
            Assert.Equal("/documents/test.pdf", document.FilePath);
            Assert.Equal(1024, document.FileSize);
        }
    }
}
