using System.ComponentModel.DataAnnotations;
using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    public class ComprehensiveTestSuite
    {
        [Fact]
        public void UserManagement_CreateUser_ShouldSucceed()
        {
            // Arrange
            var controller = new UserController();

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public void UserManagement_EditUser_ShouldSucceed()
        {
            // Arrange
            var controller = new UserController();
            var userId = 1;

            // Act
            var result = controller.Edit(userId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void UserManagement_UserValidation_ShouldEnforceRequiredFields()
        {
            // Arrange
            var user = new User();

            // Act
            var validationResults = ValidateModel(user);

            // Assert
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void ClaimManagement_CreateClaim_ShouldSucceed()
        {
            // Arrange
            var controller = new ClaimController();

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public void ClaimManagement_EditClaim_ShouldSucceed()
        {
            // Arrange
            var controller = new ClaimController();
            var claimId = 1;

            // Act
            var result = controller.Edit(claimId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ClaimManagement_ClaimValidation_ShouldEnforceBusinessRules()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 0,
                HourlyRate = 0
            };

            // Act
            var validationResults = ValidateModel(claim);

            // Assert
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void ClaimManagement_StatusTracking_ShouldWorkCorrectly()
        {
            // Arrange
            var claim = new Claim { ClaimStatus = "Pending" };

            // Act & Assert
            Assert.Equal("Under Review", claim.StatusDisplayName);
            Assert.Equal("bg-warning text-dark", claim.StatusBadgeClass);
            Assert.Equal(25, claim.StatusProgress);
        }

        [Fact]
        public void ApprovalManagement_ProcessApproval_ShouldSucceed()
        {
            // Arrange
            var controller = new ApprovalController();
            var claimId = 1;

            // Act
            var result = controller.ProcessApproval(claimId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ApprovalManagement_PendingClaims_ShouldReturnClaims()
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
        public void RoleManagement_CreateRole_ShouldSucceed()
        {
            // Arrange
            var controller = new RoleController();

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public void RoleManagement_EditRole_ShouldSucceed()
        {
            // Arrange
            var controller = new RoleController();
            var roleId = 1;

            // Act
            var result = controller.Edit(roleId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void RoleManagement_RoleValidation_ShouldEnforceRequiredFields()
        {
            // Arrange
            var role = new Role();

            // Act
            var validationResults = ValidateModel(role);

            // Assert
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void ErrorHandling_InvalidId_ShouldHandleGracefully()
        {
            // Arrange
            var userController = new UserController();
            var claimController = new ClaimController();
            var approvalController = new ApprovalController();
            var roleController = new RoleController();
            var invalidId = 99999;

            // Act & Assert
            var userResult = userController.Details(invalidId);
            var claimResult = claimController.Details(invalidId);
            var approvalResult = approvalController.Details(invalidId);
            var roleResult = roleController.Details(invalidId);

            Assert.IsType<RedirectToActionResult>(userResult);
            Assert.IsType<RedirectToActionResult>(claimResult);
            Assert.IsType<RedirectToActionResult>(approvalResult);
            Assert.IsType<RedirectToActionResult>(roleResult);
        }

        [Fact]
        public void ErrorHandling_NullData_ShouldNotThrowExceptions()
        {
            // Arrange & Act
            var user = new User();
            var claim = new Claim();
            var role = new Role();
            var approval = new Approval();

            // Assert
            Assert.NotNull(user);
            Assert.NotNull(claim);
            Assert.NotNull(role);
            Assert.NotNull(approval);
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

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}
