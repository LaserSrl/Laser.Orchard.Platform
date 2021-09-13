using Laser.Orchard.AdvancedSettings.Models;
using Laser.Orchard.AdvancedSettings.Services;
using Laser.Orchard.AdvancedSettings.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.Drivers {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsPartDriver : ContentPartDriver<ThemeSkinsPart> {
        private readonly IThemeSkinsService _themeSkinsService;

        public ThemeSkinsPartDriver(
            IThemeSkinsService themeSkinsService) {

            _themeSkinsService = themeSkinsService;

            T = NullLocalizer.Instance;
        }


        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "ThemeSkinsPart"; }
        }

        protected override DriverResult Editor(ThemeSkinsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_ThemeSkinsPart_Edit",
                () => {
                    var vm = new ThemeSkinsPartEditViewModel();
                    vm.AvailableSkinNames = _themeSkinsService.GetSkinNames();
                    vm.SelectedSkinName = part.SkinName;
                    return shapeHelper.EditorTemplate(
                         TemplateName: "Parts/ThemeSkinsPart",
                         Model: vm,
                         Prefix: Prefix);
                });
        }

        protected override DriverResult Editor(ThemeSkinsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return null; //base.Editor(part, updater, shapeHelper);
        }

        protected override void Importing(ThemeSkinsPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }
            part.SkinName = context.Attribute(part.PartDefinition.Name, "SkinName");
        }

        protected override void Exporting(ThemeSkinsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("SkinName", part.SkinName);
        }
    }
}