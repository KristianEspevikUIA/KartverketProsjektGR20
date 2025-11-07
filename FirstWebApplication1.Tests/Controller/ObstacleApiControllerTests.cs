using FirstWebApplication1.Controllers;
using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FirstWebApplication1.Tests.Controllers
{
    public class ObstacleApiControllerTests
    {
        private ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private ObstacleController CreateControllerWithUser(ApplicationDbContext context, string username, string role)
        {
            var controller = new ObstacleController(context);
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task ApproveApi_ValidId_ReturnsOkResult()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var obstacle = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleHeight = 50,
                ObstacleDescription = "Test",
                Latitude = 58.0,
                Longitude = 7.0,
                Status = "Pending"
            };
            context.Obstacles.Add(obstacle);
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context, "testuser", "Registerfører");

            // Act
            var result = await controller.ApproveApi(obstacle.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedObstacle = await context.Obstacles.FindAsync(obstacle.Id);
            Assert.Equal("Approved", updatedObstacle!.Status);
            Assert.Equal("testuser", updatedObstacle.ApprovedBy);
        }

        [Fact]
        public async Task ApproveApi_InvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var controller = CreateControllerWithUser(context, "testuser", "Registerfører");

            // Act
            var result = await controller.ApproveApi(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task RejectApi_ValidId_ReturnsOkResult()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var obstacle = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleHeight = 50,
                ObstacleDescription = "Test",
                Latitude = 58.0,
                Longitude = 7.0,
                Status = "Pending"
            };
            context.Obstacles.Add(obstacle);
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context, "testuser", "Registerfører");
            var request = new RejectRequest { DeclineReason = "Test reason" };

            // Act
            var result = await controller.RejectApi(obstacle.Id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedObstacle = await context.Obstacles.FindAsync(obstacle.Id);
            Assert.Equal("Declined", updatedObstacle!.Status);
            Assert.Equal("testuser", updatedObstacle.DeclinedBy);
            Assert.Equal("Test reason", updatedObstacle.DeclineReason);
        }

        [Fact]
        public async Task DeleteApi_ValidId_ReturnsOkResult()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var obstacle = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleHeight = 50,
                ObstacleDescription = "Test",
                Latitude = 58.0,
                Longitude = 7.0,
                Status = "Pending"
            };
            context.Obstacles.Add(obstacle);
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context, "testuser", "Registerfører");

            // Act
            var result = await controller.DeleteApi(obstacle.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var deletedObstacle = await context.Obstacles.FindAsync(obstacle.Id);
            Assert.Null(deletedObstacle);
        }

        [Fact]
        public async Task DeleteApi_InvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var controller = CreateControllerWithUser(context, "testuser", "Registerfører");

            // Act
            var result = await controller.DeleteApi(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
