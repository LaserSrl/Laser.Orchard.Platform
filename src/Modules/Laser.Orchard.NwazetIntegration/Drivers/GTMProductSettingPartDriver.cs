using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class GTMProductSettingPartDriver : ContentPartCloningDriver<GTMProductPart> {
        private readonly IOrchardServices _orchardServices;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.NwazetIntegration"; }
        }

        public GTMProductSettingPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(GTMProductPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(GTMProductPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_GTMProductPartSettingVM", () => {
                var viewModel = new GTMProductViewModel();
                var getpart = _orchardServices.WorkContext.CurrentSite.As<GTMProductPart>();
                viewModel.Id = getpart.ProductId;
                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        part.ProductId = viewModel.Id;
                    }
                }
                else {
                    viewModel.Id = part.ProductId;
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts/GTMProductPartSettingVM", Model: viewModel, Prefix: Prefix);
            });
        }
    }
}