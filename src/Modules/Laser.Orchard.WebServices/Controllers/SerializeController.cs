using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.WebServices.Controllers {
    [WebApiKeyFilter(false)]
    public class SerializeController : ApiController {
        private readonly IOrchardServices _orchardServices;
        public SerializeController(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public dynamic Get(Int32 id) {
         ContentItem theci= _orchardServices.ContentManager.Get(id);
         return new VMci(theci);
        }

    }
    public class VMci {
        public VMci(ContentItem ci) {
           if (((dynamic)ci).TitlePart!=null)
             this.Title = ((dynamic)ci).TitlePart.Title;
        }
        public string Title { get; set; }
        public string Id { get; set; }
    }

}