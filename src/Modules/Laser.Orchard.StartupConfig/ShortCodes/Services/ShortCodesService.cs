using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes.Services {
    public class ShortCodesService : IShortCodesService {
        private readonly IEnumerable<IShortCodeProvider> _providers;
        private readonly DescribeContext _emptyContext;
        public ShortCodesService(IEnumerable<IShortCodeProvider> providers) {
            _providers = providers;
            _emptyContext = new DescribeContext();
        }

        public IEnumerable<IShortCodeProvider> GetProviders() {
            return _providers;
        }

        public IEnumerable<IShortCodeProvider> GetEnabledProviders(DescribeContext context) {
            if (context == null) {
                context = _emptyContext;
            }
            return _providers.Where(x => x.Describe(context).Enabled);
        }

    }
}