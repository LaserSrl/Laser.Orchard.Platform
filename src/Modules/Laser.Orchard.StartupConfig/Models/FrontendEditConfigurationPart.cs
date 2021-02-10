using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.FrontendEditorConfiguration")]
    public class FrontendEditConfigurationPart : ContentPart {
        // attach this part to contenttypes we wish to edit on frontend with
        // a custom form. This part will allow control over which fields should
        // editable on frontend.
    }
}