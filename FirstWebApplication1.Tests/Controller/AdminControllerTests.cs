using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FirstWebApplication1.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace FirstWebApplication1.Tests.Controllers
{
    public class AdminControllerTests
    {
        // ------------------ Helpers ------------------

        private Mock<UserManager<IdentityUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object, null, null, null, null);
        }

        private AdminController CreateController(
            Mock<UserManager<IdentityUser>>? userManagerMock = null,
            Mock<RoleManager<IdentityRole>>? roleManagerMock = null,
            ClaimsPrincipal? user = null)
        {
            var userManager = userManagerMock ?? CreateUserManagerMock();
            var roleManager = roleManagerMock ?? CreateRoleManagerMock();

            var controller = new AdminController(userManager.Object, roleManager.Object);

            var httpContext = new DefaultHttpContext();

            if (user != null)
                httpContext.User = user;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Add TempData
            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            return controller;
        }

        // ------------------ Test 1 ------------------
        // EditUser (POST): Returns NotFound if user doesn't exist
        [Fact]
        public async Task EditUser_UserNotFound_ReturnsNotFound()
        {
            var userManagerMock = CreateUserManagerMock();
            userManagerMock
                .Setup(um => um.FindByIdAsync("123"))
                .ReturnsAsync((IdentityUser?)null);

            var controller = CreateController(userManagerMock);

            var result = await controller.EditUser("123", "Pilot");

            Assert.IsType<NotFoundResult>(result);
        }

        // ------------------ Test 2 ------------------
        // EditUser (POST): Updates roles and redirects to Users
        [Fact]
        public async Task EditUser_UpdatesRolesAndRedirects()
        {
            var user = new IdentityUser { Id = "123", Email = "test@example.com" };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(um => um.FindByIdAsync("123")).ReturnsAsync(user);

            var oldRoles = new List<string> { "Old1", "Old2" };
            userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(oldRoles);
            userManagerMock.Setup(um => um.RemoveFromRolesAsync(user, oldRoles))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(um => um.AddToRoleAsync(user, "Pilot"))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            var result = await controller.EditUser("123", "Pilot");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminController.Users), redirect.ActionName);
            Assert.Equal("User roles updated successfully.", controller.TempData["SuccessMessage"]);

            userManagerMock.Verify(um => um.RemoveFromRolesAsync(user, oldRoles), Times.Once);
            userManagerMock.Verify(um => um.AddToRoleAsync(user, "Pilot"), Times.Once);
        }

        // ------------------ Test 3 ------------------
        // DeleteUser: Returns NotFound if target user doesn't exist
        [Fact]
        public async Task DeleteUser_UserNotFound_ReturnsNotFound()
        {
            var userManagerMock = CreateUserManagerMock();
            userManagerMock
                .Setup(um => um.FindByIdAsync("999"))
                .ReturnsAsync((IdentityUser?)null);

            var controller = CreateController(userManagerMock);

            var result = await controller.DeleteUser("999");

            Assert.IsType<NotFoundResult>(result);
        }
        // ------------------ Test 4 ------------------
        //Admin user cannot delete themselves
        [Fact]
        public async Task DeleteUser_CannotDeleteSelf_RedirectsAndDoesNotDelete()
        {
            // Arrange
            var targetUser = new IdentityUser { Id = "123", Email = "admin@example.com" };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock
                .Setup(um => um.FindByIdAsync("123"))
                .ReturnsAsync(targetUser);

            // Simuler innlogget bruker med samme Id
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "123")
                }, "TestAuth")
            );

            var controller = CreateController(userManagerMock, user: principal);

            // Act
            var result = await controller.DeleteUser("123");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminController.Users), redirect.ActionName);

            // Viktig: brukeren skal ikke bli slettet
            userManagerMock.Verify(um => um.DeleteAsync(It.IsAny<IdentityUser>()), Times.Never);
        }
        // ------------------ Test 5 ------------------
        // DeleteUser: Successfully deletes another user
        [Fact]
        public async Task DeleteUser_DeletesOtherUserAndSetsSuccessMessage()
        {
            // Arrange
            var targetUser = new IdentityUser { Id = "999" };
            var currentUser = new IdentityUser { Id = "123" };

            var userManagerMock = CreateUserManagerMock();

            // The user we want to delete
            userManagerMock
                .Setup(um => um.FindByIdAsync("999"))
                .ReturnsAsync(targetUser);

            // The logged-in user
            userManagerMock
                .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("123");

            // GetUserAsync(User) = calls FindByIdAsync("123")
            userManagerMock
                .Setup(um => um.FindByIdAsync("123"))
                .ReturnsAsync(currentUser);

            // Delete works
            userManagerMock
                .Setup(um => um.DeleteAsync(targetUser))
                .ReturnsAsync(IdentityResult.Success);

            // Simulate logged-in user with id=123
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "123")
                }, "TestAuth"));

            var controller = CreateController(userManagerMock, user: principal);

            // Act
            var result = await controller.DeleteUser("999");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminController.Users), redirect.ActionName);

            Assert.Equal("User deleted successfully.", controller.TempData["SuccessMessage"]);

            userManagerMock.Verify(um => um.DeleteAsync(targetUser), Times.Once);
        }
    }
}