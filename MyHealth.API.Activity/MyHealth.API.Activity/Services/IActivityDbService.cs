using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Activity.Services
{
    public interface IActivityDbService
    {
        /// <summary>
        /// Retrieves all activities from the Records container.
        /// </summary>
        /// <returns></returns>
        Task<List<mdl.ActivityEnvelope>> GetActivities();

        /// <summary>
        /// Gets the activity by a provided date
        /// </summary>
        /// <param name="activityDate"></param>
        /// <returns></returns>
        Task<mdl.ActivityEnvelope> GetActivityByDate(string activityDate);
    }
}
