using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using System.Globalization;

namespace Laser.Orchard.GDPR.Settings {
    /// <summary>
    /// This class represents additional settings that control specialized anonymization 
    /// and erasure behaviours for MediaLibraryPickerFields
    /// </summary>
    [OrchardFeature("Laser.Orchard.GDPR.MediaExtension")]
    public class MediaLibraryPickerFieldGDPRPartFieldSettings {

        /// <summary>
        /// Flag that tells whether we should attempt to anonymize the items selected in the
        /// MediaLibraryPickerField after we anonymize it.
        /// </summary>
        public bool AttemptToAnonymizeItems { get; set; }

        /// <summary>
        /// If this flag is set, after anonymizing the ContentItem for the MediaLibraryPickerField and
        /// eventually its selected items, we will unselect those items that have a GDPRPart.
        /// </summary>
        public bool DetachGDPRItemsOnAnonymize { get; set; }

        /// <summary>
        /// If this flag is set, after anonymizing the ContentItem for the MediaLibraryPickerField and
        /// eventually its selected items, we will unselect all items.
        /// </summary>
        public bool DetachAllItemsOnAnonymize { get; set; }

        /// <summary>
        /// Flag that tells whether we should attempt to erase the items selected in the
        /// MediaLibraryPickerField after we erase it.
        /// </summary>
        public bool AttemptToEraseItems { get; set; }

        /// <summary>
        /// If this flag is set, after erasing the ContentItem for the MediaLibraryPickerField and
        /// eventually its selected items, we will unselect those items that have a GDPRPart.
        /// </summary>
        public bool DetachGDPRItemsOnErase { get; set; }

        /// <summary>
        /// If this flag is set, after erasing the ContentItem for the MediaLibraryPickerField and
        /// eventually its selected items, we will unselect all items.
        /// </summary>
        public bool DetachAllItemsOnErase { get; set; }

        public static void SetValues(
            ContentPartFieldDefinitionBuilder builder,
            MediaLibraryPickerFieldGDPRPartFieldSettings settings) {
            // anonymization settings
            builder.WithSetting(
                "MediaLibraryPickerFieldGDPRPartFieldSettings.AttemptToAnonymizeItems",
                settings.AttemptToAnonymizeItems.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "MediaLibraryPickerFieldGDPRPartFieldSettings.DetachGDPRItemsOnAnonymize",
                settings.DetachGDPRItemsOnAnonymize.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "MediaLibraryPickerFieldGDPRPartFieldSettings.DetachAllItemsOnAnonymize",
                settings.DetachAllItemsOnAnonymize.ToString(CultureInfo.InvariantCulture));
            // erasure settings
            builder.WithSetting(
                "MediaLibraryPickerFieldGDPRPartFieldSettings.AttemptToEraseItems",
                settings.AttemptToEraseItems.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "MediaLibraryPickerFieldGDPRPartFieldSettings.DetachGDPRItemsOnErase",
                settings.DetachGDPRItemsOnErase.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "MediaLibraryPickerFieldGDPRPartFieldSettings.DetachAllItemsOnErase",
                settings.DetachAllItemsOnErase.ToString(CultureInfo.InvariantCulture));
        }
    }
}