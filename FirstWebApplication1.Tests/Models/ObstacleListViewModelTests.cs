using System;
using System.Collections.Generic;
using System.Linq;
using FirstWebApplication1.Models;
using Xunit;

namespace FirstWebApplication1.Tests.Models
{
    public class ObstacleListViewModelTests
    {
        [Fact]
        public void Obstacles_DefaultValue_ShouldBeEmpty()
        {
            // Arrange & Act
            var viewModel = new ObstacleListViewModel();

            // Assert
            Assert.NotNull(viewModel.Obstacles);
            Assert.Empty(viewModel.Obstacles);
        }

        [Fact]
        public void Obstacles_CanBeSet_AndRetrieved()
        {
            // Arrange
            var viewModel = new ObstacleListViewModel();
            var obstacles = new List<ObstacleData>
            {
                new ObstacleData { Id = 1, ObstacleName = "Tower 1" },
                new ObstacleData { Id = 2, ObstacleName = "Tower 2" }
            };

            // Act
            viewModel.Obstacles = obstacles;

            // Assert
            Assert.Equal(2, viewModel.Obstacles.Count());
            Assert.Equal("Tower 1", viewModel.Obstacles.First().ObstacleName);
        }

        [Theory]
        [InlineData(null, "Obstacle Reports")]
        [InlineData("", "Obstacle Reports")]
        [InlineData("Approved", "Approved Obstacles")]
        [InlineData("Declined", "Rejected Obstacles")]
        [InlineData("Pending", "Pending Obstacles")]
        [InlineData("InvalidStatus", "Obstacle Reports")]
        public void Title_ShouldReturnCorrectValue_BasedOnStatusFilter(string statusFilter, string expectedTitle)
        {
            // Arrange
            var viewModel = new ObstacleListViewModel
            {
                StatusFilter = statusFilter
            };

            // Act
            var result = viewModel.Title;

            // Assert
            Assert.Equal(expectedTitle, result);
        }

        [Theory]
        [InlineData(null, "Manage and view all registered obstacles")]
        [InlineData("", "Manage and view all registered obstacles")]
        [InlineData("Approved", "Obstacles that have been reviewed and approved.")]
        [InlineData("Declined", "Obstacles that were rejected during review.")]
        [InlineData("Pending", "Obstacles waiting for review.")]
        [InlineData("InvalidStatus", "Manage and view all registered obstacles")]
        public void Description_ShouldReturnCorrectValue_BasedOnStatusFilter(string statusFilter, string expectedDescription)
        {
            // Arrange
            var viewModel = new ObstacleListViewModel
            {
                StatusFilter = statusFilter
            };

            // Act
            var result = viewModel.Description;

            // Assert
            Assert.Equal(expectedDescription, result);
        }

        [Fact]
        public void FilterProperties_CanBeSet_AndRetrieved()
        {
            // Arrange
            var viewModel = new ObstacleListViewModel();
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);

            // Act
            viewModel.SearchTerm = "test search";
            viewModel.StatusFilter = "Pending";
            viewModel.ObstacleTypeFilter = "Tower";
            viewModel.StartDate = startDate;
            viewModel.EndDate = endDate;

            // Assert
            Assert.Equal("test search", viewModel.SearchTerm);
            Assert.Equal("Pending", viewModel.StatusFilter);
            Assert.Equal("Tower", viewModel.ObstacleTypeFilter);
            Assert.Equal(startDate, viewModel.StartDate);
            Assert.Equal(endDate, viewModel.EndDate);
        }

        [Fact]
        public void FilterProperties_DefaultValues_ShouldBeNull()
        {
            // Arrange & Act
            var viewModel = new ObstacleListViewModel();

            // Assert
            Assert.Null(viewModel.SearchTerm);
            Assert.Null(viewModel.StatusFilter);
            Assert.Null(viewModel.ObstacleTypeFilter);
            Assert.Null(viewModel.StartDate);
            Assert.Null(viewModel.EndDate);
        }

        [Fact]
        public void Title_And_Description_ShouldBeDynamic_BasedOnStatusFilter()
        {
            // Arrange
            var pendingViewModel = new ObstacleListViewModel { StatusFilter = "Pending" };
            var approvedViewModel = new ObstacleListViewModel { StatusFilter = "Approved" };
            var declinedViewModel = new ObstacleListViewModel { StatusFilter = "Declined" };
            var defaultViewModel = new ObstacleListViewModel { StatusFilter = null };

            // Act & Assert
            Assert.Equal("Pending Obstacles", pendingViewModel.Title);
            Assert.Equal("Obstacles waiting for review.", pendingViewModel.Description);

            Assert.Equal("Approved Obstacles", approvedViewModel.Title);
            Assert.Equal("Obstacles that have been reviewed and approved.", approvedViewModel.Description);

            Assert.Equal("Rejected Obstacles", declinedViewModel.Title);
            Assert.Equal("Obstacles that were rejected during review.", declinedViewModel.Description);

            Assert.Equal("Obstacle Reports", defaultViewModel.Title);
            Assert.Equal("Manage and view all registered obstacles", defaultViewModel.Description);
        }
    }
}

