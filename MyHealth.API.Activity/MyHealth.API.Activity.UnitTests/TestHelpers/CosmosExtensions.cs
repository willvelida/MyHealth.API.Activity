using Microsoft.Azure.Cosmos;
using Moq;
using System.Collections.Generic;
using System.Threading;

namespace MyHealth.API.Activity.UnitTests.TestHelpers
{
    public static class CosmosExtensions
    {
        public static (Mock<FeedResponse<T>> feedResponseMock, Mock<FeedIterator<T>> feedIterator) SetupItemQueryIteratorMock<T>(this Mock<Container> containerMock, IEnumerable<T> itemsToReturn)
        {
            var feedRepsonseMock = new Mock<FeedResponse<T>>();
            feedRepsonseMock.Setup(x => x.Resource).Returns(itemsToReturn);
            var iteratorMock = new Mock<FeedIterator<T>>();
            iteratorMock.SetupSequence(x => x.HasMoreResults).Returns(true).Returns(false);
            iteratorMock.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(feedRepsonseMock.Object);
            containerMock.Setup(x => x.GetItemQueryIterator<T>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(iteratorMock.Object);

            return (feedRepsonseMock, iteratorMock);
        }
    }
}
