using KrakeAdmin.Settings.Models;
using Orchard;
using Orchard.Data;
using System.Linq;

namespace KrakeAdmin.Settings.Services {
    public interface IThemeSettingsService : IDependency {
        ThemeSettingsRecord GetSettings();
    }
    public class ThemeSettingsService : IThemeSettingsService {
        private readonly IRepository<ThemeSettingsRecord> _repository;
        private ThemeSettingsRecord _setttingsRecord;

        public ThemeSettingsService(IRepository<ThemeSettingsRecord> repository) {
            _repository = repository;
        }

        public ThemeSettingsRecord GetSettings() {
            if (_setttingsRecord == null) {
                _setttingsRecord = _repository.Table.SingleOrDefault();
                if (_setttingsRecord == null) {
                    _repository.Create(_setttingsRecord = new ThemeSettingsRecord());
                }
            }
            return _setttingsRecord;
        }

    }

}
