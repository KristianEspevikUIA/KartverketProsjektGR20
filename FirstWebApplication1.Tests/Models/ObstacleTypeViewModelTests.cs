using System.Collections.Generic;
using System.Linq;
using FirstWebApplication1.Models;
using Xunit;

namespace FirstWebApplication1.Tests.Models
{
    public class ObstacleTypeViewModelTests
    {
        // Test 1: Sjekker at SelectedType har standardverdi null ved opprettelse
        [Fact]
        public void SelectedType_DefaultValue_ShouldBeNull()
        {
            // Arrange & Act: Oppretter nytt ViewModel-objekt
            var viewModel = new ObstacleTypeViewModel();

            // Assert: Verifiserer at SelectedType er null som standard
            Assert.Null(viewModel.SelectedType);
        }

        // Test 2: Sjekker at SelectedType kan settes og hentes korrekt
        [Fact]
        public void SelectedType_CanBeSet_AndRetrieved()
        {
            // Arrange: Oppretter nytt ViewModel-objekt
            var viewModel = new ObstacleTypeViewModel();

            // Act: Setter verdi
            viewModel.SelectedType = "Tower";

            // Assert: Verifiserer at verdien ble lagret korrekt
            Assert.Equal("Tower", viewModel.SelectedType);
        }

        // Test 3: Sjekker at SelectedType kan settes tilbake til null
        [Fact]
        public void SelectedType_CanBeSetToNull()
        {
            // Arrange: Oppretter ViewModel med verdi satt
            var viewModel = new ObstacleTypeViewModel { SelectedType = "Crane" };

            // Act: Setter verdien til null
            viewModel.SelectedType = null;

            // Assert: Verifiserer at verdien nå er null
            Assert.Null(viewModel.SelectedType);
        }
    }

    public class ObstacleTypesTests
    {
        // Test 4: Sjekker at listen med hindringstyper inneholder seks elementer
        [Fact]
        public void Types_ShouldContainSixObstacleTypes()
        {
            // Arrange & Act: Henter listen med typer
            var types = ObstacleTypes.Types;

            // Assert: Verifiserer at listen inneholder 6 elementer
            Assert.Equal(6, types.Count);
        }

        // Test 5: Sjekker at listen inneholder alle forventede hindringstyper
        [Fact]
        public void Types_ShouldContainExpectedObstacleTypes()
        {
            // Arrange & Act: Henter typene og verdiene deres
            var types = ObstacleTypes.Types;
            var typeValues = types.Select(t => t.Value).ToList();

            // Assert: Verifiserer at alle forventede typer finnes i listen
            Assert.Contains("Crane", typeValues);
            Assert.Contains("Tower", typeValues);
            Assert.Contains("Building", typeValues);
            Assert.Contains("Mast", typeValues);
            Assert.Contains("Windmill", typeValues);
            Assert.Contains("Other", typeValues);
        }

        // Test 6: Sjekker at hver hindringstype har riktige tilknyttede egenskaper
        [Theory]
        [InlineData("Crane", "Crane", "fa-solid fa-tower-cell", "Construction cranes and lifting equipment")]
        [InlineData("Tower", "Tower", "fa-solid fa-broadcast-tower", "Communication and broadcast towers")]
        [InlineData("Building", "Building", "fa-solid fa-building", "Tall buildings and structures")]
        [InlineData("Mast", "Mast", "fa-solid fa-signal", "Radio masts and antenna structures")]
        [InlineData("Windmill", "Windmill", "fa-solid fa-wind", "Wind turbines and windmills")]
        [InlineData("Other", "Other", "fa-solid fa-question", "Other types of obstacles")]
        public void EachObstacleType_ShouldHaveCorrectProperties(string value, string displayName, string icon, string description)
        {
            // Arrange: Henter aktuell type basert på verdi
            var type = ObstacleTypes.Types.FirstOrDefault(t => t.Value == value);

            // Assert: Verifiserer at typen finnes og alle egenskaper stemmer
            Assert.NotNull(type);
            Assert.Equal(value, type.Value);
            Assert.Equal(displayName, type.DisplayName);
            Assert.Equal(icon, type.Icon);
            Assert.Equal(description, type.Description);
        }

        // Test 7: Sjekker at Types er en lesbar liste og ikke null
        [Fact]
        public void Types_ShouldBeReadOnlyList()
        {
            // Arrange & Act: Henter listen med typer
            var types = ObstacleTypes.Types;

            // Assert: Verifiserer at listen finnes og er av typen List
            Assert.IsType<List<ObstacleTypeOption>>(types);
            Assert.NotNull(types);
        }

        // Test 8: Sjekker at hindringstypene er i forventet rekkefølge
        [Fact]
        public void Types_ShouldBeInExpectedOrder()
        {
            // Arrange & Act: Henter typer og definerer ønsket rekkefølge
            var types = ObstacleTypes.Types;
            var expectedOrder = new List<string> { "Crane", "Tower", "Building", "Mast", "Windmill", "Other" };

            // Assert: Verifiserer at rekkefølgen stemmer
            for (int i = 0; i < expectedOrder.Count; i++)
            {
                Assert.Equal(expectedOrder[i], types[i].Value);
            }
        }
    }

    public class ObstacleTypeOptionTests
    {
        // Test 9: Sjekker at alle egenskaper har tom streng som standardverdi
        [Fact]
        public void ObstacleTypeOption_DefaultValues_ShouldBeEmptyStrings()
        {
            // Arrange & Act: Oppretter nytt ObstacleTypeOption-objekt
            var option = new ObstacleTypeOption();

            // Assert: Verifiserer at alle felter er tomme strenger
            Assert.Equal(string.Empty, option.Value);
            Assert.Equal(string.Empty, option.DisplayName);
            Assert.Equal(string.Empty, option.Icon);
            Assert.Equal(string.Empty, option.Description);
        }

        // Test 10: Sjekker at alle egenskaper kan settes individuelt
        [Fact]
        public void ObstacleTypeOption_Properties_CanBeSet()
        {
            // Arrange: Oppretter nytt objekt
            var option = new ObstacleTypeOption();

            // Act: Setter alle egenskaper
            option.Value = "TestValue";
            option.DisplayName = "Test Display";
            option.Icon = "test-icon";
            option.Description = "Test Description";

            // Assert: Verifiserer at alle verdier er korrekt lagret
            Assert.Equal("TestValue", option.Value);
            Assert.Equal("Test Display", option.DisplayName);
            Assert.Equal("test-icon", option.Icon);
            Assert.Equal("Test Description", option.Description);
        }

        // Test 11: Sjekker at alle egenskaper kan settes via objekt-initialisering
        [Fact]
        public void ObstacleTypeOption_AllProperties_CanBeSetViaObjectInitializer()
        {
            // Arrange & Act: Oppretter objekt og setter alle verdier samtidig
            var option = new ObstacleTypeOption
            {
                Value = "Bridge",
                DisplayName = "Bridge",
                Icon = "fa-bridge",
                Description = "Bridges and overpasses"
            };

            // Assert: Verifiserer at alle verdier er korrekt satt
            Assert.Equal("Bridge", option.Value);
            Assert.Equal("Bridge", option.DisplayName);
            Assert.Equal("fa-bridge", option.Icon);
            Assert.Equal("Bridges and overpasses", option.Description);
        }
    }
}