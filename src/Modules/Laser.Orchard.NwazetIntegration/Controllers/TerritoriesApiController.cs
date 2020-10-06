using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
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
        private readonly IAddressConfigurationService _addressConfigurationService;

        public TerritoriesApiController(
            IContentManager contentManager,
            IAuthorizer authorizer,
            IAddressConfigurationService addressConfigurationService) {

            _contentManager = contentManager;
            _authorizer = authorizer;
            _addressConfigurationService = addressConfigurationService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<TerritoryTag> Get(int hierarchyId, string query) {
            if (!_authorizer.Authorize(StandardPermissions.AccessAdminPanel)) {
                throw new UnauthorizedAccessException("Can't access the admin");
            }
            var hierarchy = _contentManager.Get<TerritoryHierarchyPart>(hierarchyId);

            if (hierarchy != null) {
                var parts = _contentManager
                    .Query<TerritoryPart, TerritoryPartRecord>()
                    .Where(tpr => tpr.Hierarchy.Id == hierarchyId)
                    .Join<TitlePartRecord>()
                    .Where(tpr => tpr.Title.Contains(query))
                    .List();

                return parts.Select(tp => new TerritoryTag {
                    Label = tp.As<TitlePart>().Title,
                    Value = tp.Record.TerritoryInternalRecord.Id + ";" + tp.Record.TerritoryInternalRecord.NameHash
                });
            }

            return new List<TerritoryTag>();
        }

    }
}