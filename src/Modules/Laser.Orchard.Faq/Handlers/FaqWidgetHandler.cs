using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Faq.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Faq.Handlers
{
    public class FaqWidgetHandler : ContentHandler
    {
        public FaqWidgetHandler(IRepository<FaqWidgetPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}