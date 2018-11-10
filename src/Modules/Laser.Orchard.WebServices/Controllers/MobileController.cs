using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Orchard.Themes;

namespace Laser.Orchard.WebServices.Controllers {
    public class MobileController : Controller {
        dynamic Shape { get; set; }
        public MobileController(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
        }
        [Themed]
        public ActionResult Content() {
            dynamic viewModel = Shape.Parts_StoreRedirect();
            return new ShapeResult(this, viewModel);
        }

    }
}
