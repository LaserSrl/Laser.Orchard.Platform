using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    [OrchardFeature("Laser.Orchard.StartupConfig.TinyMceEnhancement")]
    public class TinyMceSiteSettingsDriver : ContentPartDriver<TinyMceSiteSettingsPart> {
        private const string templateName = "Parts/TinyMceSiteSettings";
        private readonly ITinyMceEnhancementService _tinyMceEnhancementService;
        public Localizer T { get; set; }
        protected override string Prefix { get { return "TinyMceSettings"; } }

        public TinyMceSiteSettingsDriver(ITinyMceEnhancementService tinyMceEnhancementService) {
            _tinyMceEnhancementService = tinyMceEnhancementService;
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(TinyMceSiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(TinyMceSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            part.DefaultInitScript = _tinyMceEnhancementService.GetDefaultInitScript();
            return ContentShape("Parts_TinyMceSiteSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: templateName, Model: part, Prefix: Prefix);
            }).OnGroup("TinyMce");
        }
    }
}