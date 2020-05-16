using Laser.Orchard.AdvancedSettings.Models;
using Laser.Orchard.AdvancedSettings.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.Handlers {
    public class AdvancedSettingsPartHandler : ContentHandler {
        private readonly IAdvancedSettingsService _settingService;

        public AdvancedSettingsPartHandler(IRepository<AdvancedSettingsPartRecord> repository, IAdvancedSettingsService settingService) {
            _settingService = settingService;

            Filters.Add(StorageFilter.For(repository));
            OnRemoved<AdvancedSettingsPart>((context, part) => EvictCache(part));
            OnPublished<AdvancedSettingsPart>((context, part) => EvictCache(part));
            OnUnpublished<AdvancedSettingsPart>((context, part) => EvictCache(part));
        }

        private void EvictCache(AdvancedSettingsPart part) {
            _settingService.ReleaseCachedSetting(part.Name);
        }
    }
}