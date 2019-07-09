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

        public ContentPickerFieldCreationWindowDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        protected override string Prefix {
            get { return "Laser.Orchard.ContentPickerContentCreation"; }
        }

        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {

            var request = _orchardServices.WorkContext.HttpContext.Request;

            var routeData = request.RequestContext.RouteData;
            var isContentPickerCreation = request.QueryString["callback"] != null && request.QueryString["callback"].StartsWith("_contentpickercreate_");

            if (((Route)routeData.Route).Url.StartsWith("Admin/Contents/{action}") && routeData.Values["action"].Equals("Create") && isContentPickerCreation) {
                return ContentShape("Parts_ContentPickerCreateItem_EditExtension", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContentPickerCreateItem.EditExtension",
                                                                                                       Model: null,
                                                                                                       Prefix: Prefix));
            }

            return null;
        }

        protected override DriverResult Editor(CommonPart part, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, shapeHelper);
        }
    }
}