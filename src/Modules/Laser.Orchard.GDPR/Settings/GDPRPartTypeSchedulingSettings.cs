using Laser.Orchard.GDPR.Extensions;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using System.Globalization;

namespace Laser.Orchard.GDPR.Settings {
    /// <summary>
    /// This class represents the configuration that is done for a type regarding the
    /// scheduling of processes for GDPR compliance
    /// </summary>
    [OrchardFeature("Laser.Orchard.GDPR.Scheduling")]
    public class GDPRPartTypeSchedulingSettings {

        /// <summary>
        /// Flag telling us whether there should be a GDPR compliance automated process
        /// to anonymize items of this type.
        /// </summary>
        public bool ScheduleAnonymization { get; set; }

        /// <summary>
        /// What we should check on to see if enough time has passed.
        /// </summary>
        public EventForScheduling EventForAnonymization { get; set; }

        /// <summary>
        /// This represents the time in days that should have passed since creation/update of the content
        /// for the scheduled task to process it.
        /// </summary>
        public int AnonymizationDaysToWait { get; set; }

        /// <summary>
        /// Flag telling us whether there should be a GDPR compliance automated process
        /// to erase items of this type.
        /// </summary>
        public bool ScheduleErasure { get; set; }


        /// <summary>
        /// What we should check on to see if enough time has passed.
        /// </summary>
        public EventForScheduling EventForErasure { get; set; }

        /// <summary>
        /// This represents the time in days that should have passed since creation/update of the content
        /// for the scheduled task to process it.
        /// </summary>
        public int ErasureDaysToWait { get; set; }

        public static void SetValues(ContentTypePartDefinitionBuilder builder, GDPRPartTypeSchedulingSettings settings) {
            builder.WithSetting(
                "GDPRPartTypeSchedulingSettings.EventForAnonymization",
                settings.EventForAnonymization.ToString());
            builder.WithSetting(
                "GDPRPartTypeSchedulingSettings.AnonymizationDaysToWait",
                settings.AnonymizationDaysToWait.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
               "GDPRPartTypeSchedulingSettings.ScheduleAnonymization",
               settings.ScheduleAnonymization.ToString(CultureInfo.InvariantCulture));

            builder.WithSetting(
                "GDPRPartTypeSchedulingSettings.EventForErasure",
                settings.EventForErasure.ToString());
            builder.WithSetting(
                "GDPRPartTypeSchedulingSettings.ErasureDaysToWait",
                settings.ErasureDaysToWait.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
               "GDPRPartTypeSchedulingSettings.ScheduleErasure",
               settings.ScheduleErasure.ToString(CultureInfo.InvariantCulture));
        }
    }
}