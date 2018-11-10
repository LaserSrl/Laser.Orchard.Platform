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
    public class OwnerEmailToken : ITokenProvider {
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }
        public OwnerEmailToken(IOrchardServices orchardServices) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
        }
        public void Describe(DescribeContext context) {

            context.For("Content", T("Owner email"), T("Owner email of content item "))
                   .Token("OwnerEmail", T("OwnerEmail"), T("The email of user creator."))
                   ;
        }

        public void Evaluate(EvaluateContext context) {
             context.For<IContent>("Content")
                    .Token("OwnerEmail", x => GetTheValue(x));
        }
        private string GetTheValue(IContent part) {
            IUser OwnerUser= (IUser)((dynamic)part.As<CommonPart>()).Owner;
            if (OwnerUser != null)
                return OwnerUser.Email;
            else
                return null;
        }
    }
} 