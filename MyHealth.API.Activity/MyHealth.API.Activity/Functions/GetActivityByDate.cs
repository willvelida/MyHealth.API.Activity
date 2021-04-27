using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyHealth.API.Activity.Services;
using MyHealth.API.Activity.Validators;
using System;
using System.Threading.Tasks;

namespace MyHealth.API.Activity.Functions
{
    public class GetActivityByDate
    {
        private readonly IActivityDbService _activityDbService;
        private readonly IDateValidator _dateValidator;

        public GetActivityByDate(
            IActivityDbService activityDbService,
            IDateValidator dateValidator)
        {
            _activityDbService = activityDbService;
            _dateValidator = dateValidator;
        }

        [FunctionName(nameof(GetActivityByDate))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Activity")] HttpRequest req,
            ILogger log)
        {
            IActionResult result = null;

            try
            {
                string activityDate = req.Query["date"];

                bool isDateValid = _dateValidator.IsActivityDateValid(activityDate);
                if (isDateValid == false)
                {
                    result = new BadRequestResult();
                    return result;
                }

                var activityResponse = await _activityDbService.GetActivityByDate(activityDate);
                if (activityResponse == null)
                {
                    result = new NotFoundResult();
                    return result;
                }

                var activity = activityResponse.Activity;

                result = new OkObjectResult(activity);

            }
            catch (Exception ex)
            {
                log.LogError($"Internal Server Error. Exception thrown: {ex.Message}");
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
