using Contrib.Widgets.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Contrib.Widgets.Drivers {
    [OrchardFeature("Contrib.Widgets")]
    public class WidgetExPartDriver : ContentPartCloningDriver<WidgetExPart> {
        protected override void Imported(WidgetExPart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "HostId", s => part.Host = context.GetItemFromSession(s));
        }

        protected override void Exporting(WidgetExPart part, ExportContentContext context) {
            if (part.Host != null)
            {
                context.Element(part.PartDefinition.Name).SetAttributeValue("HostId", context.ContentManager.GetItemMetadata(part.Host).Identity.ToString());                
            }
        }

        protected override void Cloning(WidgetExPart originalPart, WidgetExPart clonePart, CloneContentContext context) {
            clonePart.Host = originalPart.Host;
        }
    }
}