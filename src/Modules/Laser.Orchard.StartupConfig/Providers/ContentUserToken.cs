using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Tokens;
using Orchard.Users.Models;

namespace Laser.Orchard.StartupConfig.Providers {
    public class ContentUserToken : ITokenProvider {
        private readonly IContentManager _contentManager;
        private readonly IAuthenticationService _authenticationService;

        public Localizer T { get; set; }

        public ContentUserToken(IContentManager contentManager, IAuthenticationService authenticationService) {
            _contentManager = contentManager;
            _authenticationService = authenticationService;

            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            context.For("Content", T("Content Items"), T("Content Items"))
                 .Token("User", T("User"), T("Gets the Username"))
                 .Token("UserEmail", T("UserEmail"), T("Gets the User Email"));

            //context.For("AuthenticatedUser", T("Authenticated User"), T("Authenticated User"))
            //    .Token("Email", T("Authenticated User Email"), T("Gets the Authenticated User Email"))
            //    .Token("Id", T("Authenticated User Id"), T("Gets the Authenticated User Id"));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                .Token("User", content => content.As<UserPart>() != null ? content.As<UserPart>().UserName : null)
                .Chain("User", "User", content => content.As<UserPart>() ?? null);
            context.For<IContent>("Content")
                .Token("UserEmail", content => content.As<UserPart>() != null ? content.As<UserPart>().Email : null)
                .Chain("UserEmail", "Text", content => content.As<UserPart>() != null ? content.As<UserPart>().Email : null);



            //context.For<IUser>("AuthenticatedUser")
            //       .Token("Email", content => _authenticationService.GetAuthenticatedUser() != null ? _authenticationService.GetAuthenticatedUser().Email : null)
            //       .Token("Id", content => {
            //           var authUser = _authenticationService.GetAuthenticatedUser();
            //           return authUser != null ? authUser.Id : 0;
            //       });


        }
    }
}