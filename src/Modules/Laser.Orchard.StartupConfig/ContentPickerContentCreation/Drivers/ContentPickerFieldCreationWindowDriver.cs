using Laser.Orchard.StartupConfig.ContentPickerContentCreation.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.Drivers {
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
                callbackUrl = request.QueryString["callback"];
            }
            model.Callback = callbackUrl;
            var isContentPickerCreation = callbackUrl.StartsWith("_contentpickercreate_");
            if (isContentPickerCreation) {
                return ContentShape("Parts_ContentPickerCreateItem_EditExtension", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContentPickerCreateItem.EditExtension",
                                                                                                       Model: model,
                                                                                                       Prefix: Prefix));
            }

            return null;
        }

        protected override DriverResult Editor(CommonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new SelectButton();
            updater.TryUpdateModel(model, Prefix, null, null);
            _controllerContextAccessor.Context.Controller.TempData["CallbackUrl"] = model.Callback;
            //quando è in postback.. richiama 
            return ContentShape("Parts_ContentPickerCreateItem_EditExtension", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContentPickerCreateItem.EditExtension",
                                                                                                   Model: model,
                                                                                                   Prefix: Prefix));
        }
    }
}