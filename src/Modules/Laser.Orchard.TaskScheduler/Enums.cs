using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler {
    /// <summary>
    /// These are used to determine the time when a task is to be executed next.
    /// </summary>
    public enum TimeUnits { Seconds, Minutes, Hours, Days, Weeks, Months, Years }


    static class EnumExtension {
        public static TimeUnits ParseEnum(string value) {
            if (string.IsNullOrWhiteSpace(value))
                return TimeUnits.Minutes;
            try {
                return (TimeUnits)Enum.Parse(typeof(TimeUnits), value);
            } catch (Exception) {
                return TimeUnits.Minutes;
            }
        }
    }
}