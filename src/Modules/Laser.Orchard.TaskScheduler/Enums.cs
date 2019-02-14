using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler {
    /// <summary>
    /// These are used to determine the time when a task is to be executed next.
    /// </summary>
    public enum TimeUnits { Seconds=1, Minutes=0, Hours=2, Days=3, Weeks=4, Months=5, Years=6 }
    public enum ExecutionTypes {WorkFlow=0, Razor=1 }
    static class EnumExtension<T> {
        public static T ParseEnum(string value) {
            if (string.IsNullOrWhiteSpace(value))
                return default(T);
            try {
                return (T)Enum.Parse(typeof(T), value);
            }
            catch (Exception) {
                return default(T);
            }
        }
    }
}