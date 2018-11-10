using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement.Drivers {
    [OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class SiteSettingsPartDriver : ContentPartDriver<SiteSettingsPart> {
        private readonly IOrchardServices _orchardServices;

        public SiteSettingsPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
    }
        protected override string Prefix {
            get { return "Laser.Template.Settings"; }
        }

        protected override DriverResult Editor(SiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);

        }

        protected override DriverResult Editor(SiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            return ContentShape("Parts_TemplateSettings_Edit", () => {
                var viewModel = new TemplateSettingsViewModel();
                var getpart = _orchardServices.WorkContext.CurrentSite.As<SiteSettingsPart>();
                viewModel.ParserIdSelected = getpart.DefaultParserIdSelected;
                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                     //   if (viewModel.ParserId != null) {
                        part.DefaultParserIdSelected = viewModel.ParserIdSelected;
                    //    }
                    }
                } else {
                    viewModel.ParserIdSelected = part.DefaultParserIdSelected;
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts/TemplateSettings_Edit", Model: viewModel, Prefix: Prefix);
            })
                .OnGroup("Template");
        }

        protected override void Importing(SiteSettingsPart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "DefaultParserEngine", x => part.DefaultParserIdSelected = x);
        }

        protected override void Exporting(SiteSettingsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("DefaultParserEngine", part.DefaultParserIdSelected);
        }
    }
}