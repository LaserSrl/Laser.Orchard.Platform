using Laser.Orchard.MultiStepAuthentication.Models;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.MultiStepAuthentication.Drivers {
    [OrchardFeature("Laser.Orchard.NonceTemplateEmail")]
    public class NonceTemplateSettingsPartDriver : ContentPartDriver<NonceTemplateSettingsPart> {
        private const string TemplateName = "Parts/NonceTemplateSettings";
        private readonly IContentManager _contentManager;
        private readonly ITemplateService _templateService;
        public Localizer T { get; set; }
        protected override string Prefix { get { return "NonceTemplateSettings"; } }

        public NonceTemplateSettingsPartDriver(IContentManager contentManager, ITemplateService templateService) {
            _contentManager = contentManager;
            _templateService = templateService;
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(NonceTemplateSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(NonceTemplateSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vModel = new CustomTemplatePickerViewModel {
                TemplateIdSelected = part.SelectedTemplate != null ? part.SelectedTemplate.Id : (int?)null,
                TemplatesList = _templateService.GetTemplates()
            };
            if (updater != null) {
                if (updater.TryUpdateModel(part, Prefix, null, null)) {
                    part.SelectedTemplate = _contentManager.Get<TemplatePart>(part.ct.TemplateIdSelected.Value);
                }
            }
            part.ct = vModel;
            return ContentShape("Parts_NonceTemplateSettings_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/NonceTemplateSettings", Model: part, Prefix: Prefix)).OnGroup("NonceLoginSettings");
        }
    }
}