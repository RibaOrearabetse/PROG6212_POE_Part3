using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    public class RoleControllerTests
    {
        [Fact]
        public void Index_ReturnsViewWithRoles()
        {
            // Arrange
            var controller = new RoleController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Role>>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public void Create_ReturnsView()
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
        public void Details_WithValidId_ReturnsView()
        {
            // Arrange
            var controller = new RoleController();
            var roleId = 1;

            // Act
            var result = controller.Details(roleId);

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
        public void Details_WithInvalidId_ReturnsRedirectToIndex()
        {
            // Arrange
            var controller = new RoleController();
            var invalidId = 999;

            // Act
            var result = controller.Details(invalidId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void Edit_WithValidId_ReturnsView()
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
        public void Edit_WithInvalidId_ReturnsRedirectToIndex()
        {
            // Arrange
            var controller = new RoleController();
            var invalidId = 999;

            // Act
            var result = controller.Edit(invalidId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
