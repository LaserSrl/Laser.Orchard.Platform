using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Contrib.Widgets.Settings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.UI.Notify;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Permissions = Orchard.Widgets.Permissions;

namespace Contrib.Widgets.Controllers {
    [ValidateInput(false), OrchardFeature("Contrib.Widgets")]
    public class AdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _services;
        private readonly IWidgetsService _widgetsService;
        private readonly IWidgetManager _widgetManager;
        private readonly IContentManager _contentManager;

        public AdminController(IOrchardServices services, IWidgetsService widgetsService, IWidgetManager widgetManager, IContentManager contentManager) {
            _services = services;
            _widgetsService = widgetsService;
            _widgetManager = widgetManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult CreateContent(string id, string zone) {
            var contentItem = _contentManager.New(id);

            if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, contentItem, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            _contentManager.Create(contentItem, VersionOptions.Draft);

            var model = _contentManager.UpdateEditor(contentItem, this);
            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();
                return View(model);
            }

            _services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been created.")
                : T("Your {0} has been created.", contentItem.TypeDefinition.DisplayName));

            return RedirectToAction("ListWidgets", new { hostId = contentItem.Id, zone });
        }

        public ActionResult ListWidgets(int hostId, string zone) {
            var widgetTypes = _widgetsService.GetWidgetTypeNames().OrderBy(x => x).ToList();

            var widgetsContainerPart = _contentManager.Get(hostId, VersionOptions.Latest).As<WidgetsContainerPart>();
            var settings = widgetsContainerPart.Settings.GetModel<WidgetsContainerSettings>();

            if (!string.IsNullOrWhiteSpace(settings.AllowedWidgets))
                widgetTypes = widgetTypes.Where(x => settings.AllowedWidgets.Split(',').Contains(x)).ToList();

            var viewModel = _services.New.ViewModel()
                .WidgetTypes(widgetTypes)
                .HostId(hostId)
                .Zone(zone);

            return View(viewModel);
        }

        public ActionResult AddWidget(int hostId, string widgetType, string zone, string returnUrl) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            var widgetPart = _services.ContentManager.New<WidgetPart>(widgetType);
            if (widgetPart == null)
                return HttpNotFound();

            try {
                var widgetPosition = _widgetManager.GetWidgets(hostId).Count(widget => widget.Zone == widgetPart.Zone) + 1;
                widgetPart.Position = widgetPosition.ToString(CultureInfo.InvariantCulture);
                widgetPart.Zone = zone;
                widgetPart.AvailableLayers = _widgetsService.GetLayers().ToList();
                widgetPart.LayerPart = _widgetManager.GetContentLayer();

                var model = _services.ContentManager.BuildEditor(widgetPart).HostId(hostId);
                return View(model);
            } catch (Exception exception) {
                Logger.Error(T("Creating widget failed: {0}", exception.Message).Text);
                _services.Notifier.Error(T("Creating widget failed: {0}", exception.Message));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Edit", "Admin", new { area = "Contents" }));
            }
        }

        [HttpPost, ActionName("AddWidget")]
        [Orchard.Mvc.FormValueRequired("submit.Save")]
        public ActionResult AddWidgetPost(string widgetType, int hostId) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            var layer = _widgetsService.GetLayers().First();
            var widgetPart = _widgetsService.CreateWidget(layer.Id, widgetType, "", "", "");
            if (widgetPart == null)
                return HttpNotFound();
            else
                _contentManager.Publish(widgetPart.ContentItem);

            var contentItem = _services.ContentManager.Get(hostId, VersionOptions.Latest);

            var contentMetadata = _services.ContentManager.GetItemMetadata(contentItem);
            var returnUrl = Url.RouteUrl(contentMetadata.EditorRouteValues);
            var model = _services.ContentManager.UpdateEditor(widgetPart, this).HostId(hostId);

            var widgetExPart = widgetPart.As<WidgetExPart>();

            try {
                widgetPart.LayerPart = _widgetManager.GetContentLayer();
                widgetExPart.Host = contentItem;
            } catch (Exception exception) {
                Logger.Error(T("Creating widget failed: {0}", exception.Message).Text);
                _services.Notifier.Error(T("Creating widget failed: {0}", exception.Message));
                return Redirect(returnUrl);
            }
            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();
                return View((object)model);
            }

            _services.Notifier.Information(T("Your {0} has been added.", widgetPart.TypeDefinition.DisplayName));
            return Redirect(returnUrl);
        }

        public ActionResult EditWidget(int hostId, int id) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            var contentItem = _services.ContentManager.Get(hostId, VersionOptions.Latest);
            var contentMetadata = _services.ContentManager.GetItemMetadata(contentItem);
            var returnUrl = Url.RouteUrl(contentMetadata.EditorRouteValues);
            var widgetPart = _widgetsService.GetWidget(id);

            if (widgetPart == null) {
                _services.Notifier.Error(T("Widget not found: {0}", id));
                return Redirect(returnUrl);
            }
            try {
                var model = _services.ContentManager.BuildEditor(widgetPart).HostId(hostId);
                return View(model);
            } catch (Exception exception) {
                Logger.Error(T("Editing widget failed: {0}", exception.Message).Text);
                _services.Notifier.Error(T("Editing widget failed: {0}", exception.Message));

                return Redirect(returnUrl);
            }
        }

        [HttpPost, ActionName("EditWidget")]
        [Orchard.Mvc.FormValueRequired("submit.Save")]
        public ActionResult EditWidgetSavePost(int hostId, int id) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            var contentItem = _services.ContentManager.Get(hostId, VersionOptions.Latest);
            var contentMetadata = _services.ContentManager.GetItemMetadata(contentItem);
            var returnUrl = Url.RouteUrl(contentMetadata.EditorRouteValues);
            var widgetPart = _contentManager.Get<WidgetPart>(id, VersionOptions.DraftRequired); //_widgetsService.GetWidget(id);

            if (widgetPart == null)
                return HttpNotFound();

            try {
                var model = _services.ContentManager.UpdateEditor(widgetPart, this).HostId(hostId);
                var widgetExPart = widgetPart.As<WidgetExPart>();

                widgetPart.LayerPart = _widgetManager.GetContentLayer();
                widgetExPart.Host = contentItem;

                if (!ModelState.IsValid) {
                    _services.TransactionManager.Cancel();
                    return View(model);
                }

                _services.Notifier.Information(T("Your {0} has been saved.", widgetPart.TypeDefinition.DisplayName));
            } catch (Exception exception) {
                Logger.Error(T("Editing widget failed: {0}", exception.Message).Text);
                _services.Notifier.Error(T("Editing widget failed: {0}", exception.Message));
            }

            return Redirect(returnUrl);
        }

        [HttpPost, ActionName("EditWidget")]
        [Orchard.Mvc.FormValueRequired("submit.Delete")]
        public ActionResult EditWidgetDeletePOST(int id, int hostId) {
            return DeleteWidget(id, hostId);
        }

        private ActionResult DeleteWidget(int id, int hostId) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            var contentItem = _services.ContentManager.Get(hostId, VersionOptions.Latest);
            var contentMetadata = _services.ContentManager.GetItemMetadata(contentItem);
            var returnUrl = Url.RouteUrl(contentMetadata.EditorRouteValues);
            var widgetPart = _widgetsService.GetWidget(id);

            if (widgetPart == null)
                return HttpNotFound();

            try {
                _widgetsService.DeleteWidget(widgetPart.Id);
                _services.Notifier.Information(T("Widget was successfully deleted"));
            } catch (Exception exception) {
                Logger.Error(T("Removing Widget failed: {0}", exception.Message).Text);
                _services.Notifier.Error(T("Removing Widget failed: {0}", exception.Message));
            }

            return Redirect(returnUrl);
        }

        private bool IsAuthorizedToManageWidgets() {
            return _services.Authorizer.Authorize(Permissions.ManageWidgets, T("Not authorized to manage widgets"));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}