using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyHealth.API.Activity.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Activity.Functions
{
    public class GetAllActivities
    {
        private readonly IActivityDbService _activityDbService;

        public GetAllActivities(
            IActivityDbService activityDbService)
        {
            _activityDbService = activityDbService ?? throw new ArgumentNullException(nameof(activityDbService));
        }

        [FunctionName(nameof(GetAllActivities))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Activities")] HttpRequest req,
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
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                throw;
            }

            return result;
        }
    }
}

