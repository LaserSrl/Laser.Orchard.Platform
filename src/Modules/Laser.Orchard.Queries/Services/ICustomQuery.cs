using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Projections.Models;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Services;
using Orchard.Tokens;

namespace Laser.Orchard.Queries.Services {

    public interface ICustomQuery : IDependency {

        Dictionary<String, Int32> Get(string option);

        IEnumerable<ContentItem> ListContent();
    }

    public class CustomQuery : ICustomQuery {
        private readonly IOrchardServices _orchardServices;

        public CustomQuery(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public Dictionary<String, Int32> Get(string option) {
            Dictionary<String, Int32> Listquery = new Dictionary<string, int>();
            IEnumerable<ContentItem> enumCI = _orchardServices
                .ContentManager.Query()
                .ForType("MyCustomQuery")
                .List()
                .Where(x => ((dynamic)x).MyCustomQueryPart
                    .Options.Value.ToString().Contains(option));
            foreach (ContentItem ci in enumCI) {
                Listquery.Add(ci.As<TitlePart>().Title, ci.Id);
            }
            return Listquery;
        }

        public IEnumerable<ContentItem> ListContent() {
            return _orchardServices.ContentManager.Query(VersionOptions.Latest).ForType(new string[] { "MyCustomQuery", "Query" }).OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc).List().Where(x => x.ContentType == "MyCustomQuery" || ((dynamic)x).QueryUserFilterExtensionPart.UserQuery.Value == true);
        }

    }
}