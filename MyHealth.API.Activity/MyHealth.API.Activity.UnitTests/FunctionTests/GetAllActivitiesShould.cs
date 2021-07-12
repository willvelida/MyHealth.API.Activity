using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.API.Activity.Functions;
using MyHealth.API.Activity.Services;
using MyHealth.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Activity.UnitTests.FunctionTests
{
    public class GetAllActivitiesShould
    {
        private Mock<IActivityDbService> _mockActivityDbService;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger> _mockLogger;

        private GetAllActivities _func;

        public GetAllActivitiesShould()
        {
            _mockActivityDbService = new Mock<IActivityDbService>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger>();

            _func = new GetAllActivities(
                _mockActivityDbService.Object,
                _mockServiceBusHelpers.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenActivitiesAreFound()
        {
            // Arrange
            var fixture = new Fixture();
            var activities = new List<mdl.ActivityEnvelope>();
            var testActivityEnvelope = fixture.Create<mdl.ActivityEnvelope>();
            activities.Add(testActivityEnvelope);
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activities));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockActivityDbService.Setup(x => x.GetActivities()).ReturnsAsync(activities);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenNoActivitiesFound()
        {
            // Arrange
            var activities = new List<mdl.ActivityEnvelope>();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activities));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockActivityDbService.Setup(x => x.GetActivities()).ReturnsAsync(activities);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ThrowBadRequestResultWhenActivityEnvelopesAreNull()
        {
            // Arrange
            MemoryStream memoryStream = new MemoryStream();
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);
            _mockActivityDbService.Setup(x => x.GetActivities()).Returns(Task.FromResult<List<mdl.ActivityEnvelope>>(null));

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(NotFoundResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(404, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }


        [Fact]
        public async Task Throw500InternalServerErrorStatusCodeWhenActivityDbServiceThrowsException()
        {
            // Arrange
            var activities = new List<mdl.ActivityEnvelope>();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activities));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockActivityDbService.Setup(x => x.GetActivities()).ThrowsAsync(new Exception());

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(StatusCodeResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(500, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
