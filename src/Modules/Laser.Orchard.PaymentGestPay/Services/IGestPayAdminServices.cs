using Laser.Orchard.PaymentGestPay.ViewModels;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.PaymentGestPay.Services {
    public interface IGestPayAdminServices : IDependency {
        GestPaySettingsViewModel GetSettingsVM();
        void UpdateSettings(GestPaySettingsViewModel vm);
    }
}
