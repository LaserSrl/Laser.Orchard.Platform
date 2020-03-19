using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.DisplayManagement.Shapes;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class GTMProductHandler : ContentHandler {
        private readonly IGTMProductService _GTMProductService;

        public GTMProductHandler(
            IRepository<GTMProductPartRecord> repository,
            IGTMProductService GTMProductService) {

            Filters.Add(StorageFilter.For(repository));
            _GTMProductService = GTMProductService;

            OnGetDisplayShape<GTMProductPart>((context, part) => {
                if (context.DisplayType == "Detail") {
                    if(context.ContentPart != null && context.ContentPart.GetType().Name=="ProductPart") {
                        _GTMProductService.FillPart(((ProductPart)context.ContentPart), part);

                        var gtmProductVM = new GTMProductVM {
                            Id = part.ProductId,
                            Name = part.Name,
                            Brand = part.Brand,
                            Category = part.Category,
                            Variant = part.Variant,
                            Price = part.Price,
                            Quantity = part.Quantity,
                            Coupon = part.Coupon,
                            Position = part.Position
                        };

                        var layout = (dynamic)context.Layout;
                        layout.Head.Add(context.New.GTMProduct(GTMProductVM: gtmProductVM));
                    }

                }
            });
        }
    }
}
