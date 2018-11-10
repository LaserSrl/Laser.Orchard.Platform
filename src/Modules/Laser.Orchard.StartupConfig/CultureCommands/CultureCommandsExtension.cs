using Orchard;
using Orchard.Commands;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.CultureCommands {
    public class CultureCommandsExtension : DefaultOrchardCommandHandler {
        private readonly ICultureManager _cultureManager;
        private readonly IOrchardServices _orchardServices;

        public CultureCommandsExtension(ICultureManager cultureManager, IOrchardServices orchardServices) {
            _cultureManager = cultureManager;
            _orchardServices = orchardServices;
        }
        [CommandHelp("cultures remove site culture <culture-name> \r\n\t" + "Remove culture for the site")]
        [CommandName("cultures remove site culture")]
        public void RemoveSiteCulture(string cultureName) {
            Context.Output.WriteLine(T("Removing site culture {0}", cultureName));

            if (!_cultureManager.IsValidCulture(cultureName)) {
                Context.Output.WriteLine(T("Supplied culture name {0} is not valid.", cultureName));
                return;
            }
            if(_orchardServices.WorkContext.CurrentSite.SiteCulture == cultureName) {
                Context.Output.WriteLine(T("Cannot remove current culture {0} from site. Change current culture first.", cultureName));
                return;
            }

            var cultureCheck = _cultureManager.ListCultures().FirstOrDefault(x => x == cultureName);
            if(string.IsNullOrEmpty(cultureCheck)) {
                Context.Output.WriteLine(T("Culture {0} is not activated on this site.", cultureName));
                return;
            }
            _cultureManager.DeleteCulture(cultureName);

            Context.Output.WriteLine(T("Site culture {0} removed successfully", cultureName));
        }
    }
}