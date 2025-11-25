using System.Security.Claims;
using FirstWebApplication1.Controllers;
using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FirstWebApplication1.Tests.Controller
{
    public class ObstacleControllerTests
    {
        // Helper: in-memory ApplicationDbContext, unique per test
        private ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // Helper: create controller with fake HttpContext, TempData and User
        private ObstacleController CreateController(
            ApplicationDbContext context,
            bool isPilot = false,
            string userName = "testuser")
        {
            var controller = new ObstacleController(context);

            var httpContext = new DefaultHttpContext();

            // Fake user & roles
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName)
            };
            if (isPilot)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Pilot"));
            }

            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // TempData
            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            return controller;
        }

        // ============================================================
        // 1. SELECT TYPE (step 1 of wizard)
        // ============================================================

        [Fact]
        public void SelectType_Get_ReturnsViewWithNewViewModel()
        {
            // Arrange
            using var context = CreateDbContext();
            var controller = CreateController(context);

            // Act
            var result = controller.SelectType();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ObstacleTypeViewModel>(view.Model);
        }

        [Fact]
        public void SelectType_Post_ValidType_SetsTempDataAndRedirectsToDataForm()
        {
            // Arrange
            using var context = CreateDbContext();
            var controller = CreateController(context);
            var model = new ObstacleTypeViewModel { SelectedType = "PowerLine" };

            // Act
            var result = controller.SelectType(model);

            // Assert: should redirect to DataForm
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ObstacleController.DataForm), redirect.ActionName);

            // TempData should contain selected obstacle type
            Assert.Equal("PowerLine", controller.TempData["ObstacleType"] as string);
        }

        // ============================================================
        // 2. DATAFORM GET (step 2 of wizard)
        // ============================================================

        [Fact]
        public async Task DataForm_Get_NoObstacleTypeInTempData_RedirectsToSelectType()
        {
            // Arrange
            using var context = CreateDbContext();
            var controller = CreateController(context);

            // Act: user tries to go to DataForm directly, skipping step 1
            var result = await controller.DataForm();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ObstacleController.SelectType), redirect.ActionName);
        }

        [Fact]
        public async Task DataForm_Get_WithObstacleType_ReturnsViewWithPrepopulatedModel()
        {
            // Arrange
            using var context = CreateDbContext();
            var controller = CreateController(context, isPilot: true);

            // Simulate that step 1 has already stored the obstacle type
            controller.TempData["ObstacleType"] = "Tower";

            // Act
            var result = await controller.DataForm();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ObstacleData>(view.Model);

            // Model should have type from TempData and default height 15
            Assert.Equal("Tower", model.ObstacleType);
            Assert.Equal(15, model.ObstacleHeight);

            // Pilot flags should be set based on the fake user role
            Assert.True((bool)controller.ViewBag.IsPilot);
            Assert.True((bool)controller.ViewBag.UsesFeet);
        }

        // ============================================================
        // 3. DATAFORM POST (step 3 – submit)
        // ============================================================

        [Fact]
        public async Task DataForm_Post_InvalidModel_RedisplaysFormWithSameModel()
        {
            // Arrange
            using var context = CreateDbContext();
            var controller = CreateController(context, isPilot: false);

            // Step 1 value
            controller.TempData["ObstacleType"] = "Tower";

            // Model missing required fields (e.g. Latitude/Longitude)
            var model = new ObstacleData
            {
                ObstacleHeight = 20,
                Latitude = null,
                Longitude = null
            };

            // Force invalid model state
            controller.ModelState.AddModelError("Latitude", "Required");

            // Act
            var result = await controller.DataForm(model, useFeet: null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.False(controller.ModelState.IsValid);

            // ViewBag flags for non-pilot user
            Assert.False((bool)controller.ViewBag.IsPilot);
            Assert.False((bool)controller.ViewBag.UsesFeet);
        }

        [Fact]
        public async Task DataForm_Post_ValidModel_SavesPendingObstacleAndReturnsOverviewView()
        {
            // Arrange
            using var context = CreateDbContext();
            var controller = CreateController(context, isPilot: true, userName: "pilotUser");

            // Step 1 type stored in TempData
            controller.TempData["ObstacleType"] = "PowerLine";

            var model = new ObstacleData
            {
                ObstacleHeight = 50,
                Latitude = 58.1,
                Longitude = 7.2,
                LineGeoJson = null,
                ObstacleDescription = null // controller normalizes this
            };

            // Act
            var result = await controller.DataForm(model, useFeet: null);

            // Assert: controller should send user to "Overview" view
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Overview", view.ViewName);

            var returned = Assert.IsType<ObstacleData>(view.Model);

            // Type and name derived correctly
            Assert.Equal("PowerLine", returned.ObstacleType);
            Assert.Equal("PowerLine", returned.ObstacleName);

            // Status and metadata set
            Assert.Equal("Pending", returned.Status);
            Assert.Equal("pilotUser", returned.SubmittedBy);
            Assert.NotEqual(default, returned.SubmittedDate);

            // Obstacle is actually persisted to DB
            var stored = await context.Obstacles.FirstOrDefaultAsync(o => o.Id == returned.Id);
            Assert.NotNull(stored);
            Assert.Equal("Pending", stored.Status);
        }

        // ============================================================
        // 4. APPROVE (status change)
        // ============================================================

        [Fact]
        public async Task Approve_ExistingObstacle_ChangesStatusAndRedirectsToList()
        {
            // Arrange
            using var context = CreateDbContext();

            // Add required fields so EF Core accepts the entity
            var obstacle = new ObstacleData
            {
                ObstacleName = "To Approve",
                Status = "Pending",
                Latitude = 58.0,              // REQUIRED
                Longitude = 7.0,              // REQUIRED
                ObstacleDescription = "Test"  // REQUIRED
            };

            context.Obstacles.Add(obstacle);
            await context.SaveChangesAsync();

            var controller = CreateController(context, userName: "caseworker");

            // Act
            var result = await controller.Approve(obstacle.Id);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ObstacleController.List), redirect.ActionName);
            Assert.Equal("Pending", redirect.RouteValues["statusFilter"]);

            var stored = await context.Obstacles.FindAsync(obstacle.Id);
            Assert.Equal("Approved", stored.Status);
            Assert.Equal("caseworker", stored.ApprovedBy);
            Assert.NotEqual(default, stored.ApprovedDate);
        }

        // ============================================================
        // 5. DELETE
        // ============================================================

        [Fact]
        public async Task Delete_ExistingObstacle_RemovesItAndRedirectsToList()
        {
            // Arrange
            using var context = CreateDbContext();

            var obstacle = new ObstacleData {
                ObstacleName = "To Delete",
                Status = "Pending",         
                Latitude = 58.0,             
                Longitude = 7.0,             
                ObstacleDescription = "Test" 
            };
            context.Obstacles.Add(obstacle);
            await context.SaveChangesAsync();

            var controller = CreateController(context, userName: "admin");

            // Act
            var result = await controller.Delete(obstacle.Id);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ObstacleController.List), redirect.ActionName);

            // The entity should be gone from the database
            var stored = await context.Obstacles.FindAsync(obstacle.Id);
            Assert.Null(stored);
        }
    }
}