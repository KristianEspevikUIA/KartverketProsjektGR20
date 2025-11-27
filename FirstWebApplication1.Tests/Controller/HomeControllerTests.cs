using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FirstWebApplication1.Controllers;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FirstWebApplication1.Tests.Controllers
{
    public class HomeControllerTests
    {
        // Hjelpemetode: lager en IConfiguration med en kjent connection string
        private IConfiguration CreateTestConfiguration(string connectionString = "Server=localhost;Database=test;User=test;Password=test;")
        {
            var settings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", connectionString }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        // Hjelpemetode: lager en HomeController basert på test-config
        private HomeController CreateController(string connectionString = "Server=localhost;Database=test;User=test;Password=test;")
        {
            var configuration = CreateTestConfiguration(connectionString);
            return new HomeController(configuration);
        }

        // --------------------------------------------------------------------
        // TEST 1: Konstruktøren leser connection string fra IConfiguration
        // --------------------------------------------------------------------
        [Fact]
        public void Constructor_ReadsConnectionStringFromConfiguration()
        {
            // Arrange
            var expectedConnectionString = "Server=localhost;Database=mydb;User=myuser;Password=mypassword;";
            var configuration = CreateTestConfiguration(expectedConnectionString);

            // Act
            var controller = new HomeController(configuration);

            // Assert
            // Vi bruker refleksjon for å lese den private felt-verdien _connectionString
            var field = typeof(HomeController)
                .GetField("_connectionString", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(field);

            var actualValue = field!.GetValue(controller) as string;
            Assert.Equal(expectedConnectionString, actualValue);
        }

        // --------------------------------------------------------------------
        // TEST 2: Index() returnerer en ViewResult
        // --------------------------------------------------------------------
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // --------------------------------------------------------------------
        // TEST 3: Privacy() returnerer en ViewResult
        // --------------------------------------------------------------------
        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // --------------------------------------------------------------------
        // TEST 4: Error() returnerer ViewResult med ErrorViewModel og RequestId
        // --------------------------------------------------------------------
        [Fact]
        public void Error_ReturnsViewWithErrorViewModelAndRequestId()
        {
            // Arrange
            var controller = CreateController();

            // Vi setter opp en HttpContext slik at HttpContext.TraceIdentifier ikke er null
            var httpContext = new DefaultHttpContext
            {
                TraceIdentifier = "test-trace-id"
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);

            // RequestId skal være satt enten fra Activity.Current eller fra HttpContext.TraceIdentifier
            Assert.False(string.IsNullOrEmpty(model.RequestId));
        }
    }
}