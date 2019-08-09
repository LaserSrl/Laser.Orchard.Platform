using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Security;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;


namespace Laser.Orchard.StartupConfig.Drivers {
    [OrchardFeature("Laser.Orchard.StartupConfig.PerItemCache")]
    public class PerItemCachePartDriver : ContentPartDriver<PerItemCachePart> {

        private readonly IOrchardServices _orchardServices;

        public PerItemCachePartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "PerItemKeyParam"; }
        }

        protected override DriverResult Editor(PerItemCachePart part, dynamic shapeHelper) {
            if(!_orchardServices.Authorizer.Authorize(PerItemCachePermission.AccessPerItemCacheKey))
                return null;

            return ContentShape("Parts_PerItemCachePart_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/PerItemCachePart_Edit", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PerItemCachePart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_orchardServices.Authorizer.Authorize(PerItemCachePermission.AccessPerItemCacheKey))
                return null;

            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }



    }
}