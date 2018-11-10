using System;
using System.Web.Mvc;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.Environment.Extensions;
using Orchard.UI.Admin;

namespace Laser.Orchard.TemplateManagement.Controllers {
    [Admin, OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class TemplateController : Controller {
        private readonly ITemplateService _templateService;

        public TemplateController(ITemplateService TemplateService) {
            _templateService = TemplateService;
        }

        public string LayoutContent(int id) {
            var template = _templateService.GetTemplate(id);

            if(!template.IsLayout)
                throw new InvalidOperationException("That is not a Layout");

            return template.Text;
        }
    }
}