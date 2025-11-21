using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    public class UserControllerTests
    {
        [Fact]
        public void Index_ReturnsViewWithUsers()
        {
            // Arrange
            var controller = new UserController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public void Create_ReturnsView()
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
        public void Details_WithInvalidId_ReturnsRedirect()
        {
            // Arrange
            var controller = new UserController();
            var invalidId = 99999;

            // Act
            var result = controller.Details(invalidId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void Edit_WithInvalidId_ReturnsRedirect()
        {
            // Arrange
            var controller = new UserController();
            var invalidId = 99999;

            // Act
            var result = controller.Edit(invalidId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void GetAllUsers_ReturnsList()
        {
            // Act
            var result = UserController.GetAllUsers();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<User>>(result);
        }
    }
}
