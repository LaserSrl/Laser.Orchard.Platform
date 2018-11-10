using Laser.Orchard.Faq.Models;
using Laser.Orchard.Faq.Services;
using Laser.Orchard.Faq.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.Faq.Drivers
{
    public class FaqWidgetDriver : ContentPartDriver<FaqWidgetPart>
    {
        private readonly IFaqService _faqService;
        private readonly IFaqTypeService _faqTypeService;

        public FaqWidgetDriver(IFaqTypeService faqTypeService, IFaqService faqService)
        {
            _faqService = faqService;
            _faqTypeService = faqTypeService;
        }

        protected override DriverResult Display(FaqWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var types = _faqTypeService.GetFaqTypes();
            var faqs = _faqService.GetLastFaqs(null, null);

            return ContentShape("Parts_FaqWidget",
                                () => shapeHelper.Parts_FaqWidget(
                                    FaqTypes: types,
                                    Faqs: faqs));
        }

        protected override DriverResult Editor(FaqWidgetPart part, dynamic shapeHelper)
        {
            var temp = ContentShape("Parts_FaqWidget_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/FaqWidget",
                                    Model: part,
                                    Prefix: Prefix));
            return temp;
        }

        //POST
        protected override DriverResult Editor(FaqWidgetPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }

        protected override string Prefix
        {
            get { return "FaqWidget"; }
        }

    }
}