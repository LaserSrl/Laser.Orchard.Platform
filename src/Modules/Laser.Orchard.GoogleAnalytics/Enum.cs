using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.GoogleAnalytics {
    public enum CookieLevels {
        Statistical = 0,
        Technical = 1,
        Marketing=2,
        Preferences=3
    }
    static class EnumExtension<T> {
        public static T ParseEnum(string value) {
            if (string.IsNullOrWhiteSpace(value))
                return default(T);
            try {
                return (T)Enum.Parse(typeof(T), value);
            } catch (Exception) {
                return default(T);
            }
        }
    }
}