using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.DisplayManagement.Shapes;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class GTMProductHandler : ContentHandler {

        public GTMProductHandler(
            IRepository<GTMProductPartRecord> repository) {

            Filters.Add(StorageFilter.For(repository));
        }
    }
}
