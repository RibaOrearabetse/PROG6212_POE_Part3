using Contract_Monthly_Claim_System__CMCS_.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    public class ModelValidationTests : CleanTestBase
    {
        [Fact]
        public void User_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                ContactNumber = "123-456-7890",
                RoleID = 1
            };

            // Act
            var validationResults = ValidateModel(user);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void User_WithEmptyFirstName_ShouldFailValidation()
        {
            // Arrange
            var user = new User
            {
                FirstName = "",
                LastName = "Doe",
                Email = "john.doe@example.com",
                RoleID = 1
            };

            // Act
            var validationResults = ValidateModel(user);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void User_WithInvalidEmail_ShouldFailValidation()
        {
            // Arrange
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "invalid-email",
                RoleID = 1
            };

            // Act
            var validationResults = ValidateModel(user);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
        }

        [Fact]
        public void Claim_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimDate = DateTime.Now,
                HoursWorked = 40,
                HourlyRate = 25.00m,
                TotalAmount = 1000.00m,
                SubmissionDate = DateTime.Now,
                UserID = 1
            };

            // Act
            var validationResults = ValidateModel(claim);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Claim_WithInvalidHoursWorked_ShouldFailValidation()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimDate = DateTime.Now,
                HoursWorked = 0, // Invalid: must be between 0.1 and 168
                HourlyRate = 25.00m,
                TotalAmount = 1000.00m,
                SubmissionDate = DateTime.Now,
                UserID = 1
            };

            // Act
            var validationResults = ValidateModel(claim);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("HoursWorked"));
        }

        [Fact]
        public void Claim_WithInvalidHourlyRate_ShouldFailValidation()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimDate = DateTime.Now,
                HoursWorked = 40,
                HourlyRate = 0, // Invalid: must be greater than 0
                TotalAmount = 1000.00m,
                SubmissionDate = DateTime.Now,
                UserID = 1
            };

            // Act
            var validationResults = ValidateModel(claim);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("HourlyRate"));
        }

        [Fact]
        public void Role_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var role = new Role
            {
                RoleName = "Test Role"
            };

            // Act
            var validationResults = ValidateModel(role);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Role_WithEmptyName_ShouldFailValidation()
        {
            // Arrange
            var role = new Role
            {
                RoleName = ""
            };

            // Act
            var validationResults = ValidateModel(role);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("RoleName"));
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
