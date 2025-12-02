using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication1.Controllers;
using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Identity; // Still needed for IdentityUser
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FirstWebApplication1.Tests.Controllers
{
    public class PilotControllerTests
    {
        private ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        // CORRECTED: This method now correctly creates the PilotController.
        private PilotController CreateController(ApplicationDbContext context)
        {
            // The PilotController does not need UserManager, so we don't pass it in.
            return new PilotController(context);
        }

        [Fact]
        public void Map_ReturnsView()
        {
            using var context = CreateDbContext();
            var controller = CreateController(context);

            var result = controller.Map();

            Assert.IsType<ViewResult>(result);
        }
        
        [Fact]
        public async Task GetApprovedObstacles_ReturnsApprovedAndPending()
        {
            using var context = CreateDbContext();
            
            context.Obstacles.AddRange(
                new ObstacleData { Id = 1, ObstacleName = "Approved Tower", Status = "Approved", ObstacleHeight = 50, Latitude = 58.1, Longitude = 7.9, ObstacleDescription = "Test" },
                new ObstacleData { Id = 2, ObstacleName = "Pending Tower", Status = "Pending", ObstacleHeight = 30, Latitude = 59.2, Longitude = 8.8, ObstacleDescription = "Pending" },
                new ObstacleData { Id = 3, ObstacleName = "Rejected Tower", Status = "Declined", ObstacleHeight = 100, Latitude = 58.2, Longitude = 7.8, ObstacleDescription = "Test2" }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.GetApprovedObstacles();

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value).ToList();
            
            Assert.Equal(2, data.Count);
            // Use reflection to check properties on the anonymous type
            Assert.Contains(data, d => (int)d.GetType().GetProperty("Id").GetValue(d) == 1);
            Assert.Contains(data, d => (int)d.GetType().GetProperty("Id").GetValue(d) == 2);
            Assert.DoesNotContain(data, d => (int)d.GetType().GetProperty("Id").GetValue(d) == 3);
        }
    }
}