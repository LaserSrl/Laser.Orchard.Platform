using System.Collections.Generic;
using Laser.Orchard.Faq.Models;
using Laser.Orchard.Faq.ViewModels;
using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.Faq.Services
{
    public interface IFaqService : IDependency
    {
        IEnumerable<FaqPart> GetLastFaqs(int? count = null, int? page = null);
        int FilterByTypeId { get; set; }
        double GetCountOfPage(int count);
        IEnumerable<FaqPart> GetTypedFaqs(int faqTypeId);
        void UpdateFaqForContentItem(ContentItem item, EditFaqViewModel model);
    }
}