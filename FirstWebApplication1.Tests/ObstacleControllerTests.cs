using FirstWebApplication1.Controllers;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication1.Tests.Controllers
{
    public class ObstacleControllerTests
    {
     
        // This test verifies that when the user visits the DataForm page, the controller returns a ViewResult with no model.

        [Fact]
        public void DataForm_Get_ReturnsViewResultWithoutModel()
        {
            // Arrange: Create an instance of the controller to test
            var controller = new ObstacleController();

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
        public void DataForm_Post_InvalidModelState_ReturnsSameViewWithModel()
        {
            // Arrange: Create controller and a dummy ObstacleData object
            var controller = new ObstacleController();
            var obstacle = new ObstacleData
            {
                // You can optionally add fake data here
            };

            // Simulate validation failure by adding a fake model error
            controller.ModelState.AddModelError("TestError", "Some validation error");

            // Act: Call the POST method with invalid model
            var result = controller.DataForm(obstacle);

            // Assert: Ensure the controller returns a ViewResult (not a Redirect)
            var viewResult = Assert.IsType<ViewResult>(result);

            // When ModelState is invalid, the controller calls:
            //     return View(obstacledata);
            // which uses the default view name ("DataForm")
            Assert.Null(viewResult.ViewName);

            // The model passed back to the view should be the same object we sent in,
            // so the user’s form input is preserved
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
        public void DataForm_Post_ValidModelState_ReturnsOverviewViewWithModel()
        {
            // Arrange: Create controller and a valid model
            var controller = new ObstacleController();
            var obstacle = new ObstacleData
            {
                // You can add realistic example data here, e.g.:
                // Height = 45, Location = "58.12N, 07.23E", etc.
            };

            // Act: Call the POST method (ModelState is valid by default)
            var result = controller.DataForm(obstacle);

            // Assert: Verify that we got a ViewResult
            var viewResult = Assert.IsType<ViewResult>(result);

            // The controller should render the "Overview" view explicitly
            Assert.Equal("Overview", viewResult.ViewName);

            // The model passed to the Overview view should be the same object
            Assert.Same(obstacle, viewResult.Model);
        }
    }
}