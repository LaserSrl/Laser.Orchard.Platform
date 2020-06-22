using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    public class TerritoriesApiController : ApiController {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;

        public TerritoriesApiController(
            IContentManager contentManager,
            IAuthorizer authorizer) {

            _contentManager = contentManager;
            _authorizer = authorizer;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<TerritoryTag> Get(int hierarchyId, string query) {
            if (!_authorizer.Authorize(StandardPermissions.AccessAdminPanel)) {
                throw new UnauthorizedAccessException("Can't access the admin");
            }
            throw new NotImplementedException();
        }
    }
}