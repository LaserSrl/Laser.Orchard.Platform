using System;
using System.Linq;
using Orchard.Taxonomies.Fields;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Orchard.Fields.Fields;
using Orchard.ContentManagement.Aspects;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class CreatorEmailTokens : ITokenProvider {
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }
        public CreatorEmailTokens(IOrchardServices orchardServices) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
        }
        public void Describe(DescribeContext context) {
            // Usage:
            // Content.Fields.Article.Categories.Terms -> 'Science, Sports, Arts'
            // Content.Fields.Article.Categories.Terms:0 -> 'Science'

            // When used with an indexer, it can be chained with Content tokens
            // Content.Fields.Article.Categories.Terms:0.DisplayUrl -> http://...

            context.For("Content", T("Creator email"), T("Creator email of content item "))
                   .Token("CreatorEmail", T("CreatorEmail"), T("The email of user creator."))
                   ;
        }

        public void Evaluate(EvaluateContext context) {
            //((dynamic)context.Data["Content"]).CommonPart.Creator.Value
            context.For<IContent>("Content")
                    .Token("CreatorEmail",x=> GetTheValue(x))//content => ((dynamic)content.As<ICommonPart>()).Creator.Value)//GetTheValue) // content => _orchardServices.ContentManager.Get((Int32)((dynamic)content).Creator.Value, VersionOptions.Latest).As<UserPart>().Email)
                   ;
        }
        private string GetTheValue(IContent part) {
            Int32 idcreatore = (Int32)((dynamic)part.As<CommonPart>()).Creator.Value;
            return _orchardServices.ContentManager.Get(idcreatore, VersionOptions.Latest).As<UserPart>().Email;
        }
    }
}