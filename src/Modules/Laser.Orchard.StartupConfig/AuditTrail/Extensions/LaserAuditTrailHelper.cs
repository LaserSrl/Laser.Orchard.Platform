using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.AuditTrail.Extensions {
    public static class LaserAuditTrailHelper {

        public static string GetLoggerName(string siteName) {
            return $"ChannelsAuditTrailProvider-{siteName}";
        }

        public static string GetAppenderName(string siteName) {
            return $"audit-file-{siteName}";
        }

        public static string GetAppenderFilePath() {
            // The actual appender may add stuff after this, such as the date and
            // the file extension.
            return Path.Combine("App_Data", GetLogsFilePath());
        }

        public static string GetLogsFilePath() {
            // The actual appender may add stuff after this, such as the date and
            // the file extension.
            return $"Logs";
        }

        public static string GetAppenderFileName(string siteName) {
            // The actual appender may add stuff after this, such as the date and
            // the file extension.
            return $"orchard-audit-{siteName}";
        }

        public static string GetAppenderDatePattern() {
            // The actual appender may add stuff after this, such as the date and
            // the file extension.
            return "-yyyy.MM.dd";
        }

        public static string GetAppenderFileExtension() {
            // The actual appender may add stuff after this, such as the date and
            // the file extension.
            return "log";
        }
    }
}