using Orchard.ContentManagement.MetaData.Builders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.WebServices.Settings {
    public class ContentFieldSerializationSettings {
        public bool AllowSerialization { get; set; }

        public ContentFieldSerializationSettings() {
            // Serialization allowed by default.
            AllowSerialization = true;
        }

        public static void SetValues(ContentPartFieldDefinitionBuilder builder, bool allowSerialization) {
            builder.WithSetting("ContentFieldSerializationSettings.AllowSerialization", allowSerialization.ToString(CultureInfo.InvariantCulture));
        }
    }
}