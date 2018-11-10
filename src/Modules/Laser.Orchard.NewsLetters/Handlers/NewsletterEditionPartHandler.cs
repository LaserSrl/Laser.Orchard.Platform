using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NewsLetters.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.NewsLetters.Handlers {
    public class NewsletterEditionPartHandler : ContentHandler {
        public NewsletterEditionPartHandler(IRepository<NewsletterEditionPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}