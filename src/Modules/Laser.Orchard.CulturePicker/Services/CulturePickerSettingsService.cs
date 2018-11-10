using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Laser.Orchard.CulturePicker.Models;
using Orchard.Data;
using Orchard.Localization.Services;
using Orchard;

namespace Laser.Orchard.CulturePicker.Services {
    public class CulturePickerSettingsService : ICulturePickerSettingsService {
        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;
        private readonly IWorkContextAccessor _work;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<ExtendedCultureRecord> _extendedCultureRepository;
        private readonly IRepository<SettingsRecord> _settingsRepository;


        public CulturePickerSettingsService(ILocalizationService localizationService, ICultureManager cultureManager, IContentManager contentManager, IRepository<ExtendedCultureRecord> repository, IWorkContextAccessor work, IRepository<Models.SettingsRecord> settingsRepository) {
            _localizationService = localizationService;
            _cultureManager = cultureManager;
            _contentManager = contentManager;
            _extendedCultureRepository = repository;
            _settingsRepository = settingsRepository;
            _work = work;
        }

        public CulturePicker.Models.SettingsModel ReadSettings() {
            var settingsRecord = _settingsRepository.Table.Count();
            if (settingsRecord == 0) {
                var newSettings = new CulturePicker.Models.SettingsRecord();
                newSettings.ShowOnlyPertinentCultures = false;
                _settingsRepository.Create(newSettings);
                newSettings = null;
            }
            var settings = new Models.SettingsModel();
            settings.Settings = _settingsRepository.Table.Single();
            settings.ExtendedCulturesList = this.CultureList(null, true);
            return settings;
        }

        public void WriteSettings(CulturePicker.Models.SettingsModel settings) {
            foreach (var culture in settings.ExtendedCulturesList) {
                this.UpdateCulture(culture);
            }
            _settingsRepository.Update(settings.Settings);
        }

        public IList<ExtendedCultureRecord> CultureList(IEnumerable<string> cultureListSource = null, bool ordered = true) {
            _extendedCultureRepository.Flush();
            List<string> listCultures;
            if (cultureListSource == null)
                listCultures = _cultureManager.ListCultures().ToList();
            else {
                listCultures = cultureListSource.ToList();
            }
            var siteCulture = _cultureManager.GetSiteCulture();
            var listExtendedCultures = _extendedCultureRepository.Table.ToList();
            var joinedCultures = from c in listCultures
                                 join
                                     ec in listExtendedCultures on c.ToString() equals ec.CultureCode into temp_ec
                                 from ec in temp_ec.DefaultIfEmpty()
                                 select new ExtendedCultureRecord { CultureCode = c.ToString(), Id = (ec != null ? ec.Id : 0), Priority = (ec != null ? ec.Priority : (c.ToString() == siteCulture ? 0 : 1)), DisplayName = (ec != null ? ec.DisplayName : null) };

            if (ordered)
                return (joinedCultures.OrderBy(o => o.Priority).ToList());
            else
                return (joinedCultures.ToList());

        }

        public void UpdateCulture(ExtendedCultureRecord culture) {
            _extendedCultureRepository.Flush();
            var record = _extendedCultureRepository.Get(o => o.CultureCode.Equals(culture.CultureCode));
            if (record != null) {
                //record.CultureCode = culture.CultureCode;
                //record.Priority = culture.Priority;
                //record.DisplayName = culture.DisplayName != "" ? culture.DisplayName : null;
                _extendedCultureRepository.Update(culture);
            } else {
                //record = new ExtendedCultureRecord();
                //record.CultureCode = culture.CultureCode;
                //record.DisplayName = culture.DisplayName != "" ? culture.DisplayName : null;
                //record.Priority = culture.Priority;
                _extendedCultureRepository.Create(culture);
            }

            record = null;
        }

        public ExtendedCultureRecord GetExtendedCulture(string culture) {
            var record = _extendedCultureRepository.Get(o => o.CultureCode.Equals(culture));
            return record;
        }
    }
}