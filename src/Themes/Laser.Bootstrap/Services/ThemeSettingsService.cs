using System.Linq;
using Orchard.Data;
using Laser.Bootstrap.Models;
using System.Collections.Generic;
using System.Configuration;
using Laser.Bootstrap.ViewModels;
using Laser.Orchard.Commons.Services;

namespace Laser.Bootstrap.Services {
    public class SettingsService : IThemeSettingsService {
        private readonly IRepository<ThemeSettingsRecord> _repository;
        private readonly LocalWebConfigManager _localConfiguration;

        public SettingsService(IRepository<ThemeSettingsRecord> repository) {
            _repository = repository;
            _localConfiguration = new LocalWebConfigManager();
        }

        public ThemeSettingsRecord GetSettings() {
            var settings = _repository.Table.SingleOrDefault();
            if (settings == null) {
                _repository.Create(settings = new ThemeSettingsRecord());
            }

            return settings;
        }


        public IList<ThemeInfoViewModel> GetThemes() {
            var config = _localConfiguration.GetConfiguration("~/Themes/Laser.Bootstrap");
            var themeDefList = config.AppSettings.Settings.AllKeys.Where(w => w.StartsWith("themedef-")).Select(
                s => new ThemeInfoViewModel {
                    Name = s.Replace("themedef-",""),
                    FileName = config.AppSettings.Settings[s].Value,
                }).ToList();
            return themeDefList;
        }

    }
}
