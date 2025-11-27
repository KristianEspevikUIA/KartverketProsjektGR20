using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication1.Controllers;
using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FirstWebApplication1.Tests.Controllers
{
    public class PilotControllerTests
    {
        private ApplicationDbContext CreateDbContext()
        {
            // FULL ISOLASJON → fikser problemet
            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();

            var provider = services.BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // unik database
                .UseInternalServiceProvider(provider) // unik EF-infrastruktur
                .Options;

            return new ApplicationDbContext(options);
        }

        private PilotController CreateController(ApplicationDbContext context)
        {
            return new PilotController(context);
        }

        // -----------------------------
        // TEST 1: Map() returns a View
        // -----------------------------
        [Fact]
        public void Map_ReturnsView()
        {
            var context = CreateDbContext();
            var controller = CreateController(context);

            var result = controller.Map();

            Assert.IsType<ViewResult>(result);
        }

        // --------------------------------------------------------
        // TEST 2: GetApprovedObstacles returns Approved + Pending
        // --------------------------------------------------------
        [Fact]
        public async Task GetApprovedObstacles_ReturnsApprovedAndPending()
        {
            var context = CreateDbContext();

            context.Obstacles.Add(new ObstacleData
            {
                Id = 1,
                ObstacleName = "Approved Tower",
                Status = "Approved",
                ObstacleHeight = 50,
                Latitude = 58.1,
                Longitude = 7.9,
                ObstacleDescription = "Test",
                LineGeoJson = "{\"type\":\"LineString\",\"coordinates\":[[7.9,58.1],[8.0,58.2]]}"
            });

            context.Obstacles.Add(new ObstacleData
            {
                Id = 2,
                ObstacleName = "Pending Tower",
                Status = "Pending",
                ObstacleHeight = 30,
                Latitude = 59.2,
                Longitude = 8.8,
                ObstacleDescription = "Pending"
            });

            context.Obstacles.Add(new ObstacleData
            {
                Id = 3,
                ObstacleName = "Rejected Tower",
                Status = "Rejected",
                ObstacleHeight = 100,
                Latitude = 58.2,
                Longitude = 7.8,
                ObstacleDescription = "Test2"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetApprovedObstacles();
            var json = Assert.IsType<JsonResult>(result);

            var data = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value).ToList();

            // Skal være Approved + Pending
            var propsById = data.ToDictionary(
                obj => (int)obj.GetType().GetProperty("Id")!.GetValue(obj)!,
                obj => obj.GetType().GetProperties());

            Assert.Equal(2, propsById.Count);
            Assert.Contains(1, propsById.Keys);
            Assert.Contains(2, propsById.Keys);
        }

        // ------------------------------------------------------------------
        // TEST 3: GetApprovedObstacles includes all fields needed by the map
        // ------------------------------------------------------------------
        [Fact]
        public async Task GetApprovedObstacles_IncludesRequiredFields()
        {
            var context = CreateDbContext();

            context.Obstacles.Add(new ObstacleData
            {
                Id = 10,
                ObstacleName = "Line Tower",
                Status = "Approved",
                ObstacleHeight = 77,
                Latitude = 60.1,
                Longitude = 5.2,
                ObstacleDescription = "desc",
                LineGeoJson = "{\"type\":\"LineString\",\"coordinates\":[[5.2,60.1],[5.3,60.2]]}"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetApprovedObstacles();
            var json = Assert.IsType<JsonResult>(result);

            var data = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value);
            var item = Assert.Single(data);

            var props = item.GetType().GetProperties();

            Assert.Equal(10, (int)props.Single(p => p.Name == "Id").GetValue(item)!);
            Assert.Equal("Line Tower", (string)props.Single(p => p.Name == "ObstacleName").GetValue(item)!);
            Assert.Equal(77, (int)(double)props.Single(p => p.Name == "ObstacleHeight").GetValue(item)!);
            Assert.Equal(60.1, (double)props.Single(p => p.Name == "Latitude").GetValue(item)!);
            Assert.Equal(5.2, (double)props.Single(p => p.Name == "Longitude").GetValue(item)!);
            Assert.NotNull(props.Single(p => p.Name == "LineGeoJson").GetValue(item));
            Assert.NotNull(props.Single(p => p.Name == "Status").GetValue(item));
        }
    }
}