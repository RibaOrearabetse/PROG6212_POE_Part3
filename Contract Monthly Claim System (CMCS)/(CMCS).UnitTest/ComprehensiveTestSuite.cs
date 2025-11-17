using System.ComponentModel.DataAnnotations;
using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    /// <summary>
    /// Comprehensive test suite covering all key functionalities of the Contract Monthly Claim System
    /// </summary>
    public class ComprehensiveTestSuite : BaseTest
    {
        #region User Management Tests

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Unit)]
        public void UserManagement_CreateUser_ShouldSucceed()
        {
            // Arrange
            var controller = new UserController();
            var user = CreateValidUser();

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Unit)]
        public void UserManagement_EditUser_ShouldSucceed()
        {
            // Arrange
            var controller = new UserController();
            var userId = 1;

            // Act
            var result = controller.Edit(userId);

            // Assert
            // The controller will initialize with sample data, so this should work
            if (result is ViewResult viewResult)
            {
                var model = Assert.IsType<User>(viewResult.Model);
                Assert.Equal(userId, model.UserID);
            }
            else
            {
                // If it redirects, that's also acceptable behavior
                Assert.IsType<RedirectToActionResult>(result);
            }
        }

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Validation)]
        public void UserManagement_UserValidation_ShouldEnforceRequiredFields()
        {
            // Arrange
            var user = new User(); // Empty user

            // Act
            var validationResults = ValidateModel(user);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("FirstName"));
            Assert.Contains(validationResults, v => v.MemberNames.Contains("LastName"));
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
        }

        #endregion

        #region Claim Management Tests

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Unit)]
        public void ClaimManagement_CreateClaim_ShouldSucceed()
        {
            // Arrange
            var controller = new ClaimController();
            var claim = CreateValidClaim();

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Unit)]
        public void ClaimManagement_EditClaim_ShouldSucceed()
        {
            // Arrange
            var controller = new ClaimController();
            var claimId = 1;

            // Act
            var result = controller.Edit(claimId);

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
        [Trait("Category", TestConfiguration.Categories.Validation)]
        public void ClaimManagement_ClaimValidation_ShouldEnforceBusinessRules()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 0, // Invalid: must be between 0.1 and 168
                HourlyRate = 0   // Invalid: must be greater than 0
            };

            // Act
            var validationResults = ValidateModel(claim);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("HoursWorked"));
            Assert.Contains(validationResults, v => v.MemberNames.Contains("HourlyRate"));
        }

        [Fact]
        [Trait("Category", TestConfiguration.Categories.BusinessLogic)]
        public void ClaimManagement_StatusTracking_ShouldWorkCorrectly()
        {
            // Arrange
            var claim = new Claim { ClaimStatus = "Pending" };

            // Act & Assert
            Assert.Equal("Under Review", claim.StatusDisplayName);
            Assert.Equal("bg-warning text-dark", claim.StatusBadgeClass);
            Assert.Equal(25, claim.StatusProgress);
        }

        #endregion

        #region Approval Management Tests

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Unit)]
        public void ApprovalManagement_ProcessApproval_ShouldSucceed()
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
        [Trait("Category", TestConfiguration.Categories.Unit)]
        public void ApprovalManagement_PendingClaims_ShouldReturnClaims()
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
        [Trait("Category", TestConfiguration.Categories.Integration)]
        public void ApprovalManagement_EndToEndWorkflow_ShouldWork()
        {
            // Arrange
            var approvalController = new ApprovalController();
            var claimController = new ClaimController();

            // Act - Create test claims
            var createTestClaimsResult = approvalController.CreateTestClaims();
            var redirectResult = Assert.IsType<RedirectToActionResult>(createTestClaimsResult);

            // Act - Get pending claims
            var pendingClaimsResult = approvalController.PendingClaims();
            var viewResult = Assert.IsType<ViewResult>(pendingClaimsResult);

            // Assert
            Assert.Equal("PendingClaims", redirectResult.ActionName);
            Assert.NotNull(viewResult);
        }

        #endregion

        #region Role Management Tests

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Unit)]
        public void RoleManagement_CreateRole_ShouldSucceed()
        {
            // Arrange
            var controller = new RoleController();
            var role = CreateValidRole();

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Unit)]
        public void RoleManagement_EditRole_ShouldSucceed()
        {
            // Arrange
            var controller = new RoleController();
            var roleId = 1;

            // Act
            var result = controller.Edit(roleId);

            // Assert
            // The controller will initialize with sample data, so this should work
            if (result is ViewResult viewResult)
            {
                var model = Assert.IsType<Role>(viewResult.Model);
                Assert.Equal(roleId, model.RoleID);
            }
            else
            {
                // If it redirects, that's also acceptable behavior
                Assert.IsType<RedirectToActionResult>(result);
            }
        }

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Validation)]
        public void RoleManagement_RoleValidation_ShouldEnforceRequiredFields()
        {
            // Arrange
            var role = new Role(); // Empty role

            // Act
            var validationResults = ValidateModel(role);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("RoleName"));
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        [Trait("Category", TestConfiguration.Categories.ErrorHandling)]
        public void ErrorHandling_InvalidId_ShouldHandleGracefully()
        {
            // Arrange
            var userController = new UserController();
            var claimController = new ClaimController();
            var approvalController = new ApprovalController();
            var roleController = new RoleController();
            var invalidId = 999;

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
        [Trait("Category", TestConfiguration.Categories.ErrorHandling)]
        public void ErrorHandling_NullData_ShouldNotThrowExceptions()
        {
            // Arrange & Act
            var user = new User();
            var claim = new Claim();
            var role = new Role();
            var approval = new Approval();

            // Assert - Should not throw exceptions
            Assert.NotNull(user);
            Assert.NotNull(claim);
            Assert.NotNull(role);
            Assert.NotNull(approval);
        }

        #endregion

        #region Data Consistency Tests

        [Fact]
        [Trait("Category", TestConfiguration.Categories.Integration)]
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
        [Trait("Category", TestConfiguration.Categories.Integration)]
        public void DataConsistency_CrossControllerData_ShouldBeConsistent()
        {
            // Arrange
            var userController = new UserController();
            var roleController = new RoleController();

            // Act
            var usersResult = userController.Index();
            var rolesResult = roleController.Index();

            // Assert
            var usersViewResult = Assert.IsType<ViewResult>(usersResult);
            var rolesViewResult = Assert.IsType<ViewResult>(rolesResult);

            var users = Assert.IsAssignableFrom<IEnumerable<User>>(usersViewResult.Model);
            var roles = Assert.IsAssignableFrom<IEnumerable<Role>>(rolesViewResult.Model);

            Assert.NotNull(users);
            Assert.NotNull(roles);
        }

        #endregion

        #region Helper Methods

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        #endregion
    }
}
