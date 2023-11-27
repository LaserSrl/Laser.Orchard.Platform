﻿using Orchard.UI.Resources;

namespace Laser.Orchard.StartupConfig {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle("LaserBase").SetUrl("laser-base.min.css", "laser-base.css");

            //manifest.DefineStyle("FontAwesome").SetUrl("font-awesome/css/font-awesome.min.css");
            manifest.DefineStyle("FontAwesome430").SetUrl("//maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css");
            manifest.DefineStyle("FontAwesome430.ie7").SetUrl("//maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome-ie7.min.css");
            manifest.DefineStyle("FontAwesome.5.14")
                .SetUrl("font-awesome/css/all.min.css", "font-awesome/css/all.css");

            //maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css

            // color picker
            manifest.DefineScript("spectrum").SetUrl("spectrum.js").SetDependencies("jQuery");

            // tabulator (currently v5.5.2)
            manifest.DefineScript("tabulator")
                .SetUrl("tabulator\\tabulator.min.js", "tabulator\\tabulator.js")
                .SetDependencies("jQueryUI");
            manifest.DefineStyle("tabulator")
                .SetUrl("tabulator\\tabulator.min.css", "tabulator\\tabulator.css");
            manifest.DefineStyle("tabulatorBootstrap")
                .SetUrl("tabulator\\bootstrap\\tabulator_bootstrap.min.css", "tabulator\\bootstrap\\tabulator_bootstrap.css")
                .SetDependencies("Bootstrap");

            // luxon (used by tabulator to format datetime columns)
            manifest.DefineScript("luxon")
                .SetUrl("tabulator\\luxon.min.js", "tabulator\\luxon.js");

            // content picker creation
            manifest.DefineScript("ContentPickerCreation")
                .SetUrl("contentPickerCreation\\ContentPickerCreation.js", "contentPickerCreation\\ContentPickerCreation.js")
                .SetDependencies("jQueryUI");
            manifest.DefineStyle("ContentPickerCreation")
                .SetUrl("contentPickerCreation\\ContentPickerCreation.css", "contentPickerCreation\\ContentPickerCreation.css");
            manifest.DefineScript("ContentPickerThemeHiding")
                .SetUrl("contentPickerCreation\\ContentPickerThemeHiding.js", "contentPickerCreation\\ContentPickerThemeHiding.js")
                .SetVersion("1.1")
                .SetDependencies("jQueryUI");
            manifest.DefineStyle("ContentPickerThemeHiding")
                .SetUrl("contentPickerCreation\\ContentPickerThemeHiding.css", "contentPickerCreation\\ContentPickerThemeHiding.css");

            manifest.DefineScript("DatetimeEditorFilter")
               .SetUrl("datetime-editor-filter.min.js", "datetime-editor-filter.js");
        }
    }
}