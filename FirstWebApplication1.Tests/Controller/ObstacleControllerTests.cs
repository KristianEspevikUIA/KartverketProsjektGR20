using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FirstWebApplication1.Controllers;
using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FirstWebApplication1.Tests.Controller
{
    public class ObstacleControllerTests
    {
        // ================== HELPER METHODS ==================

        private ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private Mock<UserManager<IdentityUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private ObstacleController CreateController(
            ApplicationDbContext context,
            string? organization = null,
            string userName = "testuser",
            string role = "Pilot")
        {
            var userManagerMock = CreateUserManagerMock();
            var controller = new ObstacleController(context, userManagerMock.Object);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            };
            if (organization != null)
            {
                claims.Add(new Claim("Organization", organization));
            }

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            };
            
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, new Mock<ITempDataProvider>().Object);

            return controller;
        }

        // ================== ORIGINAL TESTS (RESTORED & VERIFIED) ==================

        [Fact]
        public void SelectType_Get_ReturnsViewWithNewViewModel()
        {
            using var context = CreateDbContext();
            var controller = CreateController(context);
            var result = controller.SelectType();
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ObstacleTypeViewModel>(view.Model);
        }

        [Fact]
        public void SelectType_Post_ValidType_SetsTempDataAndRedirectsToDataForm()
        {
            using var context = CreateDbContext();
            var controller = CreateController(context);
            var model = new ObstacleTypeViewModel { SelectedType = "Tower" };
            var result = controller.SelectType(model);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ObstacleController.DataForm), redirect.ActionName);
            Assert.Equal("Tower", controller.TempData["ObstacleType"]);
        }

        [Fact]
        public async Task DataForm_Get_NoObstacleTypeInTempData_RedirectsToSelectType()
        {
            using var context = CreateDbContext();
            var controller = CreateController(context);
            var result = await controller.DataForm();
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ObstacleController.SelectType), redirect.ActionName);
        }

        [Fact]
        public async Task DataForm_Post_InvalidModel_RedisplaysFormWithSameModel()
        {
            using var context = CreateDbContext();
            var controller = CreateController(context);
            controller.TempData["ObstacleType"] = "Tower";
            var model = new ObstacleData { ObstacleHeight = 20, Latitude = null, Longitude = null };
            controller.ModelState.AddModelError("Latitude", "Required");

            var result = await controller.DataForm(model, useFeet: null);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Approve_ExistingObstacle_ChangesStatusAndRedirectsToList()
        {
            using var context = CreateDbContext();
            var obstacle = new ObstacleData { Id = 1, ObstacleName = "To Approve", Status = "Pending", Latitude = 58.0, Longitude = 7.0, ObstacleDescription = "Test" };
            context.Obstacles.Add(obstacle);
            await context.SaveChangesAsync();
            var controller = CreateController(context, userName: "caseworker");

            var result = await controller.Approve(obstacle.Id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ObstacleController.List), redirect.ActionName);
            var stored = await context.Obstacles.FindAsync(obstacle.Id);
            Assert.Equal("Approved", stored.Status);
            Assert.Equal("caseworker", stored.ApprovedBy);
        }

        [Fact]
        public async Task Delete_ExistingObstacle_RemovesItAndRedirectsToList()
        {
            using var context = CreateDbContext();
            var obstacle = new ObstacleData { Id = 1, ObstacleName = "To Delete", Status = "Pending", Latitude = 58.0, Longitude = 7.0, ObstacleDescription = "Test" };
            context.Obstacles.Add(obstacle);
            await context.SaveChangesAsync();
            var controller = CreateController(context, userName: "admin", role: "Admin");

            var result = await controller.Delete(obstacle.Id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ObstacleController.List), redirect.ActionName);
            var stored = await context.Obstacles.FindAsync(obstacle.Id);
            Assert.Null(stored);
        }

        // ================== NEW & IMPROVED TESTS ==================

        [Fact]
        public async Task DataForm_Post_SavesObstacleWithUserOrganization()
        {
            // Arrange
            using var context = CreateDbContext();
            var controller = CreateController(context, organization: "Luftforsvaret");
            controller.TempData["ObstacleType"] = "Tower";
            var model = new ObstacleData { ObstacleHeight = 100, Latitude = 60.0, Longitude = 10.0, ObstacleDescription = "Test Tower" };

            // Act
            var result = await controller.DataForm(model, useFeet: false);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Overview", redirectResult.ActionName);
            var savedObstacle = Assert.Single(context.Obstacles);
            Assert.Equal("Luftforsvaret", savedObstacle.Organization);
            Assert.Equal("Pending", savedObstacle.Status);
        }

        [Fact]
        public async Task List_FiltersByMinAndMaxHeight()
        {
            // Arrange
            using var context = CreateDbContext();
            context.Obstacles.AddRange(
                new ObstacleData { ObstacleName = "Short", ObstacleHeight = 20, Latitude = 1, Longitude = 1, ObstacleDescription = "d" },
                new ObstacleData { ObstacleName = "Medium", ObstacleHeight = 100, Latitude = 1, Longitude = 1, ObstacleDescription = "d" },
                new ObstacleData { ObstacleName = "Tall", ObstacleHeight = 250, Latitude = 1, Longitude = 1, ObstacleDescription = "d" }
            );
            await context.SaveChangesAsync();
            var controller = CreateController(context);

            // Act: Filter for obstacles between 50 and 150 meters
            var result = await controller.List(minHeight: 50, maxHeight: 150);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ObstacleListViewModel>(viewResult.Model);
            var obstacle = Assert.Single(model.Obstacles);
            Assert.Equal("Medium", obstacle.ObstacleName);
        }

        [Fact]
        public async Task List_FiltersByObstacleType()
        {
            // Arrange
            using var context = CreateDbContext();
            context.Obstacles.AddRange(
                new ObstacleData { ObstacleName = "C1", ObstacleType = "Crane", ObstacleHeight = 50, Latitude = 1, Longitude = 1, ObstacleDescription = "d" },
                new ObstacleData { ObstacleName = "T1", ObstacleType = "Tower", ObstacleHeight = 150, Latitude = 1, Longitude = 1, ObstacleDescription = "d" },
                new ObstacleData { ObstacleName = "C2", ObstacleType = "Crane", ObstacleHeight = 80, Latitude = 1, Longitude = 1, ObstacleDescription = "d" }
            );
            await context.SaveChangesAsync();
            var controller = CreateController(context);

            // Act: Filter for "Crane"
            var result = await controller.List(obstacleTypeFilter: "Crane");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ObstacleListViewModel>(viewResult.Model);
            Assert.Equal(2, model.Obstacles.Count());
            Assert.All(model.Obstacles, o => Assert.Equal("Crane", o.ObstacleType));
        }

        [Fact]
        public async Task List_FiltersByOrganization()
        {
            // Arrange
            using var context = CreateDbContext();
            context.Obstacles.AddRange(
                new ObstacleData { ObstacleName = "Airforce One", Organization = "Luftforsvaret", ObstacleHeight = 1, Latitude = 1, Longitude = 1, ObstacleDescription = "d" },
                new ObstacleData { ObstacleName = "Police One", Organization = "Politiets helikoptertjeneste", ObstacleHeight = 1, Latitude = 1, Longitude = 1, ObstacleDescription = "d" },
                new ObstacleData { ObstacleName = "Airforce Two", Organization = "Luftforsvaret", ObstacleHeight = 1, Latitude = 1, Longitude = 1, ObstacleDescription = "d" }
            );
            await context.SaveChangesAsync();
            var controller = CreateController(context);

            // Act: Filter for "Luftforsvaret"
            var result = await controller.List(organizationFilter: "Luftforsvaret");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ObstacleListViewModel>(viewResult.Model);
            Assert.Equal(2, model.Obstacles.Count());
            Assert.All(model.Obstacles, o => Assert.Equal("Luftforsvaret", o.Organization));
        }
    }
}