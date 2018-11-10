using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig;
using Laser.Orchard.StartupConfig.Services;

namespace Laser.Orchard.ExternalContent {
    public class Migration : DataMigrationImpl {

        private readonly IUtilsServices _utilsServices;

        public Migration(IUtilsServices utilsServices) {
            _utilsServices = utilsServices;
        }
        public int Create() {
           return 1;
        }
        public int UpdateFrom1() {
            _utilsServices.EnableFeature("Orchard.Caching");
            return 2;
        }
    }
}