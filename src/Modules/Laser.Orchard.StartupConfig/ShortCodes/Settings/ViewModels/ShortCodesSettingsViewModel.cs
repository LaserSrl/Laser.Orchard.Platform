using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using Laser.Orchard.StartupConfig.ShortCodes.Settings.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes.Settings.ViewModels {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ShortCodesSettingsViewModel {
        public ShortCodesSettingsViewModel() {
            EnabledShortCodes = new List<string>();
            DefinedShortCodes = new List<string>();
        }

        public IEnumerable<string> EnabledShortCodes { get; set; }

        public IEnumerable<string> DefinedShortCodes { get; private set; }

        public void Populate(ShortCodesSettings settings, IEnumerable<IShortCodeProvider> shortCodeProviders) {
            if (shortCodeProviders != null) {
                DefinedShortCodes = shortCodeProviders.Select(x => x.Describe(new DescribeContext()).Name).OrderBy(x => x);
            }
            if (settings != null && !string.IsNullOrWhiteSpace(settings.EnabledShortCodes)) {
                EnabledShortCodes = settings.EnabledShortCodes.Split(',');
            }
            else {
                EnabledShortCodes = new List<string>();
            }
        }
    }
}