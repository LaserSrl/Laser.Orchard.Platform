using Orchard.ContentManagement.MetaData.Builders;
using System.Globalization;

namespace Laser.Orchard.GDPR.Settings {
    /// <summary>
    /// This class represents the settings for a ContentType that has a GDPRPart
    /// </summary>
    public class GDPRPartTypeSettings {
        
        /// <summary>
        /// This flag causes ContentItems of the type it is set to be considered ProfileItems.
        /// These have a set of functionalities associated with them and related to GDPR. For
        /// instance, they are the ones a BO user (with the proper permissions) may invoke
        /// anonymization or erasure on.
        /// </summary>
        public bool IsProfileItemType { get; set; }

        /// <summary>
        /// This flag causes ContentItems of the type to be deleted after their Erasure process
        /// has been handled.
        /// </summary>
        public bool DeleteItemsAfterErasure { get; set; }

        public static void SetValues(ContentTypePartDefinitionBuilder builder, GDPRPartTypeSettings settings) {
            builder.WithSetting(
                "GDPRPartTypeSettings.IsProfileItemType",
                settings.IsProfileItemType.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "GDPRPartTypeSettings.DeleteItemsAfterErasure",
                settings.DeleteItemsAfterErasure.ToString(CultureInfo.InvariantCulture));
        }
    }
}