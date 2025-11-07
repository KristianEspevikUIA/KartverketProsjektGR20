using FirstWebApplication1.Controllers;
using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FirstWebApplication1.Tests.Controllers
{
    public class ObstacleControllerTests
    {
        private ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // This test verifies that when the user visits the DataForm page, the controller returns a ViewResult with no model.

        [Fact]
        public void DataForm_Get_ReturnsViewResultWithoutModel()
        {
            // Arrange: Create an instance of the controller to test
            using var context = CreateInMemoryContext();
            var controller = new ObstacleController(context);

            // Act: Call the GET method
            var result = controller.DataForm();

            // Assert: Verify that the result is a ViewResult
            var viewResult = Assert.IsType<ViewResult>(result);

            // The model should be null because no data is passed initially
            Assert.Null(viewResult.Model);

            // The ViewName should be null, meaning it uses the default view ("DataForm")
            Assert.Null(viewResult.ViewName);
        }
        
        // This test simulates what happens when a user submits the form with invalid data (for example, missing required fields).
        [Fact]
        public async Task DataForm_Post_InvalidModelState_ReturnsSameViewWithModel()
        {
            // Arrange: Create controller and a dummy ObstacleData object
            using var context = CreateInMemoryContext();
            var controller = new ObstacleController(context);
            var obstacle = new ObstacleData
            {
                // You can optionally add fake data here
            };

            // Simulate validation failure by adding a fake model error
            controller.ModelState.AddModelError("TestError", "Some validation error");

            // Act: Call the POST method with invalid model
            var result = await controller.DataForm(obstacle);

            // Assert: Ensure the controller returns a ViewResult (not a Redirect)
            var viewResult = Assert.IsType<ViewResult>(result);

            // When ModelState is invalid, the controller calls:
            //     return View(obstacledata);
            // which uses the default view name ("DataForm")
            Assert.Null(viewResult.ViewName);

            // The model passed back to the view should be the same object we sent in,
            // so the user's form input is preserved
            Assert.Same(obstacle, viewResult.Model);
        }

        // -------------------------------------------------------------
        // TEST 3: POST request -> DataForm(obstacledata) with VALID model
        // -------------------------------------------------------------
        // This test checks that when valid data is submitted,
        // the controller redirects the user to the "Overview" view
        // and passes along the valid model.
        // This corresponds to the controller code:
        //     return View("Overview", obstacledata);
        [Fact]
        public async Task DataForm_Post_ValidModelState_ReturnsOverviewViewWithModel()
        {
            // Arrange: Create controller and a valid model
            using var context = CreateInMemoryContext();
            var controller = new ObstacleController(context);
            
            // Set up a fake user identity
            var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new System.Security.Claims.Claim[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "testuser")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var obstacle = new ObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 100,
                ObstacleDescription = "Test Description",
                Latitude = 58.0,
                Longitude = 7.0
            };

            // Act: Call the POST method (ModelState is valid by default)
            var result = await controller.DataForm(obstacle);

            // Assert: Verify that we got a ViewResult
            var viewResult = Assert.IsType<ViewResult>(result);

            // The controller should render the "Overview" view explicitly
            Assert.Equal("Overview", viewResult.ViewName);

            // The model passed to the Overview view should be the same object
            Assert.Same(obstacle, viewResult.Model);
        }
    }
}
