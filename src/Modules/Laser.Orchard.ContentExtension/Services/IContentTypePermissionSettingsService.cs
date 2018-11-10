using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Laser.Orchard.ContentExtension.Models;
using Orchard.Data;
//using Orchard.Localization.Services;
using Orchard;

namespace Laser.Orchard.ContentExtension.Services {
    public interface IContentTypePermissionSettingsService : IDependency {
        SettingsModel ReadSettings();
        void WriteSettings(SettingsModel settings);
    }
    public class ContentTypePermissionSettingsService : IContentTypePermissionSettingsService {
        private readonly IContentManager _contentManager;
        //  private readonly ICultureManager _cultureManager;
        private readonly IWorkContextAccessor _work;
        //     private readonly ILocalizationService _localizationService;
        //    private readonly IRepository<ExtendedCultureRecord> _extendedCultureRepository;
        private readonly IRepository<ContentTypePermissionRecord> _settingsRepository;


        public ContentTypePermissionSettingsService(
            //ILocalizationService localizationService, 
            // ICultureManager cultureManager, 
            IContentManager contentManager,
            // IRepository<ExtendedCultureRecord> repository, 
            IWorkContextAccessor work, IRepository<ContentTypePermissionRecord> settingsRepository) {
            // _localizationService = localizationService;
            //  _cultureManager = cultureManager;
            _contentManager = contentManager;
            // _extendedCultureRepository = repository;
            _settingsRepository = settingsRepository;
            _work = work;
        }

        public SettingsModel ReadSettings() {
            //  var settingsRecord = _settingsRepository.Table.Count();
            var settings = new SettingsModel();
            var gri = _settingsRepository.Fetch(x => x.Id > 0).Select(x => x);
            settings.ListContPermission = gri.ToList();//.Single();
            return settings;
        }

        public void WriteSettings(SettingsModel settings) {
            foreach (var contperm in settings.ListContPermission) {
                this.UpdateContPerm(contperm);
            }
            //  _settingsRepository.Update(settings.ListContPermission);
        }


        public void UpdateContPerm(ContentTypePermissionRecord contpermrec) {
            _settingsRepository.Flush();
            var record = _settingsRepository.Get(o => o.Id.Equals(contpermrec.Id));
            if (record != null) {
                if (contpermrec.ContentType == " ")
                    _settingsRepository.Delete(record);
                else
                    _settingsRepository.Update(contpermrec);

            }
            else {
                if (contpermrec.ContentType != " ")
                _settingsRepository.Create(contpermrec);
            }

            record = null;
        }


        //public IList<ExtendedCultureRecord> CultureList(IEnumerable<string> cultureListSource = null, bool ordered = true) {
        //    _extendedCultureRepository.Flush();
        //    List<string> listCultures;
        //    if (cultureListSource == null)
        //        listCultures = _cultureManager.ListCultures().ToList();
        //    else {
        //        listCultures = cultureListSource.ToList();
        //    }
        //    var siteCulture = _cultureManager.GetSiteCulture();
        //    var listExtendedCultures = _extendedCultureRepository.Table.ToList();
        //    var joinedCultures = from c in listCultures
        //                         join
        //                             ec in listExtendedCultures on c.ToString() equals ec.CultureCode into temp_ec
        //                         from ec in temp_ec.DefaultIfEmpty()
        //                         select new ExtendedCultureRecord { CultureCode = c.ToString(), Id = (ec != null ? ec.Id : 0), Priority = (ec != null ? ec.Priority : (c.ToString() == siteCulture ? 0 : 1)), DisplayName = (ec != null ? ec.DisplayName : null) };

        //    if (ordered)
        //        return (joinedCultures.OrderBy(o => o.Priority).ToList());
        //    else
        //        return (joinedCultures.ToList());

        //}

        //public ExtendedCultureRecord GetExtendedCulture(string culture) {
        //    var record = _extendedCultureRepository.Get(o => o.CultureCode.Equals(culture));
        //    return record;
        //}
    }
}