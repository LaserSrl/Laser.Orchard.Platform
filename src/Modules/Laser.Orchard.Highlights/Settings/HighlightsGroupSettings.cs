using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Highlights.Settings {
    public class HighlightsGroupSettings {
        public string ShapeName { get; set; }
        public string ShapeDescription { get; set; }
        public IList<string> AvailablePlugins { get; set; }
        public void Build(ContentTypePartDefinitionBuilder builder) {
            builder.WithSetting("HighlightsGroupSettings.ShapeName", ShapeName);
            builder.WithSetting("HighlightsGroupSettings.ShapeDescription", ShapeDescription);
        }

    }
}