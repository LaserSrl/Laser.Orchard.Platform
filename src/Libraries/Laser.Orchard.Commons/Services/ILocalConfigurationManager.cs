using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using Orchard;

namespace Laser.Orchard.Commons.Services {
    // attualmente questa interfaccia non viene utilizzata, istanziare direttamente il servizio
    public interface ILocalConfigurationManager {
        Configuration GetConfiguration(string url);
    }
}
