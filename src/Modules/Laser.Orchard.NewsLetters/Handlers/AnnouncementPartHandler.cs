using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NewsLetters.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.NewsLetters.Handlers {
    public class AnnouncementPartHandler : ContentHandler {
        public AnnouncementPartHandler(IRepository<AnnouncementPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}