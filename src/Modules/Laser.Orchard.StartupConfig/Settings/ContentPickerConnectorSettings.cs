using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Settings {
    [OrchardFeature("Laser.Orchard.StartupConfig.RelationshipsEnhancer")]
    public class ContentPickerConnectorSettings {
        public string FieldsFilter { get; set; }
        public void Build(ContentTypePartDefinitionBuilder builder) {
            builder.WithSetting("ContentPickerConnectorSettings.FieldsFilter", FieldsFilter);
        }

    }
}