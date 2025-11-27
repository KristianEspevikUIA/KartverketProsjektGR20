using System.Collections.Generic;
using System.Linq;
using FirstWebApplication1.Models;
using Xunit;

namespace FirstWebApplication1.Tests.Models
{
    public class ObstacleTypeViewModelTests
    {
        [Fact]
        public void SelectedType_DefaultValue_ShouldBeNull()
        {
            // Arrange & Act
            var viewModel = new ObstacleTypeViewModel();

            // Assert
            Assert.Null(viewModel.SelectedType);
        }

        [Fact]
        public void SelectedType_CanBeSet_AndRetrieved()
        {
            // Arrange
            var viewModel = new ObstacleTypeViewModel();

            // Act
            viewModel.SelectedType = "Tower";

            // Assert
            Assert.Equal("Tower", viewModel.SelectedType);
        }

        [Fact]
        public void SelectedType_CanBeSetToNull()
        {
            // Arrange
            var viewModel = new ObstacleTypeViewModel { SelectedType = "Crane" };

            // Act
            viewModel.SelectedType = null;

            // Assert
            Assert.Null(viewModel.SelectedType);
        }
    }

    public class ObstacleTypesTests
    {
        [Fact]
        public void Types_ShouldContainSixObstacleTypes()
        {
            // Arrange & Act
            var types = ObstacleTypes.Types;

            // Assert
            Assert.Equal(6, types.Count);
        }

        [Fact]
        public void Types_ShouldContainExpectedObstacleTypes()
        {
            // Arrange & Act
            var types = ObstacleTypes.Types;
            var typeValues = types.Select(t => t.Value).ToList();

            // Assert
            Assert.Contains("Crane", typeValues);
            Assert.Contains("Tower", typeValues);
            Assert.Contains("Building", typeValues);
            Assert.Contains("Mast", typeValues);
            Assert.Contains("Windmill", typeValues);
            Assert.Contains("Other", typeValues);
        }

        [Theory]
        [InlineData("Crane", "Crane", "fa-solid fa-tower-cell", "Construction cranes and lifting equipment")]
        [InlineData("Tower", "Tower", "fa-solid fa-broadcast-tower", "Communication and broadcast towers")]
        [InlineData("Building", "Building", "fa-solid fa-building", "Tall buildings and structures")]
        [InlineData("Mast", "Mast", "fa-solid fa-signal", "Radio masts and antenna structures")]
        [InlineData("Windmill", "Windmill", "fa-solid fa-wind", "Wind turbines and windmills")]
        [InlineData("Other", "Other", "fa-solid fa-question", "Other types of obstacles")]
        public void EachObstacleType_ShouldHaveCorrectProperties(string value, string displayName, string icon, string description)
        {
            // Arrange
            var type = ObstacleTypes.Types.FirstOrDefault(t => t.Value == value);

            // Assert
            Assert.NotNull(type);
            Assert.Equal(value, type.Value);
            Assert.Equal(displayName, type.DisplayName);
            Assert.Equal(icon, type.Icon);
            Assert.Equal(description, type.Description);
        }

        [Fact]
        public void Types_ShouldBeReadOnlyList()
        {
            // Arrange & Act
            var types = ObstacleTypes.Types;

            // Assert
            Assert.IsType<List<ObstacleTypeOption>>(types);
            Assert.NotNull(types);
        }

        [Fact]
        public void Types_ShouldBeInExpectedOrder()
        {
            // Arrange & Act
            var types = ObstacleTypes.Types;
            var expectedOrder = new List<string> { "Crane", "Tower", "Building", "Mast", "Windmill", "Other" };

            // Assert
            for (int i = 0; i < expectedOrder.Count; i++)
            {
                Assert.Equal(expectedOrder[i], types[i].Value);
            }
        }
    }

    public class ObstacleTypeOptionTests
    {
        [Fact]
        public void ObstacleTypeOption_DefaultValues_ShouldBeEmptyStrings()
        {
            // Arrange & Act
            var option = new ObstacleTypeOption();

            // Assert
            Assert.Equal(string.Empty, option.Value);
            Assert.Equal(string.Empty, option.DisplayName);
            Assert.Equal(string.Empty, option.Icon);
            Assert.Equal(string.Empty, option.Description);
        }

        [Fact]
        public void ObstacleTypeOption_Properties_CanBeSet()
        {
            // Arrange
            var option = new ObstacleTypeOption();

            // Act
            option.Value = "TestValue";
            option.DisplayName = "Test Display";
            option.Icon = "test-icon";
            option.Description = "Test Description";

            // Assert
            Assert.Equal("TestValue", option.Value);
            Assert.Equal("Test Display", option.DisplayName);
            Assert.Equal("test-icon", option.Icon);
            Assert.Equal("Test Description", option.Description);
        }

        [Fact]
        public void ObstacleTypeOption_AllProperties_CanBeSetViaObjectInitializer()
        {
            // Arrange & Act
            var option = new ObstacleTypeOption
            {
                Value = "Bridge",
                DisplayName = "Bridge",
                Icon = "fa-bridge",
                Description = "Bridges and overpasses"
            };

            // Assert
            Assert.Equal("Bridge", option.Value);
            Assert.Equal("Bridge", option.DisplayName);
            Assert.Equal("fa-bridge", option.Icon);
            Assert.Equal("Bridges and overpasses", option.Description);
        }
    }
}
