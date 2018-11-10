using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.Faq.Models;
using Laser.Orchard.Faq.Services;
using Laser.Orchard.Faq.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace Laser.Orchard.Faq.Controllers
{
    
    public class FaqWidgetAjaxController :Controller
    {
        private readonly IFaqService _faqService;

        public FaqWidgetAjaxController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        [HttpPost]
        public JsonResult GetTypedFaq(int faqTypeId)
        {
            List<ViewFaqViewModel> model = new List<ViewFaqViewModel>();
            
            model = _faqService.GetTypedFaqs(faqTypeId).Select(retrieveFaq).ToList();

            return Json(model);
        }

        private ViewFaqViewModel retrieveFaq(FaqPart faqPart)
        {
            ViewFaqViewModel model = new ViewFaqViewModel();
            BodyPart bodyPart = faqPart.ContentItem.As<BodyPart>();
            if (bodyPart != null && bodyPart.Text != null)
            {
                model.Answer = bodyPart.Text;
            }
            else
            {
                model.Answer = string.Empty;
            }
            model.Question = faqPart.Question;
            model.Id = faqPart.Id;
            return model;
        }
    }
}