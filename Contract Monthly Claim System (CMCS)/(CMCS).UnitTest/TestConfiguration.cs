using Contract_Monthly_Claim_System__CMCS_.Models;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    /// <summary>
    /// Test configuration and setup for the Contract Monthly Claim System
    /// </summary>
    public class TestConfiguration
    {
        /// <summary>
        /// Test categories for organizing tests
        /// </summary>
        public static class Categories
        {
            public const string Unit = "Unit";
            public const string Integration = "Integration";
            public const string Validation = "Validation";
            public const string ErrorHandling = "ErrorHandling";
            public const string BusinessLogic = "BusinessLogic";
        }

        /// <summary>
        /// Test data constants
        /// </summary>
        public static class TestData
        {
            public const string ValidEmail = "test@example.com";
            public const string InvalidEmail = "invalid-email";
            public const string ValidFirstName = "John";
            public const string ValidLastName = "Doe";
            public const string ValidRoleName = "Test Role";
            public const decimal ValidHourlyRate = 25.00m;
            public const decimal ValidHoursWorked = 40.0m;
            public const decimal ValidTotalAmount = 1000.00m;
        }

        /// <summary>
        /// Test scenarios for comprehensive coverage
        /// </summary>
        public static class TestScenarios
        {
            public const string ValidUserCreation = "Valid user creation scenario";
            public const string InvalidUserCreation = "Invalid user creation scenario";
            public const string ValidClaimCreation = "Valid claim creation scenario";
            public const string InvalidClaimCreation = "Invalid claim creation scenario";
            public const string ValidApprovalProcess = "Valid approval process scenario";
            public const string InvalidApprovalProcess = "Invalid approval process scenario";
            public const string ValidRoleManagement = "Valid role management scenario";
            public const string InvalidRoleManagement = "Invalid role management scenario";
        }
    }

    /// <summary>
    /// Base test class with common test utilities
    /// </summary>
    public abstract class BaseTest
    {
        /// <summary>
        /// Creates a valid user for testing
        /// </summary>
        protected User CreateValidUser()
        {
            return new User
            {
                UserID = 1,
                FirstName = TestConfiguration.TestData.ValidFirstName,
                LastName = TestConfiguration.TestData.ValidLastName,
                Email = TestConfiguration.TestData.ValidEmail,
                ContactNumber = "123-456-7890",
                RoleID = 1
            };
        }

        /// <summary>
        /// Creates a valid claim for testing
        /// </summary>
        protected Claim CreateValidClaim()
        {
            return new Claim
            {
                ClaimID = 1,
                ClaimDate = DateTime.Now,
                HoursWorked = TestConfiguration.TestData.ValidHoursWorked,
                HourlyRate = TestConfiguration.TestData.ValidHourlyRate,
                TotalAmount = TestConfiguration.TestData.ValidTotalAmount,
                SubmissionDate = DateTime.Now,
                UserID = 1,
                ClaimStatus = "Pending"
            };
        }

        /// <summary>
        /// Creates a valid role for testing
        /// </summary>
        protected Role CreateValidRole()
        {
            return new Role
            {
                RoleID = 1,
                RoleName = TestConfiguration.TestData.ValidRoleName
            };
        }

        /// <summary>
        /// Creates a valid approval for testing
        /// </summary>
        protected Approval CreateValidApproval()
        {
            return new Approval
            {
                ApprovalID = 1,
                ClaimID = 1,
                ApprovalDate = DateTime.Now,
                Comments = "Test approval",
                ApproverID = 1
            };
        }
    }
}
