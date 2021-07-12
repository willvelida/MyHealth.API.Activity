using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.API.Activity.Services;
using MyHealth.API.Activity.UnitTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Activity.UnitTests.ServiceTests
{
    public class ActivityDbServiceShould
    {
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Container> _mockContainer;
        private Mock<IConfiguration> _mockConfiguration;

        private ActivityDbService _sut;

        public ActivityDbServiceShould()
        {
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_mockContainer.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["DatabaseName"]).Returns("db");
            _mockConfiguration.Setup(x => x["ContainerName"]).Returns("col");

            _sut = new ActivityDbService(
                _mockConfiguration.Object,
                _mockCosmosClient.Object);
        }

        [Fact]
        public async Task GetAllActivites()
        {
            // Arrange
            List<mdl.ActivityEnvelope> activityEnvelopes = new List<mdl.ActivityEnvelope>();
            var fixture = new Fixture();
            mdl.ActivityEnvelope activityEnvelope = fixture.Create<mdl.ActivityEnvelope>(); ;
            activityEnvelopes.Add(activityEnvelope);

            _mockContainer.SetupItemQueryIteratorMock(activityEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int> { activityEnvelopes.Count });

            // Act
            var response = await _sut.GetActivities();

            // Assert
            Assert.Equal(activityEnvelopes.Count, response.Count);
        }

        [Fact]
        public async Task GetAllActivies_NoResultsReturned()
        {
            // Arrange
            var emptyActivitiesList = new List<mdl.ActivityEnvelope>();

            var getActivities = _mockContainer.SetupItemQueryIteratorMock(emptyActivitiesList);
            getActivities.feedIterator.Setup(x => x.HasMoreResults).Returns(false);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { 0 });

            // Act
            var response = await _sut.GetActivities();

            // Act
            Assert.Empty(response);
        }

        [Fact]
        public async Task CatchExceptionWhenCosmosThrowsExceptionWhenGetActivitiesIsCalled()
        {
            // Arrange
            _mockContainer.Setup(x => x.GetItemQueryIterator<mdl.ActivityEnvelope>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
                .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.GetActivities();

            // Act
            await responseAction.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetActivityByDate()
        {
            // Arrange
            List<mdl.ActivityEnvelope> activityEnvelopes = new List<mdl.ActivityEnvelope>();
            mdl.ActivityEnvelope activityEnvelope = new mdl.ActivityEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                DocumentType = "Test",
                Activity = new mdl.Activity
                {
                    CaloriesBurned = 4500,
                    ActivityDate = "31/12/2019"
                }
            };
            activityEnvelopes.Add(activityEnvelope);

            _mockContainer.SetupItemQueryIteratorMock(activityEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int> { activityEnvelopes.Count });

            var activityDate = activityEnvelope.Activity.ActivityDate;

            // Act
            var response = await _sut.GetActivityByDate(activityDate);

            // Assert
            Assert.Equal(activityDate, response.Activity.ActivityDate);
        }

        [Fact]
        public async Task GetActivityByDate_NoResultsReturned()
        {
            // Arrange
            var emptyActivitiesList = new List<mdl.ActivityEnvelope>();

            var getActivities = _mockContainer.SetupItemQueryIteratorMock(emptyActivitiesList);
            getActivities.feedIterator.Setup(x => x.HasMoreResults).Returns(false);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { 0 });

            // Act
            var response = await _sut.GetActivityByDate("31/12/2019");

            // Act
            Assert.Null(response);
        }

        [Fact]
        public async Task CatchExceptionWhenCosmosThrowsExceptionWhenGetActivityByDateIsCalled()
        {
            // Arrange
            _mockContainer.Setup(x => x.GetItemQueryIterator<mdl.ActivityEnvelope>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
                .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.GetActivityByDate("31/12/2019");

            // Act
            await responseAction.Should().ThrowAsync<Exception>();
        }
    }
}
