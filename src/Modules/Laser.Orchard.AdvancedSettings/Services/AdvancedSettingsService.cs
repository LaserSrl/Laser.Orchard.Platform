using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.AdvancedSettings.Models;
using Orchard.Caching;
using Orchard.ContentManagement;

namespace Laser.Orchard.AdvancedSettings.Services {
    public class AdvancedSettingsService : IAdvancedSettingsService {
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public AdvancedSettingsService(IContentManager contentManager, ICacheManager cacheManager, ISignals signals) {
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _signals = signals;
        }
        public string SettingsCacheKey(string settingName) {
            return string.Format("AdvancedSettings-{0}-Cache", settingName);
        }

        public IContent GetCachedSetting(string settingName) {
            var advancedSettings = _cacheManager.Get(SettingsCacheKey(settingName), true, context => {
                context.Monitor(_signals.When(SettingsCacheKey(settingName)));
                var settings = _contentManager.Query<AdvancedSettingsPart, AdvancedSettingsPartRecord>().Where(x => x.Name.Equals(settingName, StringComparison.InvariantCultureIgnoreCase)).Slice(0, 1).SingleOrDefault();
                return settings?.ContentItem;
            });
            return advancedSettings;
        }

        public void ReleaseCachedSetting(string settingName) {
            _signals.Trigger(SettingsCacheKey(settingName));
        }
    }
}
