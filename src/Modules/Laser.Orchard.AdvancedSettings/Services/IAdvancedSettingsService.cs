using Laser.Orchard.AdvancedSettings.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.Services {
    public interface IAdvancedSettingsService : IDependency {
        IContent GetCachedSetting(string settingName);
        void ReleaseCachedSetting(string settingName);
    }
}