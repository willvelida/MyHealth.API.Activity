using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Activity.Services;
using MyHealth.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Activity.Functions
{
    public class GetAllActivities
    {
        private readonly IActivityDbService _activityDbService;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;

        public GetAllActivities(
            IActivityDbService activityDbService,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration)
        {
            _activityDbService = activityDbService ?? throw new ArgumentNullException(nameof(activityDbService));
            _serviceBusHelpers = serviceBusHelpers ?? throw new ArgumentNullException(nameof(serviceBusHelpers));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [FunctionName(nameof(GetAllActivities))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Activity")] HttpRequest req,
            ILogger log)
        {
            IActionResult result;
            List<mdl.Activity> activities = new List<mdl.Activity>();

            try
            {
                var activityEnvelopes = await _activityDbService.GetActivities();

                foreach (var item in activityEnvelopes)
                {
                    activities.Add(item.Activity);
                }

                result = new OkObjectResult(activities);
            }
            catch (Exception ex)
            {
                log.LogError($"Internal Server Error. Exception thrown: {ex.Message}");
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}

