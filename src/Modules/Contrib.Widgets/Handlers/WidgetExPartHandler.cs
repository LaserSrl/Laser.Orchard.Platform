using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Contrib.Widgets.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.UI.Notify;
using Orchard.Widgets.Models;
using System;
using System.Linq;
using System.Web.Routing;

namespace Contrib.Widgets.Handlers {
    [OrchardFeature("Contrib.Widgets")]
    public class WidgetExPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IWidgetManager _widgetManager;
        private readonly ILocalizationService _localizationService;
        private readonly INotifier _notifier;

        public WidgetExPartHandler(IRepository<WidgetExPartRecord> repository,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IWidgetManager widgetManager,
            ILocalizationService localizationService,
            INotifier notifier) {
            Filters.Add(StorageFilter.For(repository));
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _widgetManager = widgetManager;
            _localizationService = localizationService;
            _notifier = notifier;

            T = NullLocalizer.Instance;
            OnActivated<WidgetExPart>(SetupFields);
            OnUpdated<WidgetExPart>(PublishWidget);
        }

        public Localizer T { get; set; }

        private void SetupFields(ActivatedContentContext context, WidgetExPart part) {
            part.HostField.Loader(() => part.Record.HostId != null ? _contentManager.Get(part.Record.HostId.Value, VersionOptions.Latest) : null);
            part.HostField.Setter(x => {
                part.Record.HostId = x != null ? x.Id : default(int?);
                return x;
            });
        }

        private void PublishWidget(UpdateContentContext context, WidgetExPart part) {
            if (!context.ContentItem.TypeDefinition.Settings.ContainsKey("Stereotype") || 
                context.ContentItem.TypeDefinition.Settings["Stereotype"] != "Widget" || 
                part.ContentItem.Has<IPublishingControlAspect>())
                return;

            //TODO: When the widget has been updated and the settings of the WidgetContainer tell us to keep localizations syncronized,
            //      we need to place it, coherently with its culture, within the right translated host if possible.
            if (part.Host != null) {
                var settings = part.Host.As<WidgetsContainerPart>().Settings.GetModel<WidgetsContainerSettings>();
                if (settings.TryToLocalizeItems) {
                    var hostLocalizationPart = part.Host.As<LocalizationPart>();
                    var widgetLocalizationPart = part.As<LocalizationPart>();
                    if (hostLocalizationPart != null
                        && widgetLocalizationPart != null
                        && hostLocalizationPart.Culture != null
                        && hostLocalizationPart.Culture.Culture != widgetLocalizationPart.Culture?.Culture) {

                        var hostContentForWidgetCulture = _localizationService.GetLocalizations(part.Host, VersionOptions.Latest).FirstOrDefault(x => x.Culture?.Culture == widgetLocalizationPart.Culture.Culture);
                        if (hostContentForWidgetCulture != null) {
                            part.Host = hostContentForWidgetCulture.ContentItem;
                            _notifier.Add(NotifyType.Information, T("The widget '{0}' has been moved under its '{1}' container", part.As<WidgetPart>().Title, widgetLocalizationPart.Culture.Culture));
                        }
                    }

                }
            }
            // Then we publish the widget
            _contentManager.Publish(part.ContentItem);

        }

        private bool KeepLocalizationsInSync(WidgetExPart part) {
            // TODO: read this from settings
            return true;
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var widgetExPart = context.ContentItem.As<WidgetExPart>();

            if (widgetExPart == null || widgetExPart.Host == null)
                return;

            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Contrib.Widgets"},
                {"Controller", "Admin"},
                {"Action", "EditWidget"},
                {"hostId", widgetExPart.Host.Id},
                {"Id", widgetExPart.Id}
            };

        }

        protected override void Activated(ActivatedContentContext context) {
            if (!context.ContentItem.TypeDefinition.Settings.ContainsKey("Stereotype") || context.ContentItem.TypeDefinition.Settings["Stereotype"] != "Widget")
                return;

            if (!context.ContentItem.Is<WidgetExPart>()) {
                _contentDefinitionManager.AlterTypeDefinition(context.ContentType, type => type.WithPart("WidgetExPart"));
            }
        }
    }
}