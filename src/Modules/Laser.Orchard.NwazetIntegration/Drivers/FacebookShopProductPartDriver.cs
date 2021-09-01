using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductPartDriver : ContentPartDriver<FacebookShopProductPart> {
        protected override string Prefix => "FacebookShopProductPart";

        public FacebookShopProductPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(FacebookShopProductPart part, dynamic shapeHelper) {
            return ContentShape("Parts_FacebookShopProductPart",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/FacebookShopProductPartEditor",
                    Model: new FacebookShopProductPartViewModel() {
                        SynchronizeFacebookShop = part.SynchronizeFacebookShop
                    },
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(FacebookShopProductPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new FacebookShopProductPartViewModel() {
                SynchronizeFacebookShop = part.SynchronizeFacebookShop
            };
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                part.SynchronizeFacebookShop = viewModel.SynchronizeFacebookShop;
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(FacebookShopProductPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name)
                .SetAttributeValue("SynchronizeFacebookShop", part.Record.SynchronizeFacebookShop);
        }

        protected override void Importing(FacebookShopProductPart part, ImportContentContext context) {
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            bool flag = false;
            bool.TryParse(context.Attribute(part.PartDefinition.Name, "SynchronizeFacebookShop"), out flag);
            part.SynchronizeFacebookShop = flag;
        }
    }
}