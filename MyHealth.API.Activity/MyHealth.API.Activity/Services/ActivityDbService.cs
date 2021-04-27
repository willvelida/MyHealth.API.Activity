using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using MyHealth.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyHealth.API.Activity.Services
{
    public class ActivityDbService : IActivityDbService
    {
        private readonly IConfiguration _configuration;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public ActivityDbService(
            IConfiguration configuration,
            CosmosClient cosmosClient)
        {
            _configuration = configuration;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer(_configuration["DatabaseName"], _configuration["ContainerName"]);
        }

        public async Task<List<ActivityEnvelope>> GetActivities()
        {
            try
            {
                QueryDefinition query = new QueryDefinition("SELECT * FROM Records c WHERE c.DocumentType = 'Activity'");
                List<ActivityEnvelope> activityEnvelopes = new List<ActivityEnvelope>();

                FeedIterator<ActivityEnvelope> feedIterator = _container.GetItemQueryIterator<ActivityEnvelope>(query);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<ActivityEnvelope> queryResponse = await feedIterator.ReadNextAsync();
                    activityEnvelopes.AddRange(queryResponse.Resource);
                }

                return activityEnvelopes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ActivityEnvelope> GetActivityByDate(string activityDate)
        {
            try
            {
                QueryDefinition query = new QueryDefinition("SELECT * FROM Records c WHERE c.DocumentType = 'Activity' AND c.Activity.ActivityDate = @activityDate")
                    .WithParameter("@activityDate", activityDate);

                List<ActivityEnvelope> activityEnvelopes = new List<ActivityEnvelope>();

                FeedIterator<ActivityEnvelope> feedIterator = _container.GetItemQueryIterator<ActivityEnvelope>(query);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<ActivityEnvelope> queryResponse = await feedIterator.ReadNextAsync();
                    activityEnvelopes.AddRange(queryResponse.Resource);
                }

                return activityEnvelopes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
