using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Faq.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Faq.Handlers
{
    public class FaqWidgetAjaxHandler : ContentHandler
    {
        public FaqWidgetAjaxHandler(IRepository<FaqWidgetAjaxPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}