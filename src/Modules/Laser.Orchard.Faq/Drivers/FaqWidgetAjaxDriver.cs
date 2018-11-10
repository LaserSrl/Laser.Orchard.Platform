using Laser.Orchard.Faq.Models;
using Laser.Orchard.Faq.Services;
using Laser.Orchard.Faq.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.Faq.Drivers
{
    public class FaqWidgetAjaxDriver : ContentPartDriver<FaqWidgetAjaxPart>
    {
        private readonly IFaqService _faqService;
        private readonly IFaqTypeService _faqTypeService;

        public FaqWidgetAjaxDriver(IFaqTypeService faqTypeService, IFaqService faqService)
        {
            _faqService = faqService;
            _faqTypeService = faqTypeService;
        }

        protected override DriverResult Display(FaqWidgetAjaxPart part, string displayType, dynamic shapeHelper)
        {
            var types = _faqTypeService.GetFaqTypes();
           
            return ContentShape("Parts_FaqWidgetAjax",
                                () => shapeHelper.Parts_FaqWidgetAjax(
                                    FaqTypes: types));
        }

        protected override DriverResult Editor(FaqWidgetAjaxPart part, dynamic shapeHelper)
        {
            var temp = ContentShape("Parts_FaqWidgetAjax_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/FaqWidgetAjax",
                                    Model: part,
                                    Prefix: Prefix));
            return temp;
        }

        //POST
        protected override DriverResult Editor(FaqWidgetAjaxPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }

        protected override string Prefix
        {
            get { return "FaqWidgetAjax"; }
        }

    }
}