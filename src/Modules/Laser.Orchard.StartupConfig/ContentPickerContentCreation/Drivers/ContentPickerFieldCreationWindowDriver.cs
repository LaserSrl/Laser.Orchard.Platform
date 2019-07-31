using Laser.Orchard.StartupConfig.ContentPickerContentCreation.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using System;
using System.Linq;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.Drivers {
    [OrchardFeature("Laser.Orchard.StartupConfig.ContentPickerContentCreation")]

    public class ContentPickerFieldCreationWindowDriver : ContentPartDriver<CommonPart> {

        private readonly IOrchardServices _orchardServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;

        public ContentPickerFieldCreationWindowDriver(IOrchardServices orchardServices, IControllerContextAccessor controllerContextAccessor) {
            _orchardServices = orchardServices;
            _controllerContextAccessor = controllerContextAccessor;
        }

        protected override string Prefix {
            get { return "Laser.Orchard.ContentPickerContentCreation"; }
        }

        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {

            var request = _orchardServices.WorkContext.HttpContext.Request;

            var routeData = request.RequestContext.RouteData;
            var model = new SelectButton();

            var callbackUrl = (string)_controllerContextAccessor.Context.Controller.TempData["CallbackUrl"] ?? "";
            if (callbackUrl == "") {
                callbackUrl = request.QueryString.ToString();
                model.NameCPField = request.QueryString["namecpfield"];
            }
            else if(_controllerContextAccessor.Context.Controller.TempData["namecpfield"] != null) {
                model.NameCPField = _controllerContextAccessor.Context.Controller.TempData["namecpfield"].ToString();
            }

            model.Callback = callbackUrl;
            model.IdContent = part.ContentItem.Id;
            TitlePart titlePart = part.ContentItem.As<TitlePart>();
            if (titlePart != null && !string.IsNullOrWhiteSpace(titlePart.Title)) {
                model.TitleContent = titlePart.Title;
            }

            model.Published = part.ContentItem.IsPublished();

            var isContentPickerCreation = callbackUrl.Contains("_contentpickercreate_");
            if (isContentPickerCreation) {
                return ContentShape("Parts_ContentPickerCreateItem_EditExtension", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContentPickerCreateItem.EditExtension",
                                                                                                       Model: model,
                                                                                                       Prefix: Prefix));
            }

            return null;
        }

        protected override DriverResult Editor(CommonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new SelectButton();
            var updateSuccess = updater.TryUpdateModel(model, Prefix, null, null);

            if (!string.IsNullOrWhiteSpace(model.Callback)) {
                _controllerContextAccessor.Context.Controller.TempData["CallbackUrl"] = model.Callback;
                _controllerContextAccessor.Context.Controller.TempData["namecpfield"] = model.NameCPField;

                if (updateSuccess)
                    return ContentShape("Parts_ContentPickerCreateItem_EditExtension", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContentPickerCreateItem.EditExtension",
                                                                                                       Model: model,
                                                                                                       Prefix: Prefix));
                else
                    return Editor(part, shapeHelper);
            }

            return null;
        }
    }
}