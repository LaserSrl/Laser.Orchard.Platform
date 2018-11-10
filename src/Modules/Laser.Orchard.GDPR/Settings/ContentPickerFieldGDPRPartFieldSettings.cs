using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using System.Globalization;

namespace Laser.Orchard.GDPR.Settings {
    /// <summary>
    /// This class represents additional settings that control specialize anonymization 
    /// and erasure behaviours for ContentPickerFields
    /// </summary>
    [OrchardFeature("Laser.Orchard.GDPR.ContentPickerFieldExtension")]
    public class ContentPickerFieldGDPRPartFieldSettings {

        /// <summary>
        /// Flag that tells whether we should attempt to anonymize the items selected in the
        /// ContentPickerField after we anonymize it.
        /// </summary>
        public bool AttemptToAnonymizeItems { get; set; }

        /// <summary>
        /// If this flag is set, after anonymizing the ContentItem for the ContentPickerField and
        /// eventually its selected items, we will unselect those items that have a GDPRPart.
        /// </summary>
        public bool DetachGDPRItemsOnAnonymize { get; set; }

        /// <summary>
        /// If this flag is set, after anonymizing the ContentItem for the ContentPickerField and
        /// eventually its selected items, we will unselect all items.
        /// </summary>
        public bool DetachAllItemsOnAnonymize { get; set; }

        /// <summary>
        /// Flag that tells whether we should attempt to erase the items selected in the
        /// ContentPickerField after we erase it.
        /// </summary>
        public bool AttemptToEraseItems { get; set; }

        /// <summary>
        /// If this flag is set, after erasing the ContentItem for the ContentPickerField and
        /// eventually its selected items, we will unselect those items that have a GDPRPart.
        /// </summary>
        public bool DetachGDPRItemsOnErase { get; set; }

        /// <summary>
        /// If this flag is set, after erasing the ContentItem for the ContentPickerField and
        /// eventually its selected items, we will unselect all items.
        /// </summary>
        public bool DetachAllItemsOnErase { get; set; }

        public static void SetValues(
            ContentPartFieldDefinitionBuilder builder, 
            ContentPickerFieldGDPRPartFieldSettings settings) {
            // anonymization settings
            builder.WithSetting(
                "ContentPickerFieldGDPRPartFieldSettings.AttemptToAnonymizeItems",
                settings.AttemptToAnonymizeItems.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "ContentPickerFieldGDPRPartFieldSettings.DetachGDPRItemsOnAnonymize",
                settings.DetachGDPRItemsOnAnonymize.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "ContentPickerFieldGDPRPartFieldSettings.DetachAllItemsOnAnonymize",
                settings.DetachAllItemsOnAnonymize.ToString(CultureInfo.InvariantCulture));
            // erasure settings
            builder.WithSetting(
                "ContentPickerFieldGDPRPartFieldSettings.AttemptToEraseItems",
                settings.AttemptToEraseItems.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "ContentPickerFieldGDPRPartFieldSettings.DetachGDPRItemsOnErase",
                settings.DetachGDPRItemsOnErase.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "ContentPickerFieldGDPRPartFieldSettings.DetachAllItemsOnErase",
                settings.DetachAllItemsOnErase.ToString(CultureInfo.InvariantCulture));
        }
    }
}