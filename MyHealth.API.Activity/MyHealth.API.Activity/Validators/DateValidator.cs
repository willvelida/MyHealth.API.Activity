using System;
using System.Globalization;

namespace MyHealth.API.Activity.Validators
{
    public class DateValidator : IDateValidator
    {
        public bool IsActivityDateValid(string activityDate)
        {
            bool isDateValid = false;
            string pattern = "yyyy-MM-dd";
            DateTime parsedActivityDate;

            if (DateTime.TryParseExact(activityDate, pattern, null, DateTimeStyles.None, out parsedActivityDate))
            {
                isDateValid = true;
            }

            return isDateValid;
        }
    }
}
