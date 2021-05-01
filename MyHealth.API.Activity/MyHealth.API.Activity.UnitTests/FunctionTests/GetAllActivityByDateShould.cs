using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.API.Activity.Functions;
using MyHealth.API.Activity.Services;
using MyHealth.API.Activity.Validators;
using MyHealth.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Activity.UnitTests.FunctionTests
{
    public class GetAllActivityByDateShould
    {
        private Mock<IActivityDbService> _mockActivityDbService;
        private Mock<IDateValidator> _mockDateValidator;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger> _mockLogger;

        private GetActivityByDate _func;

        public GetAllActivityByDateShould()
        {
            _mockActivityDbService = new Mock<IActivityDbService>();
            _mockDateValidator = new Mock<IDateValidator>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger>();

            _func = new GetActivityByDate(
                _mockActivityDbService.Object,
                _mockDateValidator.Object,
                _mockServiceBusHelpers.Object,
                _mockConfiguration.Object);
        }

        [Theory]
        [InlineData("100/12/2020")]
        [InlineData("12/111/2020")]
        [InlineData("12/11/20201")]
        public async Task ThrowBadRequestResultWhenActivityDateRequestIsInvalid(string invalidDateInput)
        {
            // Arrange
            var activityEnvelope = new mdl.ActivityEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activityEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Query["date"]).Returns(invalidDateInput);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsActivityDateValid(invalidDateInput)).Returns(false);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(BadRequestResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(400, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ThrowNotFoundResultWhenActivityResponseIsNull()
        {
            // Arrange
            var activityEnvelope = new mdl.ActivityEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activityEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Query["date"]).Returns("31/12/2019");
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsActivityDateValid(It.IsAny<string>())).Returns(true);
            _mockActivityDbService.Setup(x => x.GetActivityByDate(It.IsAny<string>())).Returns(Task.FromResult<mdl.ActivityEnvelope>(null));

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(NotFoundResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(404, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenActivityIsFound()
        {
            // Arrange
            var activityEnvelope = new mdl.ActivityEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Activity = new mdl.Activity
                {
                    ActivityDate = "31/12/2019"
                },
                DocumentType = "Test"
            };
            var activityDate = activityEnvelope.Activity.ActivityDate;
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activityEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Query["date"]).Returns(activityDate);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsActivityDateValid(activityDate)).Returns(true);
            _mockActivityDbService.Setup(x => x.GetActivityByDate(activityDate)).ReturnsAsync(activityEnvelope);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task Throw500InternalServerErrorStatusCodeWhenActivityDbServiceThrowsException()
        {
            // Arrange
            var activityEnvelope = new mdl.ActivityEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activityEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Query["date"]).Returns("31/12/2019");
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsActivityDateValid(It.IsAny<string>())).Returns(true);
            _mockActivityDbService.Setup(x => x.GetActivityByDate(It.IsAny<string>())).ThrowsAsync(new Exception());

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
