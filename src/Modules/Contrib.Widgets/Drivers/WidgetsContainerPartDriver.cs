using System;
using System.Linq;
using System.Xml;
using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Contrib.Widgets.ViewModels;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Themes.Services;
using Orchard.Widgets.Services;
using Contrib.Widgets.Settings;
using Orchard.Security;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Runtime.Remoting.Contexts;
using System.Web.UI.WebControls.WebParts;

namespace Contrib.Widgets.Drivers {
    [OrchardFeature("Contrib.Widgets")]
    public class WidgetsContainerPartDriver : ContentPartCloningDriver<WidgetsContainerPart> {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IWidgetsService _widgetsService;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IWidgetManager _widgetManager;
        private readonly IWorkContextAccessor _wca;
        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        private readonly ICultureManager _cultureManager;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public WidgetsContainerPartDriver(
            ISiteThemeService siteThemeService,
            IWidgetsService widgetsService,
            IVirtualPathProvider virtualPathProvider,
            IShapeFactory shapeFactory,
            IWidgetManager widgetManager,
            IWorkContextAccessor wca,
            IContentManager contentManager,
            ILocalizationService localizationService,
            ICultureManager cultureManager,
            IAuthorizer authorizer,
            INotifier notifier) {

            _siteThemeService = siteThemeService;
            _widgetsService = widgetsService;
            _virtualPathProvider = virtualPathProvider;
            New = shapeFactory;
            _widgetManager = widgetManager;
            _wca = wca;
            _contentManager = contentManager;
            _localizationService = localizationService;
            _cultureManager = cultureManager;
            _authorizer = authorizer;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private dynamic New { get; set; }

        protected override string Prefix {
            get { return "WidgetsContainer"; }
        }

        protected override DriverResult Display(WidgetsContainerPart part, string displayType, dynamic shapeHelper) {
            // TODO: make DisplayType configurable
            if (displayType != "Detail")
                return null;

            var settings = part.Settings.GetModel<WidgetsContainerSettings>();

            var widgetParts = _widgetManager.GetWidgets(part.Id, part.ContentItem.IsPublished());

            if (!settings.UseHierarchicalAssociation) {
                if (!string.IsNullOrWhiteSpace(settings.AllowedWidgets))
                    widgetParts = widgetParts.Where(x => settings.AllowedWidgets.Split(',').Contains(x.TypeDefinition.Name));
                if (!string.IsNullOrWhiteSpace(settings.AllowedZones))
                    widgetParts = widgetParts.Where(x => settings.AllowedZones.Split(',').Contains(x.Zone));
            }
            else if (settings.HierarchicalAssociation != null && settings.HierarchicalAssociation.Count() > 0) {
                widgetParts = widgetParts.Where(wp => settings.HierarchicalAssociation.Any(z => z.ZoneName.Equals(wp.Zone) && z.Widgets.Any(w => w.WidgetType == wp.TypeDefinition.Name || w.WidgetType == "All")));
            }
            // Build and add shape to zone.
            var workContext = _wca.GetContext();
            var zones = workContext.Layout.Zones;
            foreach (var widgetPart in widgetParts) {
                var widgetShape = _contentManager.BuildDisplay(widgetPart);
                zones[widgetPart.Zone].Add(widgetShape, widgetPart.Position);
            }

            return null;
        }

        protected override DriverResult Editor(WidgetsContainerPart part, dynamic shapeHelper) {
            return ContentShape("Parts_WidgetsContainer", () => {
                if (!_authorizer.Authorize(Permissions.ManageContainerWidgets)) {
                    return null;
                }
                var settings = part.Settings.GetModel<WidgetsContainerSettings>();

                var currentTheme = _siteThemeService.GetSiteTheme();
                var currentThemesZones = _widgetsService.GetZones(currentTheme).ToList();
                var widgetTypes = _widgetsService.GetWidgetTypeNames().ToList();
                if (!settings.UseHierarchicalAssociation) {
                    if (!string.IsNullOrWhiteSpace(settings.AllowedZones))
                        currentThemesZones = currentThemesZones.Where(x => settings.AllowedZones.Split(',').Contains(x)).ToList();
                    if (!string.IsNullOrWhiteSpace(settings.AllowedWidgets))
                        widgetTypes = widgetTypes.Where(x => settings.AllowedWidgets.Split(',').Contains(x)).ToList();
                }
                else if (settings.HierarchicalAssociation != null && settings.HierarchicalAssociation.Count() > 0) {
                    currentThemesZones = currentThemesZones.Where(ctz => settings.HierarchicalAssociation.Select(x => x.ZoneName).Contains(ctz)).ToList();
                    widgetTypes = widgetTypes.Where(w => settings.HierarchicalAssociation.Any(x => x.Widgets.Any(a => a.WidgetType == w || a.WidgetType == "All"))).ToList();
                }

                var widgets = _widgetManager.GetWidgets(part.Id, false);

                var zonePreviewImagePath = string.Format("{0}/{1}/ThemeZonePreview.png", currentTheme.Location, currentTheme.Id);
                var zonePreviewImage = _virtualPathProvider.FileExists(zonePreviewImagePath) ? zonePreviewImagePath : null;

                var layer = _widgetsService.GetLayers().First();

                // recupero i contenuti localizzati una try è necessaria in quanto non è detto che un contenuto sia localizzato
                dynamic contentLocalizations;
                try {
                    contentLocalizations = _localizationService
                        .GetLocalizations(part.ContentItem, VersionOptions.Latest) //the other cultures
                        .Where(lp => //as long as a culture has been assigned
                            lp.Culture != null && !string.IsNullOrWhiteSpace(lp.Culture.Culture))
                        .OrderBy(o => o.Culture.Culture)
                        .ToList();
                }
                catch {
                    contentLocalizations = null;
                }

                var viewModel = New.ViewModel()
                    .CurrentTheme(currentTheme)
                    .Zones(currentThemesZones)
                    .ContentLocalizations(contentLocalizations)
                    .ZonePreviewImage(zonePreviewImage)
                    .WidgetTypes(widgetTypes)
                    .Widgets(widgets)
                    .ContentItem(part.ContentItem)
                    .LayerId(layer.Id)
                    .CloneFrom(0);

                return shapeHelper.EditorTemplate(TemplateName: "Parts.WidgetsContainer", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(WidgetsContainerPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new WidgetsContainerViewModel();
            if (updater.TryUpdateModel(viewModel, null, null, null)) {
                UpdatePositions(viewModel);
                RemoveWidgets(viewModel);
                CloneWidgets(viewModel.CloneFrom >= 0 ? _contentManager.Get(viewModel.CloneFrom, VersionOptions.Latest) : null, part.ContentItem, part.Settings.GetModel<WidgetsContainerSettings>()?.TryToLocalizeItems);

            }

            return Editor(part, shapeHelper);
        }



        private void RemoveWidgets(WidgetsContainerViewModel viewModel) {
            if (string.IsNullOrEmpty(viewModel.RemovedWidgets))
                return;

            var widgetIds = JsonConvert.DeserializeObject<int[]>(viewModel.RemovedWidgets);

            var unableToDeleteSome = false;
            foreach (var widgetId in widgetIds) {
                // make sure that the user is allowed to delete the widget.
                // Doing this check here handles cases where the UI is not aligned with the 
                // configuration, because the latter changed but the former hasn't updated
                // yet.
                var currentWidget = _widgetsService.GetWidget(widgetId);
                if (currentWidget != null && _authorizer.Authorize(
                    Orchard.Core.Contents.Permissions.DeleteContent, currentWidget)) {
                    _widgetsService.DeleteWidget(widgetId);
                }
                else {
                    // If current widget is null, it means it has already been removed (perhaps a concurrent delete call - e.g. when two browser tabs are open?).
                    // It is not a permission issue, so nothing should really be notified to the user.
                    unableToDeleteSome = (currentWidget != null);
                }
            }
            if (unableToDeleteSome) {
                _notifier.Warning(T("You don't have the permissions to remove some of the widgets you attempted to delete."));
            }
        }

        private void UpdatePositions(WidgetsContainerViewModel viewModel) {
            if (string.IsNullOrEmpty(viewModel.WidgetPlacement))
                return;

            var data = JsonConvert.DeserializeXNode(viewModel.WidgetPlacement);
            var zonesNode = data.Root;

            foreach (var zoneNode in zonesNode.Elements()) {
                var zoneName = zoneNode.Name.ToString();
                var widgetElements = zoneNode.Elements("widgets");
                var position = 0;

                foreach (var widget in widgetElements
                    .Select(widgetNode => XmlConvert.ToInt32(widgetNode.Value))
                    .Select(widgetId => _widgetsService.GetWidget(widgetId))
                    .Where(widget => widget != null)) {

                    widget.Position = (position++).ToString();
                    widget.Zone = zoneName;
                }
            }
        }

        #region [ Clone Functionality ]

        private void CloneWidgets(ContentItem source, ContentItem destination, bool? tryLocalizeitems) {
            if (source == null || destination == null) return;

            // recupero i WidgetExPart del Content selezionato come source
            var widgets = _widgetManager.GetWidgets(source.Id, source.IsPublished());
            var originalLocalization = source.As<LocalizationPart>();
            var destinationLocalization = destination.As<LocalizationPart>();
            foreach (var widget in widgets) {
                ContentItem clonedWidget;
                // if we need to keep the localization in sync, we try to do it
                if (tryLocalizeitems ?? false) {
                    // Checks if original and destination have a different culture, and if yes tries to keep localizations of widgets in sync
                    if (originalLocalization != null && destinationLocalization != null && originalLocalization.Culture?.Culture != destinationLocalization.Culture?.Culture) {
                        var translatedWidget = _localizationService.GetLocalizedContentItem(widget, destinationLocalization.Culture?.Culture)?.ContentItem;
                        // If there is not a localizaed widget
                        //  We clone the widget and we assign the right culture if possible
                        // else 
                        // we use the localizaed widget
                        if (translatedWidget == null) {
                            clonedWidget = _contentManager.Clone(widget.ContentItem);
                            var clonedWidgetLocalizationPart = clonedWidget.As<LocalizationPart>();
                            if (clonedWidgetLocalizationPart != null) {
                                clonedWidgetLocalizationPart.Culture.Culture = destinationLocalization.Culture.Culture;
                            }
                        }
                        else {
                            clonedWidget = translatedWidget;
                        }
                    }
                    else {
                        // We clone the widget, but if we are translating a culture we clone only the orginal culture
                        if (IsTranslating()) {
                            var originalWidgetLocalization = widget.As<LocalizationPart>();
                            if (originalWidgetLocalization == null) {
                                clonedWidget = _contentManager.Clone(widget.ContentItem);
                            }
                            else if (originalWidgetLocalization != null &&
                                originalWidgetLocalization.Culture != null &&
                                originalWidgetLocalization.Culture.Culture == originalLocalization.Culture?.Culture) {
                                clonedWidget = _contentManager.Clone(widget.ContentItem);
                            }
                            else { //skip this widget from cloning and skip the subsequent processing
                                continue;
                            }
                        }
                        else {
                            clonedWidget = _contentManager.Clone(widget.ContentItem);
                        }
                    }
                }
                else {
                    clonedWidget = _contentManager.Clone(widget.ContentItem);
                }
                var widgetExPart = clonedWidget.As<WidgetExPart>();

                // assegno il nuovo contenitore se non nullo ( nel caso di HtmlWidget per esempio la GetWidget ritorna nullo...)
                if (widgetExPart != null) {
                    widgetExPart.Host = destination;
                    _contentManager.Publish(widgetExPart.ContentItem);
                }
                if (tryLocalizeitems ?? false) {
                    var clonedWisgetLocalization = clonedWidget.As<LocalizationPart>();
                    // if the widegt has a LocalizationPart, we manage it
                    if (clonedWisgetLocalization != null) {
                        if (destinationLocalization != null) {
                            clonedWisgetLocalization.Culture.Culture = destinationLocalization.Culture.Culture;
                        }
                        var originalWidgetLocalization = widget.ContentItem.As<LocalizationPart>();
                        //We need to manage the MasterContentItem only if we are translating the widget;
                        //On the contrary if we are cloning it, we have nothing to do.
                        if (IsTranslating()) {
                            if (originalWidgetLocalization.MasterContentItem == null) {
                                clonedWisgetLocalization.MasterContentItem = widget.ContentItem;
                            }
                            else {
                                clonedWisgetLocalization.MasterContentItem = originalWidgetLocalization.MasterContentItem;
                            }
                        }
                    }
                }
            }
        }

        private bool IsTranslating() {
            var routeData = _wca.GetContext().HttpContext.Request.RequestContext.RouteData.Values;
            object action, area;

            return routeData.TryGetValue("action", out action) &&
                            routeData.TryGetValue("area", out area) &&
                            action.ToString().ToUpperInvariant() == "TRANSLATE" &&
                            area.ToString().ToUpperInvariant() == "ORCHARD.LOCALIZATION";
        }

        #endregion

        protected override void Cloning(WidgetsContainerPart originalPart, WidgetsContainerPart clonePart, CloneContentContext context) {
            CloneWidgets(context.ContentItem, context.CloneContentItem, clonePart.Settings.GetModel<WidgetsContainerSettings>()?.TryToLocalizeItems);
        }
    }
}