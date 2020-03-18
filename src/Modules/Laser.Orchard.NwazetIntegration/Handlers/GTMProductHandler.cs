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
        private readonly IOrchardServices _orchardServices;
        private readonly IGTMProductService _GTMProductService;

        public GTMProductHandler(
            IRepository<GTMProductPartRecord> repository,
            IOrchardServices orchardServices,
            IGTMProductService GTMProductService) {

            Filters.Add(StorageFilter.For(repository));
            _orchardServices = orchardServices;
            _GTMProductService = GTMProductService;

            OnGetDisplayShape<GTMProductPart>((context, part) => {
                if (context.DisplayType == "Detail") {
                    _GTMProductService.FillPart(part);

                    var gtmProductVM = new GTMProductVM {
                        Id = part.ProductId
                    };

                    //remove previous sharelink metas
                    var layout = (dynamic)context.Layout;
                    if (layout.Head?.Items != null) {
                        var headShapes = (List<object>)layout.Head.Items;
                        headShapes.RemoveAll(sha => ((Shape)sha)
                          .Metadata
                          .Type == "GTMProduct");
                    }
                    layout.Head.Add(context.New.GTMProduct(GTMProductVM: gtmProductVM));
                }
            });
        }
    }
}
