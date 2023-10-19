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
                CloneWidgets(viewModel, part.ContentItem);

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



        //protected override void Imported(WidgetsContainerPart part, ImportContentContext context) {
        //    var hostId = context.Attribute(part.PartDefinition.Name, "HostId");
        //    if (hostId != null) {
        //        CloneWidgets(Convert.ToInt32(hostId), part.ContentItem);
        //    }
        //}

        //protected override void Exporting(WidgetsContainerPart part, ExportContentContext context) {
        //    // memorizzo l'id della pagina sorgente
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("HostId", part.Id.ToString());
        //}

        #region [ Clone Functionality ]

        private void CloneWidgets(WidgetsContainerViewModel viewModel, ContentItem hostContentItem) {
            CloneWidgets(viewModel.CloneFrom, hostContentItem);
        }

        private void CloneWidgets(int originalContentId, ContentItem destinationContentItem) {
            if (originalContentId <= 0) return;

            // recupero i WidgetExPart del Content selezionato come Master (CloneFrom)
            var widgets = _widgetManager.GetWidgets(originalContentId);
            foreach (var widget in widgets) {

                // recupero il content widget nel ContentItem Master
                //var widgetPart = _widgetsService.GetWidget(widget.Id);

                // Clono il ContentMaster e recupero la parte WidgetExPart
                var clonedContentitem = _contentManager.Clone(widget.ContentItem);

                var widgetExPart = clonedContentitem.As<WidgetExPart>();

                // assegno il nuovo contenitore se non nullo ( nel caso di HtmlWidget per esempio la GetWidget ritorna nullo...)
                if (widgetExPart != null) {
                    widgetExPart.Host = destinationContentItem;
                    _contentManager.Publish(widgetExPart.ContentItem);
                }

            }

        }

        private void CloneWidgets(ContentItem original, ContentItem destination) {
            if (original == null || destination == null) return;

            // recupero i WidgetExPart del Content selezionato come Master (CloneFrom)
            var widgets = _widgetManager.GetWidgets(original.Id, original.IsPublished());
            foreach (var widget in widgets) {
                // Clono il ContentMaster e recupero la parte WidgetExPart
                var clonedContentitem = _contentManager.Clone(widget.ContentItem);

                var widgetExPart = clonedContentitem.As<WidgetExPart>();

                // assegno il nuovo contenitore se non nullo ( nel caso di HtmlWidget per esempio la GetWidget ritorna nullo...)
                if (widgetExPart != null) {
                    widgetExPart.Host = destination;
                    _contentManager.Publish(widgetExPart.ContentItem);
                }

                // se il widget ha una LocalizationPart, la gestisco
                var clonedLocalization = clonedContentitem.As<LocalizationPart>();
                if (clonedLocalization != null) {
                    clonedLocalization.Culture = destination.As<LocalizationPart>().Culture;
                    var originalLocalization = widget.ContentItem.As<LocalizationPart>();
                    if (originalLocalization.MasterContentItem == null) {
                        clonedLocalization.MasterContentItem = widget.ContentItem;
                    }
                    else {
                        clonedLocalization.MasterContentItem = originalLocalization.MasterContentItem;
                    }
                }
            }
        }
        #endregion

        protected override void Cloning(WidgetsContainerPart originalPart, WidgetsContainerPart clonePart, CloneContentContext context) {
            CloneWidgets(context.ContentItem, context.CloneContentItem);
        }
    }
}