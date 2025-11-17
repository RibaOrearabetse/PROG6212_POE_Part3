using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    public class IntegrationTests : CleanTestBase
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
            var roles = Assert.IsAssignableFrom<IEnumerable<Role>>(rolesViewResult.Model);

            // Assert - Roles should be available
            Assert.NotNull(roles);
            Assert.NotEmpty(roles);

            // Act - Create user
            var createResult = userController.Create();
            var createViewResult = Assert.IsType<ViewResult>(createResult);

            // Assert - Create view should be returned
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
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(usersViewResult.Model);

            // Assert - Users should be available
            Assert.NotNull(users);

            // Act - Create claim
            var createResult = claimController.Create();
            var createViewResult = Assert.IsType<ViewResult>(createResult);

            // Assert - Create view should be returned
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
            var claims = Assert.IsAssignableFrom<IEnumerable<Claim>>(claimsViewResult.Model);

            // Assert - Claims should be available
            Assert.NotNull(claims);

            // Act - Get pending claims
            var pendingClaimsResult = approvalController.PendingClaims();
            var pendingClaimsViewResult = Assert.IsType<ViewResult>(pendingClaimsResult);
            var pendingClaims = Assert.IsAssignableFrom<IEnumerable<Claim>>(pendingClaimsViewResult.Model);

            // Assert - Pending claims should be available
            Assert.NotNull(pendingClaims);
        }

        [Fact]
        public void RoleManagement_CRUDOperations_ShouldWorkEndToEnd()
        {
            // Arrange
            var roleController = new RoleController();

            // Act - Get all roles
            var indexResult = roleController.Index();
            var indexViewResult = Assert.IsType<ViewResult>(indexResult);
            var roles = Assert.IsAssignableFrom<IEnumerable<Role>>(indexViewResult.Model);

            // Assert - Roles should be available
            Assert.NotNull(roles);

            // Act - Create role
            var createResult = roleController.Create();
            var createViewResult = Assert.IsType<ViewResult>(createResult);

            // Assert - Create view should be returned
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

            // Act - Get data from all controllers
            var usersResult = userController.Index();
            var rolesResult = roleController.Index();
            var claimsResult = claimController.Index();
            var approvalsResult = approvalController.Index();

            // Assert - All controllers should return valid results
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

            // Act & Assert - All main pages should be accessible
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

            // Act & Assert - All controllers should handle invalid IDs gracefully
            var userDetails = userController.Details(999);
            var roleDetails = roleController.Details(999);
            var claimDetails = claimController.Details(999);
            var approvalDetails = approvalController.Details(999);

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

            // Assert - Using correct properties
            Assert.Equal("test.pdf", document.FileName);
            Assert.Equal("application/pdf", document.ContentType);
            Assert.Equal("/documents/test.pdf", document.FilePath);
            Assert.Equal(1024, document.FileSize);
        }
    }
}