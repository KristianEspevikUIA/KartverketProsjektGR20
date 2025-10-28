using FirstWebApplication1.Models;

namespace FirstWebApplication1.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Obstacle_ShouldHaveDefaultValues()
        {
            var obstacle = new ObstacleData();
            Assert.NotNull(obstacle);
         
        }
    }
}
