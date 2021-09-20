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
using System.Web.Mvc;

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
                    vm.SelectedSkinName = part.SkinName;
                    PopulateVMOptions(vm);
                    PopulateVMVariables(part, vm);
                    return shapeHelper.EditorTemplate(
                         TemplateName: "Parts/ThemeSkinsPart",
                         Model: vm,
                         Prefix: Prefix);
                });
        }

        protected override DriverResult Editor(ThemeSkinsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vm = new ThemeSkinsPartEditViewModel();
            if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                // TODO validate variables in the vm
                part.SkinName = vm.SelectedSkinName;
                part.Variables = vm.Variables;
            }
            return ContentShape("Parts_ThemeSkinsPart_Edit",
                () => {
                    PopulateVMOptions(vm);
                    return shapeHelper.EditorTemplate(
                         TemplateName: "Parts/ThemeSkinsPart",
                         Model: vm,
                         Prefix: Prefix);
                });
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

        private void PopulateVMOptions(ThemeSkinsPartEditViewModel vm) {
            vm.AvailableSkinNames = _themeSkinsService.GetSkinNames();
            var options = new List<SelectListItem>();
            // the manifest may have a skin named Default, that would then be the one
            // that should be used when nothing is selected.
            var exclude = new List<string>();
            var defaultSkin = vm.AvailableSkinNames.FirstOrDefault(s => s.Equals("default", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(defaultSkin)) {
                options.Add(new SelectListItem {
                    Text = defaultSkin,
                    Value = defaultSkin,
                    Selected = string.IsNullOrWhiteSpace(vm.SelectedSkinName) 
                        || defaultSkin.Equals(vm.SelectedSkinName, StringComparison.OrdinalIgnoreCase)
                });
                exclude.Add(defaultSkin);
            } else {
                options.Add(new SelectListItem {
                    Text = T("Default").Text,
                    Value = string.Empty,
                    Selected = string.IsNullOrWhiteSpace(vm.SelectedSkinName)
                });
            }
            if (vm.AvailableSkinNames != null) {
                options.AddRange(vm.AvailableSkinNames
                    .Except(exclude)
                    .Select(x => new SelectListItem {
                        Text = x,
                        Value = x,
                        Selected = string.Equals(x, vm.SelectedSkinName)
                    }));
            }
            vm.Options = options;
        }

        private void PopulateVMVariables(ThemeSkinsPart part, ThemeSkinsPartEditViewModel vm) {
            var variables = _themeSkinsService.GetSkinVariables();
            foreach (var savedVariable in part.Variables) {
                var fromTheme = variables.FirstOrDefault(v => v.Name.Equals(savedVariable.Name));
                if (fromTheme != null) {
                    fromTheme.Value = savedVariable.Value;
                }
            }
            vm.Variables = variables.ToArray();
        }
    }
}