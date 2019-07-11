using Laser.Orchard.StartupConfig.ContentPickerContentCreation.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
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
                callbackUrl = request.QueryString.ToString();
                model.NameCPFiels = request.QueryString["namecpfield"];
            }
            else if(_controllerContextAccessor.Context.Controller.TempData["namecpfield"] != null) {
                model.NameCPFiels = _controllerContextAccessor.Context.Controller.TempData["namecpfield"].ToString();
            }
            model.Callback = callbackUrl;
            model.IdContent = part.ContentItem.Id;
            var tPart = (TitlePart)part.ContentItem.Parts.Single(p => p is TitlePart);
            if (tPart != null) {
                model.TitleContent = tPart.Title;
            }
            else {
                model.TitleContent = Convert.ToString(part.ContentItem.Id);
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
            updater.TryUpdateModel(model, Prefix, null, null);
            _controllerContextAccessor.Context.Controller.TempData["CallbackUrl"] = model.Callback;
            _controllerContextAccessor.Context.Controller.TempData["namecpfield"] = model.NameCPFiels;
            //quando è in postback.. richiama 
            return ContentShape("Parts_ContentPickerCreateItem_EditExtension", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContentPickerCreateItem.EditExtension",
                                                                                                   Model: model,
                                                                                                   Prefix: Prefix));
        }
    }
}