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
using Orchard.Security;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class OwnershipToken : ITokenProvider {
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }
        public OwnershipToken(IOrchardServices orchardServices) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
        }
        public void Describe(DescribeContext context) {
            context.For("Content", T("Content"), T("The processed content"))
                   .Token("Ownership", T("Owner or User"), T("Owner of content item or User content item."))
                   .Token("OwnerEmail", T("OwnerEmail"), T("The email of user creator."));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                   .Token("OwnerEmail", x => GetTheValue(x))
                   .Token("Ownership", x => GetTheContentOwnerOrUser(x))
                   .Chain("User", "User", x => GetTheContentOwnerOrUser(x));
        }
        private string GetTheValue(IContent content) {
            if (content.As<CommonPart>() != null) {
                return content.As<CommonPart>().Owner != null ? content.As<CommonPart>().Owner.Email : null;
            }
            else if (content.As<UserPart>() != null) {
                return content.As<UserPart>().Email;
            }
            return null;

        }

        private ContentItem GetTheContentOwnerOrUser(IContent content) {
            if (content.As<CommonPart>() != null) {
                return content.As<CommonPart>().Owner != null ? content.As<CommonPart>().Owner.ContentItem : null;
            }
            else if (content.As<UserPart>() != null) {
                return content.ContentItem;
            }
            return null;
        }
    }
}