using Newtonsoft.Json;
using Orchard.ContentManagement.MetaData.Builders;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Laser.Orchard.GDPR.Settings {
    /// <summary>
    /// This class represents settings for ContentFields that belong to a ContentType
    /// that also includes a GDPRPart.
    /// </summary>
    public class GDPRPartFieldSettings {

        public GDPRPartFieldSettings() {
            AnonymizationPropertyValuePairs = new Dictionary<string, string>();
            ErasurePropertyValuePairs = new Dictionary<string, string>();
        }

        /// <summary>
        /// Flag that tells whether the field should undergo anonymization.
        /// </summary>
        public bool ShouldAnonymize { get; set; }

        /// <summary>
        /// Flag that tells whether the field should undergo erasure.
        /// </summary>
        public bool ShouldErase { get; set; }

        /// <summary>
        /// Dictionary for Property-Value pairs used while performing erasure with the
        /// default handler based on reflection.
        /// </summary>
        public Dictionary<string, string> ErasurePropertyValuePairs { get; set; }

        /// <summary>
        /// Serialization of the dictionary, so that we may carry the settings in the definitions.
        /// </summary>
        public string ErasureSerializedPairs {
            get { return JsonConvert.SerializeObject(ErasurePropertyValuePairs); }
            set {
                try {
                    ErasurePropertyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                } catch (Exception) {
                }
            }
        }

        /// <summary>
        /// Dictionary for Property-Value pairs used while performing Anonymization with the
        /// default handler based on reflection.
        /// </summary>
        public Dictionary<string, string> AnonymizationPropertyValuePairs { get; set; }

        /// <summary>
        /// Serialization of the dictionary, so that we may carry the settings in the definitions.
        /// </summary>
        public string AnonymizationSerializedPairs {
            get { return JsonConvert.SerializeObject(AnonymizationPropertyValuePairs); }
            set {
                try {
                    AnonymizationPropertyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                } catch (Exception) {
                }
            }
        }

        public static void SetValues(ContentPartFieldDefinitionBuilder builder, GDPRPartFieldSettings settings) {
            builder.WithSetting(
                "GDPRPartFieldSettings.ShouldAnonymize",
                settings.ShouldAnonymize.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting(
                "GDPRPartFieldSettings.ShouldErase",
                settings.ShouldErase.ToString(CultureInfo.InvariantCulture));
            // serialized dictionaries
            builder.WithSetting(
                "GDPRPartFieldSettings.AnonymizationSerializedPairs",
                settings.AnonymizationSerializedPairs);
            builder.WithSetting(
                "GDPRPartFieldSettings.ErasureSerializedPairs",
                settings.ErasureSerializedPairs);
        }
    }
}