using FirstWebApplication1.Models;
using Xunit;

namespace FirstWebApplication1.Tests.Models
{
    public class ErrorViewModelTests
    {
        // Test 1: Sjekker at RequestId har standardverdi null når objektet opprettes
        [Fact]
        public void RequestId_DefaultValue_ShouldBeNull()
        {
            // Arrange & Act: Oppretter et nytt ErrorViewModel-objekt
            var viewModel = new ErrorViewModel();

            // Assert: Verifiserer at RequestId er null
            Assert.Null(viewModel.RequestId);
        }

        // Test 2: Sjekker at RequestId kan settes og hentes korrekt
        [Fact]
        public void RequestId_CanBeSet_AndRetrieved()
        {
            // Arrange: Oppretter et nytt ErrorViewModel-objekt
            var viewModel = new ErrorViewModel();

            // Act: Setter en verdi på RequestId
            viewModel.RequestId = "test-123";

            // Assert: Verifiserer at verdien ble lagret korrekt
            Assert.Equal("test-123", viewModel.RequestId);
        }

        // Test 3: Sjekker at ShowRequestId returnerer false når RequestId er null
        [Fact]
        public void ShowRequestId_WhenRequestIdIsNull_ShouldReturnFalse()
        {
            // Arrange: Oppretter ErrorViewModel med RequestId satt til null
            var viewModel = new ErrorViewModel { RequestId = null };

            // Act: Leser verdien til ShowRequestId
            var result = viewModel.ShowRequestId;

            // Assert: Forventer at ShowRequestId er false
            Assert.False(result);
        }

        // Test 4: Sjekker at ShowRequestId returnerer false når RequestId er tom streng
        [Fact]
        public void ShowRequestId_WhenRequestIdIsEmpty_ShouldReturnFalse()
        {
            // Arrange: Oppretter ErrorViewModel med tom RequestId
            var viewModel = new ErrorViewModel { RequestId = "" };

            // Act: Leser verdien til ShowRequestId
            var result = viewModel.ShowRequestId;

            // Assert: Forventer at ShowRequestId er false
            Assert.False(result);
        }

        // Test 5: Sjekker at ShowRequestId returnerer true når RequestId har en verdi
        [Fact]
        public void ShowRequestId_WhenRequestIdHasValue_ShouldReturnTrue()
        {
            // Arrange: Oppretter ErrorViewModel med gyldig RequestId
            var viewModel = new ErrorViewModel { RequestId = "12345" };

            // Act: Leser verdien til ShowRequestId
            var result = viewModel.ShowRequestId;

            // Assert: Forventer at ShowRequestId er true
            Assert.True(result);
        }
    }
}