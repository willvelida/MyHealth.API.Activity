namespace MyHealth.API.Activity.Validators
{
    public interface IDateValidator
    {
        /// <summary>
        /// Validates the provided activity date to ensure it's in the correct format.
        /// </summary>
        /// <param name="activityDate"></param>
        /// <returns></returns>
        bool IsActivityDateValid(string activityDate);
    }
}
