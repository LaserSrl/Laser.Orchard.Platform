using System.Collections.Generic;
using Laser.Orchard.Faq.Models;
using Laser.Orchard.Faq.ViewModels;
using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.Faq.Services
{
    public interface IFaqTypeService : IDependency
    {
        List<FaqTypePartRecord> GetFaqTypes();
        List<FaqTypePartRecord> GetFaqTypes(bool filterOnCurrentCulture);
        //void UpdateFaqForContentItem(ContentItem item, EditFaqViewModel model);
        FaqTypePartRecord GetFaqType(int id);
        bool TypeNameAlredyExists(string name);
    }


}