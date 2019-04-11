using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.Services;
using Laser.Orchard.SEO.ViewModels;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;


namespace Laser.Orchard.SEO.Drivers {


    [OrchardFeature("Laser.Orchard.Favicon")]
    public class FaviconSettingsPartDriver : ContentPartDriver<FaviconSettingsPart> {


        private readonly ISignals _signals;


        public FaviconSettingsPartDriver(ISignals signals, IFaviconService faviconService) {
            _signals = signals;
        }


        protected override string Prefix { get { return "FaviconSettings"; } }


        protected override DriverResult Editor(FaviconSettingsPart part, dynamic shapeHelper) {

            return ContentShape("Parts_Favicon_FaviconSettings",
                                () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/Favicon.FaviconSettings",
                                   Model: new FaviconSettingsViewModel {
                                       FaviconUrl = part.FaviconUrl
                                   },
                                   Prefix: Prefix)).OnGroup("Favicon");
        }


        protected override DriverResult Editor(FaviconSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var faviconSettingsViewModel = new FaviconSettingsViewModel {
                FaviconUrl = part.FaviconUrl
            };
            if (updater.TryUpdateModel(faviconSettingsViewModel, Prefix, null, null)) {
                part.FaviconUrl = faviconSettingsViewModel.FaviconUrl;
                _signals.Trigger("Laser.Orchard.Favicon.Changed");
            }
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(FaviconSettingsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("FaviconUrl", part.FaviconUrl);
        }
        protected override void Importing(FaviconSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            if (root == null) {
                return;
            }
            var importedFaviconUrl = context.Attribute(part.PartDefinition.Name, "FaviconUrl");
            if (importedFaviconUrl != null) {
                part.FaviconUrl = importedFaviconUrl;
            }
        }
    }
}