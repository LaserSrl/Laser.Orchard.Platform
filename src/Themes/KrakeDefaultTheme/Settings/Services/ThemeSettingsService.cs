using KrakeDefaultTheme.Settings.Models;
using Orchard;
using Orchard.Data;
using System.Linq;

namespace KrakeDefaultTheme.Settings.Services {
    public interface IThemeSettingsService : IDependency {
        ThemeSettingsRecord GetSettings();
    }
    public class ThemeSettingsService : IThemeSettingsService {
        private readonly IRepository<ThemeSettingsRecord> _repository;

        public ThemeSettingsService(IRepository<ThemeSettingsRecord> repository) {
            _repository = repository;
        }

        public ThemeSettingsRecord GetSettings() {
            var settings = _repository.Table.SingleOrDefault();
            if (settings == null) {
                _repository.Create(settings = new ThemeSettingsRecord());
            }

            return settings;
        }

    }

}
