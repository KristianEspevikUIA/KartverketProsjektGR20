using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FirstWebApplication1.Controllers;
using FirstWebApplication1.Models;
using FirstWebApplication1.Models.Admin;
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
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            // Setup default behaviors
            userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new IdentityUser { Id = id, Email = $"{id}@test.com" });

            userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(new List<string>());

            userManagerMock.Setup(um => um.GetClaimsAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(new List<Claim>());

            return userManagerMock;
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
                .Setup(um => um.FindByIdAsync("non-existent-id"))
                .ReturnsAsync((IdentityUser?)null);

            var controller = CreateController(userManagerMock);

            var result = await controller.EditUser("non-existent-id", "Pilot", "Org");

            Assert.IsType<NotFoundResult>(result);
        }

        // ------------------ Test 2 ------------------
        // EditUser (POST): Updates role and adds organization claim
        [Fact]
        public async Task EditUser_UpdatesRoleAndAddsOrganizationClaim_WhenNoneExisted()
        {
            var user = new IdentityUser { Id = "123", Email = "test@example.com" };
            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(um => um.FindByIdAsync("123")).ReturnsAsync(user);

            // User has no existing roles or claims
            userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());

            // Act
            var controller = CreateController(userManagerMock);
            var result = await controller.EditUser("123", "Pilot", "Luftforsvaret");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminController.Users), redirect.ActionName);
            Assert.Equal("User updated successfully.", controller.TempData["SuccessMessage"]);

            // Verify role was added
            userManagerMock.Verify(um => um.AddToRoleAsync(user, "Pilot"), Times.Once);

            // Verify claim was added
            userManagerMock.Verify(um => um.AddClaimAsync(user, It.Is<Claim>(c => c.Type == "Organization" && c.Value == "Luftforsvaret")), Times.Once);
        }

        // ------------------ Test 3 ------------------
        // EditUser (POST): Changes existing organization claim
        [Fact]
        public async Task EditUser_ChangesExistingOrganizationClaim()
        {
            var user = new IdentityUser { Id = "123", Email = "test@example.com" };
            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(um => um.FindByIdAsync("123")).ReturnsAsync(user);

            // User has an existing claim
            var existingClaim = new Claim("Organization", "Old-Org");
            userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync(new List<Claim> { existingClaim });

            // Act
            var controller = CreateController(userManagerMock);
            await controller.EditUser("123", "Pilot", "New-Org");

            // Assert
            // Verify old claim was removed
            userManagerMock.Verify(um => um.RemoveClaimAsync(user, It.Is<Claim>(c => c.Type == "Organization" && c.Value == "Old-Org")), Times.Once);

            // Verify new claim was added
            userManagerMock.Verify(um => um.AddClaimAsync(user, It.Is<Claim>(c => c.Type == "Organization" && c.Value == "New-Org")), Times.Once);
        }
        
        // ------------------ Test 4 ------------------
        //Admin user cannot delete themselves
        [Fact]
        public async Task DeleteUser_CannotDeleteSelf_RedirectsAndDoesNotDelete()
        {
            var userManagerMock = CreateUserManagerMock();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "123") }));

            var controller = CreateController(userManagerMock, user: principal);

            // Act
            var result = await controller.DeleteUser("123");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminController.Users), redirect.ActionName);
            Assert.Equal("You cannot delete your own account.", controller.TempData["ErrorMessage"]);

            userManagerMock.Verify(um => um.DeleteAsync(It.IsAny<IdentityUser>()), Times.Never);
        }
    }
}