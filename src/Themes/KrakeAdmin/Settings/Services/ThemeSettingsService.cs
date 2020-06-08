using KrakeAdmin.Settings.Models;
using KrakeAdmin.Settings.ViewModels;
using Orchard;
using Orchard.Caching;
using Orchard.Data;
using System.Linq;

namespace KrakeAdmin.Settings.Services {
    public interface IThemeSettingsService : IDependency {
        ThemeSettingsRecord GetSettingsRecord();
        ThemeSettingsViewModel GetSettings();
        void UpdateSettingsRecord(ThemeSettingsViewModel viewModel);

    }
    public class ThemeSettingsService : IThemeSettingsService {

        private readonly IRepository<ThemeSettingsRecord> _repository;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        public ThemeSettingsService(IRepository<ThemeSettingsRecord> repository, ICacheManager cacheManager, ISignals signals) {
            _repository = repository;
            _cacheManager = cacheManager;
            _signals = signals;
        }
        public ThemeSettingsRecord GetSettingsRecord() {
            var settings = _repository.Table.SingleOrDefault();
            if (settings == null) {
                _repository.Create(settings = new ThemeSettingsRecord());
            }
            return settings;
        }


        public ThemeSettingsViewModel GetSettings() {
            return _cacheManager.Get("KrakeAdminSettingsCache", true, context => {
                context.Monitor(_signals.When("KrakeAdminSettingsChanged"));
                var settings = GetSettingsRecord();
                return settings.ToThemeSettingsViewModel();
            });
        }

        public void UpdateSettingsRecord(ThemeSettingsViewModel viewModel) {
            var settings = this.GetSettingsRecord();
            settings.HeaderLogoUrl = viewModel.HeaderLogoUrl;
            settings.BaseLineText = viewModel.BaseLineText;
            settings.PlaceholderLogoUrl = viewModel.PlaceholderLogoUrl;
            settings.PlaceholderSmallLogoUrl = viewModel.PlaceholderSmallLogoUrl;
            _signals.Trigger("KrakeAdminSettingsChanged");
        }


    }

    public static class ThemeSettingsRecordExtensions {
        public static ThemeSettingsViewModel ToThemeSettingsViewModel(this ThemeSettingsRecord record) {
            return new ThemeSettingsViewModel() {
                HeaderLogoUrl = record.HeaderLogoUrl,
                BaseLineText = record.BaseLineText,
                PlaceholderLogoUrl = record.PlaceholderLogoUrl,
                PlaceholderSmallLogoUrl = record.PlaceholderSmallLogoUrl
            };
        }
    }
}
