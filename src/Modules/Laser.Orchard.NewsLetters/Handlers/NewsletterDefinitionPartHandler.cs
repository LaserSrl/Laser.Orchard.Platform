using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NewsLetters.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.NewsLetters.Handlers {
    public class NewsletterDefinitionPartHandler : ContentHandler {
        public NewsletterDefinitionPartHandler(IRepository<NewsletterDefinitionPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}