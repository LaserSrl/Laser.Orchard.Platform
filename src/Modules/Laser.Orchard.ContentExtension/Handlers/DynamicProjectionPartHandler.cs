using System;
using Laser.Orchard.ContentExtension.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.ContentExtension.Handlers {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionPartHandler : ContentHandler {
        public DynamicProjectionPartHandler(IRepository<DynamicProjectionPartRecord> dynamicprojectionRepository) {
            Filters.Add(StorageFilter.For(dynamicprojectionRepository));
            OnInitializing<DynamicProjectionPart>((ctx, x) => {
                x.OnAdminMenu = false;
                x.AdminMenuText = String.Empty;
            });
        }
    }
}

