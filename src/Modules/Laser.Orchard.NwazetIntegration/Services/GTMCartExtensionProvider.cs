using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class GTMCartExtensionProvider : ICartExtensionProvider {
        private readonly IShoppingCart _shoppingCart;
        private readonly dynamic _shapeFactory;
        private readonly IGTMProductService _GTMProductService;

        public GTMCartExtensionProvider(
            IShoppingCart shoppingCart,
            IShapeFactory shapeFactory,
            IGTMProductService GTMProductService) {

            _shoppingCart = shoppingCart;
            _shapeFactory = shapeFactory;
            _GTMProductService = GTMProductService;
        }

        public IEnumerable<dynamic> CartExtensionShapes() {

            if (_GTMProductService.ShoulAddEcommerceTags()) {
                var productQuantities = _shoppingCart
                    .GetProducts();

                var useGA4 = _GTMProductService.UseGA4();

                var gtmObjs = productQuantities
                    // Should we take for granted the fact that a product will always
                    // have a GTMProductPart?
                    .Where(pq => pq.Product.Is<GTMProductPart>())
                    .Select(pq => {
                        var part = pq.Product.As<GTMProductPart>();
                        _GTMProductService.FillPart(part);
                        var vm = _GTMProductService.GetViewModel(part);
                        vm.Quantity = pq.Quantity;
                        return vm;
                    });

                yield return _shapeFactory.GTMShoppingCart(GTMProducts: gtmObjs);
            }
        }
    }
}