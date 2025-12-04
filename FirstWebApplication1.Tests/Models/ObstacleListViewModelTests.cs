using System;
using System.Collections.Generic;
using System.Linq;
using FirstWebApplication1.Models;
using Xunit;

namespace FirstWebApplication1.Tests.Models
{
    public class ObstacleListViewModelTests
    {
        // Test 1: Sjekker at Obstacles har standardverdi som tom liste
        [Fact]
        public void Obstacles_DefaultValue_ShouldBeEmpty()
        {
            // Arrange & Act: Oppretter nytt ViewModel-objekt
            var viewModel = new ObstacleListViewModel();

            // Assert: Verifiserer at listen finnes og er tom
            Assert.NotNull(viewModel.Obstacles);
            Assert.Empty(viewModel.Obstacles);
        }

        // Test 2: Sjekker at Obstacles kan settes og hentes korrekt
        [Fact]
        public void Obstacles_CanBeSet_AndRetrieved()
        {
            // Arrange: Oppretter ViewModel og en liste med hindringer
            var viewModel = new ObstacleListViewModel();
            var obstacles = new List<ObstacleData>
            {
                new ObstacleData { Id = 1, ObstacleName = "Tower 1" },
                new ObstacleData { Id = 2, ObstacleName = "Tower 2" }
            };

            // Act: Setter listen inn i ViewModel
            viewModel.Obstacles = obstacles;

            // Assert: Verifiserer at riktig antall og riktig første element er lagret
            Assert.Equal(2, viewModel.Obstacles.Count());
            Assert.Equal("Tower 1", viewModel.Obstacles.First().ObstacleName);
        }

        // Test 3: Sjekker at Title returnerer korrekt verdi basert på statusfilter
        [Theory]
        [InlineData(null, "Obstacle Reports")]
        [InlineData("", "Obstacle Reports")]
        [InlineData("Approved", "Approved Obstacles")]
        [InlineData("Declined", "Rejected Obstacles")]
        [InlineData("Pending", "Pending Obstacles")]
        [InlineData("InvalidStatus", "Obstacle Reports")]
        public void Title_ShouldReturnCorrectValue_BasedOnStatusFilter(string statusFilter, string expectedTitle)
        {
            // Arrange: Oppretter ViewModel med gitt statusfilter
            var viewModel = new ObstacleListViewModel
            {
                StatusFilter = statusFilter
            };

            // Act: Leser Title-verdien
            var result = viewModel.Title;

            // Assert: Verifiserer at tittelen stemmer med forventet resultat
            Assert.Equal(expectedTitle, result);
        }

        // Test 4: Sjekker at Description returnerer korrekt verdi basert på statusfilter
        [Theory]
        [InlineData(null, "Manage and view all registered obstacles")]
        [InlineData("", "Manage and view all registered obstacles")]
        [InlineData("Approved", "Obstacles that have been reviewed and approved.")]
        [InlineData("Declined", "Obstacles that were rejected during review.")]
        [InlineData("Pending", "Obstacles waiting for review.")]
        [InlineData("InvalidStatus", "Manage and view all registered obstacles")]
        public void Description_ShouldReturnCorrectValue_BasedOnStatusFilter(string statusFilter, string expectedDescription)
        {
            // Arrange: Oppretter ViewModel med gitt statusfilter
            var viewModel = new ObstacleListViewModel
            {
                StatusFilter = statusFilter
            };

            // Act: Leser Description-verdien
            var result = viewModel.Description;

            // Assert: Verifiserer at beskrivelsen stemmer med forventet verdi
            Assert.Equal(expectedDescription, result);
        }

        // Test 5: Sjekker at alle filteregenskaper kan settes og hentes korrekt
        [Fact]
        public void FilterProperties_CanBeSet_AndRetrieved()
        {
            // Arrange: Oppretter ViewModel og datoer for filtrering
            var viewModel = new ObstacleListViewModel();
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);

            // Act: Setter alle filterverdier
            viewModel.SearchTerm = "test search";
            viewModel.StatusFilter = "Pending";
            viewModel.ObstacleTypeFilter = "Tower";
            viewModel.StartDate = startDate;
            viewModel.EndDate = endDate;

            // Assert: Verifiserer at alle verdier er riktig lagret
            Assert.Equal("test search", viewModel.SearchTerm);
            Assert.Equal("Pending", viewModel.StatusFilter);
            Assert.Equal("Tower", viewModel.ObstacleTypeFilter);
            Assert.Equal(startDate, viewModel.StartDate);
            Assert.Equal(endDate, viewModel.EndDate);
        }

        // Test 6: Sjekker at filteregenskapene har null som standardverdi
        [Fact]
        public void FilterProperties_DefaultValues_ShouldBeNull()
        {
            // Arrange & Act: Oppretter nytt ViewModel-objekt
            var viewModel = new ObstacleListViewModel();

            // Assert: Verifiserer at alle filterverdier er null
            Assert.Null(viewModel.SearchTerm);
            Assert.Null(viewModel.StatusFilter);
            Assert.Null(viewModel.ObstacleTypeFilter);
            Assert.Null(viewModel.StartDate);
            Assert.Null(viewModel.EndDate);
        }

        // Test 7: Sjekker at både Title og Description endrer seg dynamisk basert på statusfilter
        [Fact]
        public void Title_And_Description_ShouldBeDynamic_BasedOnStatusFilter()
        {
            // Arrange: Oppretter ViewModel-objekter med ulike statuser
            var pendingViewModel = new ObstacleListViewModel { StatusFilter = "Pending" };
            var approvedViewModel = new ObstacleListViewModel { StatusFilter = "Approved" };
            var declinedViewModel = new ObstacleListViewModel { StatusFilter = "Declined" };
            var defaultViewModel = new ObstacleListViewModel { StatusFilter = null };

            // Act & Assert: Verifiserer riktige titler og beskrivelser for hver status
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

