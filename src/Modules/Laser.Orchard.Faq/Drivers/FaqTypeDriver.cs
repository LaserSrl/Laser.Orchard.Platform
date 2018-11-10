using Laser.Orchard.Faq.Models;
using Laser.Orchard.Faq.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Mvc;

namespace Laser.Orchard.Faq.Drivers
{
    public class FaqTypeDriver : ContentPartCloningDriver<FaqTypePart>
    {
        private readonly IFaqTypeService _faqTypeService;

        public Localizer T;

        public FaqTypeDriver(IFaqTypeService faqTypeService) {
            _faqTypeService = faqTypeService;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(FaqTypePart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_FaqType",
                                () => shapeHelper.Parts_FaqType (
                                    Title: part.Title,
                                    Id: part.Id));
        }

        protected override string Prefix {
            get { return "FaqType"; }
        }

        protected override DriverResult Editor(FaqTypePart part, dynamic shapeHelper) {

            var temp = ContentShape("Parts_FaqType_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/FaqType",
                                    Model: part,
                                    Prefix: Prefix));
            
            return temp;
        }

        protected override DriverResult Editor(FaqTypePart part, IUpdateModel updater, dynamic shapeHelper) {
            //var temp = _faqTypeService.TypeNameAlredyExists("qwe");
            updater.TryUpdateModel(part, Prefix, null, null);
            //if (part.Title!= null && _faqTypeService.TypeNameAlredyExists(part.Title))
            //{
            //    updater.AddModelError("DuplicateName", T("Faq Type with the same name already exists."));
            //}

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(FaqTypePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Title", part.Title);
        }

        protected override void Importing(FaqTypePart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var faqTitle = root.Attribute("Title");
            if (faqTitle != null) {
                part.Title = faqTitle.Value;
            }
        }

        protected override void Cloning(FaqTypePart originalPart, FaqTypePart clonePart, CloneContentContext context) {
            clonePart.Title = originalPart.Title;
        }
    }
}