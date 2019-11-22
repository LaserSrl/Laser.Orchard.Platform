using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Web;
using System.Web.Routing;
using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Contrib.Widgets.Settings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Mvc.Routes;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Zones;

namespace Contrib.Widgets.Handlers {
    public class WidgetsContainerPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IWidgetManager _widgetManager;
        private readonly ILocalizationService _localizationService;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IShapeFactory _shapeFactory;
        private readonly RequestContext _requestContext;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly ShellSettings _shellSettings;
        private readonly UrlPrefix _urlPrefix;
        public Localizer T { get; set; }

        public WidgetsContainerPartHandler(
            IContentManager contentManager,
            IWidgetManager widgetManager,
            ILocalizationService localizationService,
            Lazy<IEnumerable<IContentHandler>> handlers,
            IShapeFactory shapeFactory,
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider,
            ShellSettings shellSettings) {
            _contentManager = contentManager;
            _widgetManager = widgetManager;
            _localizationService = localizationService;
            _handlers = handlers;
            _shapeFactory = shapeFactory;
            _requestContext = requestContext;
            _virtualPathProvider = virtualPathProvider;
            _shellSettings = shellSettings;
            if (!string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                _urlPrefix = new UrlPrefix(_shellSettings.RequestUrlPrefix);
            T = NullLocalizer.Instance;

            OnRemoved<WidgetsContainerPart>((context, part) => {
                DeleteWidgets(part);
            });
            OnUpdateEditorShape<WidgetsContainerPart>((context, part) => {
                var lPart = part.ContentItem.As<LocalizationPart>();
                if(lPart != null) {
                    var settings = part.Settings.GetModel<WidgetsContainerSettings>();
                    if (settings.TryToLocalizeItems) {
                        var culture = lPart.Culture;
                        var widgets = _widgetManager.GetWidgets(part.ContentItem.Id, part.ContentItem.IsPublished());
                        foreach (var widget in widgets) {
                            // create build context shape for the widget
                            dynamic itemShape = CreateItemShape("Content_Edit");
                            itemShape.ContentItem = widget.ContentItem;
                            // trigger this handler to manage taxonomy fileds
                            var ctx1 = new BuildEditorContext(itemShape, widget.ContentItem, "", _shapeFactory);
                            _handlers.Value.Invoke(handler => handler.BuildEditor(ctx1), Logger);

                            // set localization of current content item AFTER invoking BuildEditor on it otherwise BuildEditor does not translate taxonomy fileds
                            _localizationService.SetContentCulture(widget.ContentItem, culture.Culture);

                            // trigger this handler to manage content picker fields and media library picker fields
                            // parameter updater is empty to avoid unwanted changes and validations on fields
                            var ctx2 = new UpdateEditorContext(
                                ctx1.Shape, 
                                widget.ContentItem, 
                                new EmptyUpdater(), 
                                "", 
                                _shapeFactory, 
                                context.ShapeTable, 
                                GetPath());
                            _handlers.Value.Invoke(handler => handler.UpdateEditor(ctx2), Logger);

                            widget.ContentItem.VersionRecord.Published = false;
                            _contentManager.Publish(widget.ContentItem);
                        }
                    }
                }
            });
        }

        private dynamic CreateItemShape(string actualShapeType) {
            var zoneHolding = new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty()));
            zoneHolding.Metadata.Type = actualShapeType;
            return zoneHolding;
        }

        /// <summary>
        /// Gets the current app-relative path, i.e. ~/my-blog/foo.
        /// </summary>
        private string GetPath() {
            var appRelativePath = _virtualPathProvider.ToAppRelative(_requestContext.HttpContext.Request.Path);
            // If the tenant has a prefix, we strip the tenant prefix away.
            if (_urlPrefix != null)
                appRelativePath = _urlPrefix.RemoveLeadingSegments(appRelativePath);

            return VirtualPathUtility.AppendTrailingSlash(appRelativePath);
        }

        private void DeleteWidgets(WidgetsContainerPart part) {
            var contentItem = part.ContentItem;

            var widgets = _widgetManager.GetWidgets(contentItem.Id, false);
            foreach (var w in widgets) {
                _contentManager.Remove(w.ContentItem);
            }
        }
        private class EmptyUpdater : IUpdateModel {
            public void AddModelError(string key, LocalizedString errorMessage) {
            }

            public bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class {
                return true;
            }
        }
    }
}