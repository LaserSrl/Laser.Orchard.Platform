using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Handlers {
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperHandler : ContentHandler {

        public KeywordHelperHandler(IRepository<KeywordHelperPartVersionRecord> repository) {

            Filters.Add(StorageFilter.For(repository));

        }
    }
}