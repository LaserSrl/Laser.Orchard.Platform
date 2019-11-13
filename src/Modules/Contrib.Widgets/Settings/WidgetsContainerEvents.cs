using Contrib.Widgets.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Themes.Services;
using Orchard.Widgets.Services;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Contrib.Widgets.Settings {
    public class WidgetsContainerEvents : ContentDefinitionEditorEventsBase {

        private readonly ISiteThemeService _siteThemeService;
        private readonly IWidgetsService _widgetsService;

        public WidgetsContainerEvents(ISiteThemeService siteThemeService, IWidgetsService widgetsService) {
            _siteThemeService = siteThemeService;
            _widgetsService = widgetsService;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name == "WidgetsContainerPart") {
                var model = definition.Settings.GetModel<WidgetsContainerSettings>();

                var currentTheme = _siteThemeService.GetSiteTheme();

                var allowedViewModel = new WidgetsContainerSettingsViewModel();
                allowedViewModel.SelectedZones = model.AllowedZones != null ? model.AllowedZones.Split(',') : new string[] { };
                allowedViewModel.Zones = _widgetsService.GetZones(currentTheme).ToList();
                allowedViewModel.SelectedWidgets = model.AllowedWidgets != null ? model.AllowedWidgets.Split(',') : new string[] { };
                allowedViewModel.Widgets = _widgetsService.GetWidgetTypeNames().OrderBy(o => o).ToList();
                allowedViewModel.UseHierarchicalAssociation = model.UseHierarchicalAssociation;
                allowedViewModel.HierarchicalAssociationJson = model.HierarchicalAssociationJson;
                allowedViewModel.TryToLocalizeItems = model.TryToLocalizeItems;

                yield return DefinitionTemplate(allowedViewModel);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "WidgetsContainerPart") yield break;

            var allowedViewModel = new WidgetsContainerSettingsViewModel();
            if (updateModel.TryUpdateModel(allowedViewModel, "WidgetsContainerSettingsViewModel", null, null)) {
                builder.WithSetting("WidgetsContainerSettings.AllowedZones", allowedViewModel.SelectedZones != null ? string.Join(",", allowedViewModel.SelectedZones) : "");
                builder.WithSetting("WidgetsContainerSettings.AllowedWidgets", allowedViewModel.SelectedWidgets != null ? string.Join(",", allowedViewModel.SelectedWidgets) : "");
                builder.WithSetting("WidgetsContainerSettings.UseHierarchicalAssociation", allowedViewModel.UseHierarchicalAssociation.ToString());
                builder.WithSetting("WidgetsContainerSettings.HierarchicalAssociationJson", allowedViewModel.HierarchicalAssociationJson);
                builder.WithSetting("WidgetsContainerSettings.TryToLocalizeItems", allowedViewModel.TryToLocalizeItems.ToString());
            }

            yield return DefinitionTemplate(allowedViewModel);
        }
    }
}