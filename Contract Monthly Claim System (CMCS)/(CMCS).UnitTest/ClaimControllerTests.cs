using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Contract_Monthly_Claim_System__CMCS_.Models;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    public class ClaimControllerTests
    {
        [Fact]
        public void Index_ReturnsViewWithClaims()
        {
            // Arrange
            var controller = new ClaimController();

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
            var controller = new ClaimController();

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
            var controller = new ClaimController();
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
            var controller = new ClaimController();
            var invalidId = 99999;

            // Act
            var result = controller.Edit(invalidId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void GetAllClaims_ReturnsList()
        {
            // Act
            var result = ClaimController.GetAllClaims();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<Claim>>(result);
        }
    }
}
