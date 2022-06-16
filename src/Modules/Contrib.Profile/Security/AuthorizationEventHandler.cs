using Orchard.Security;
using Orchard.Core.Contents;
using Orchard;
using System;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Contrib.Profile.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IWorkContextAccessor _workContextAccessor;

        public AuthorizationEventHandler(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (context.Granted || context.Permission != Permissions.ViewProfiles || context.Content != context.User) {
                return;
            }
        }
    }
}