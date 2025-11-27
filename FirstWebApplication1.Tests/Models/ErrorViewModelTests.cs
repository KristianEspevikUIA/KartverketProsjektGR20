using FirstWebApplication1.Models;
using Xunit;

namespace FirstWebApplication1.Tests.Models
{
    public class ErrorViewModelTests
    {
        [Fact]
        public void RequestId_DefaultValue_ShouldBeNull()
        {
            // Arrange & Act
            var viewModel = new ErrorViewModel();

            // Assert
            Assert.Null(viewModel.RequestId);
        }

        [Fact]
        public void RequestId_CanBeSet_AndRetrieved()
        {
            // Arrange
            var viewModel = new ErrorViewModel();

            // Act
            viewModel.RequestId = "test-123";

            // Assert
            Assert.Equal("test-123", viewModel.RequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsNull_ShouldReturnFalse()
        {
            // Arrange
            var viewModel = new ErrorViewModel { RequestId = null };

            // Act
            var result = viewModel.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsEmpty_ShouldReturnFalse()
        {
            // Arrange
            var viewModel = new ErrorViewModel { RequestId = "" };

            // Act
            var result = viewModel.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsWhitespace_ShouldReturnFalse()
        {
            // Arrange
            var viewModel = new ErrorViewModel { RequestId = "   " };

            // Act
            var result = viewModel.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdHasValue_ShouldReturnTrue()
        {
            // Arrange
            var viewModel = new ErrorViewModel { RequestId = "12345" };

            // Act
            var result = viewModel.ShowRequestId;

            // Assert
            Assert.True(result);
        }
    }
}