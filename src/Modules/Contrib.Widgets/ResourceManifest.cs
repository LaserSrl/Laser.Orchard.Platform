using Orchard;
using Orchard.UI.Resources;
using Orchard.ContentManagement;
using System;

namespace Contrib.Widgets {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("TheAdminWidgetContainerStyle").SetUrl("admin.widgets-container.min.css", "admin.widgets-container.css");

           // manifest.DefineScript("ThemeAdminAdminJs").SetUrl("~/Themes/TheAdmin/scripts/admin.js");
            manifest.DefineScript("AdminWidgetContainer").SetUrl("admin.widgets-container.min.js", "admin.widgets-container.js").SetDependencies("jQueryUI_Sortable");
            manifest.DefineScript("AdminEditWidget").SetUrl("admin.edit-widget.min.js", "admin.edit-widget.js");
            manifest.DefineScript("Html5").SetUrl("html5.js");
        }
    }
}