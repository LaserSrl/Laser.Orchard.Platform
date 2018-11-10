using Laser.Orchard.DataProtection.Services;
using Orchard.Security;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.DataProtection.Models;

namespace Laser.Orchard.DataProtection.Security {
    public class DataProtectionAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IDataProtectionCheckerService _dataProtectionCheckerService;
        public DataProtectionAuthorizationEventHandler(IDataProtectionCheckerService dataProtectionCheckerService) {
            _dataProtectionCheckerService = dataProtectionCheckerService;
        }
        public void Adjust(CheckAccessContext context) {
        }

        public void Checking(CheckAccessContext context) {
        }

        public void Complete(CheckAccessContext context) {
            if (context.Content != null) {
                var ci = _dataProtectionCheckerService.CheckDataRestricitons(context.Content.ContentItem, context.Permission);
                if (ci == null) {
                    context.Granted = false;
                    context.Adjusted = true;
                }
            }
        }
    }
}