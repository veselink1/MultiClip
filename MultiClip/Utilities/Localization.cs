using System;

namespace MultiClip.Utilities
{
    public static class Localization
    {
        public static string ToLocalTimepointString(DateTime dateTime)
        {
            string dayString;

            int dayDiff = DateTime.Now.DayOfYear - dateTime.DayOfYear;
            if (dayDiff < 7 && DateTime.Now.DayOfWeek >= dateTime.DayOfWeek)
            {
                if (dayDiff == 0)
                {
                    dayString = "Today";
                }
                else if (dayDiff == 1)
                {
                    dayString = "Yesterday";
                }
                else
                {
                    dayString = dateTime.DayOfWeek.ToString("D");
                }
            }
            else
            {
                dayString = dateTime.Day + " " + dateTime.ToString("MMM");
            }

            return dayString + ", " + dateTime.ToString("h:mm tt");
        }
    }
}