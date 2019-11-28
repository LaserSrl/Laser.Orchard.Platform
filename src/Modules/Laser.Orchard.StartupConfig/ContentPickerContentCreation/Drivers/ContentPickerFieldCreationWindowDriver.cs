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
            var model = new ContentPickerCreationWindowVM {
                IdContent = part.ContentItem.Id,
                Published = part.ContentItem.IsPublished(),
                TypeContent = part.ContentItem.ContentType
            };

            TitlePart titlePart = part.ContentItem.As<TitlePart>();
            if (titlePart != null && !string.IsNullOrWhiteSpace(titlePart.Title)) {
                model.TitleContent = titlePart.Title;
            }

            return ContentShape("Parts_ContentPickerCreateItem_EditExtension", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContentPickerCreateItem.EditExtension",
                                                                                                       Model: model,
                                                                                                       Prefix: Prefix));
        }

        protected override DriverResult Editor(CommonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new ContentPickerCreationWindowVM();
            updater.TryUpdateModel(model, Prefix, null, null);

            return ContentShape("Parts_ContentPickerCreateItem_EditExtension", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContentPickerCreateItem.EditExtension",
                                                                                                   Model: model,
                                                                                                   Prefix: Prefix));
        }
    }
}