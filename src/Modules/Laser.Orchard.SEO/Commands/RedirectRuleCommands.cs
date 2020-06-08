using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.Services;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using System;
using System.Linq;

namespace Laser.Orchard.SEO.Commands {
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectRuleCommands : DefaultOrchardCommandHandler {

        private readonly IRedirectService _redirectService;

        public RedirectRuleCommands(
            IRedirectService redirectService) {

            _redirectService = redirectService;
        }

        [CommandName("redirects import")]
        [CommandHelp("redirects import <createdDateTimeTick sourceUrl destinationUrl isPermanent>\r\n\t" + "Imports the redirects.")]
        public void Import(
            long createdDateTimeTick, 
            string sourceUrl, 
            string destinationUrl, 
            bool isPermanent) {

            var result = _redirectService.GetTable().FirstOrDefault(x=>x.SourceUrl == sourceUrl);

            if (result == null) {
                _redirectService.Add(new RedirectRule {
                                            CreatedDateTime = new DateTime(createdDateTimeTick),
                                            SourceUrl = sourceUrl,
                                            DestinationUrl = destinationUrl,
                                            IsPermanent = isPermanent
                });
            }
        }
    }
}