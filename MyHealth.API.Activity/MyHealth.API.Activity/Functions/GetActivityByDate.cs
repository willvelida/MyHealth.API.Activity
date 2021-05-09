using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Activity.Services;
using MyHealth.API.Activity.Validators;
using MyHealth.Common;
using System;
using System.Threading.Tasks;

namespace MyHealth.API.Activity.Functions
{
    public class GetActivityByDate
    {
        private readonly IActivityDbService _activityDbService;
        private readonly IDateValidator _dateValidator;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;

        public GetActivityByDate(
            IActivityDbService activityDbService,
            IDateValidator dateValidator,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration)
        {
            _activityDbService = activityDbService ?? throw new ArgumentNullException(nameof(activityDbService));
            _serviceBusHelpers = serviceBusHelpers ?? throw new ArgumentNullException(nameof(serviceBusHelpers));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dateValidator = dateValidator ?? throw new ArgumentNullException(nameof(dateValidator));
        }

        [FunctionName(nameof(GetActivityByDate))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Activity")] HttpRequest req,
            ILogger log,
            string activityDate)
        {
            IActionResult result = null;

            try
            {
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
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
