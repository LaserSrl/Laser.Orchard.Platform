namespace Laser.Orchard.GDPR.Extensions {
    public static class Constants {

        public static string ScheduledTaskName = @"GDPRScheduledProcessing";
        public static string ScheduledTaskFormat = ScheduledTaskName + @"_{0}";
        public static string GDPRCookieName = "GDPRCookie";
    }
}