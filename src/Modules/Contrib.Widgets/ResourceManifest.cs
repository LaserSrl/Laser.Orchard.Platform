using Orchard;
using Orchard.UI.Resources;
using Orchard.ContentManagement;
using System;

namespace Contrib.Widgets {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("TheAdminWidgetContainerStyle").SetUrl("admin.widgets-container.min.css");

           // manifest.DefineScript("ThemeAdminAdminJs").SetUrl("~/Themes/TheAdmin/scripts/admin.js");
            manifest.DefineScript("AdminWidgetContainer").SetUrl("admin.widgets-container.min.js").SetDependencies("jQueryUI_Sortable");
            manifest.DefineScript("AdminEditWidget").SetUrl("admin.edit-widget.min.js");
            manifest.DefineScript("Html5").SetUrl("html5.js");
        }
    }
}