using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.Faq.Models;
using Laser.Orchard.Faq.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization.Models;
using Orchard.Localization.Records;
using Orchard.Localization.Services;

namespace Laser.Orchard.Faq.Services {
    public class FaqTypeService : IFaqTypeService {
        private readonly IRepository<FaqTypePartRecord> _FaqTypeRepository;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _worckContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ICommonsServices _commonsServices;

        public FaqTypeService(IRepository<FaqTypePartRecord> FaqTypeRepository,
            IContentManager contentManager,
            IWorkContextAccessor worckContextAccessor,
            ILocalizationService localizationService,
            ICommonsServices commonsServices
            ) {
            _FaqTypeRepository = FaqTypeRepository;
            _contentManager = contentManager;
            _worckContextAccessor = worckContextAccessor;
            _localizationService = localizationService;
            _commonsServices = commonsServices;
        }

        public List<FaqTypePartRecord> GetFaqTypes() {
            return GetFaqTypes(true);

        }

        public FaqTypePartRecord GetFaqType(int id) {
            return _FaqTypeRepository.Get(p => p.Id == id) ?? new FaqTypePartRecord();
        }

        public bool TypeNameAlredyExists(string name) {
            return _FaqTypeRepository.Table.Where(fp => fp.Title == name).Count() >= 2;
        }

        public List<FaqTypePartRecord> GetFaqTypes(bool filterOnCurrentCulture) {
            if (filterOnCurrentCulture) {
                var culture = _worckContextAccessor.GetContext().CurrentCulture;
                var idCurrentCulture = _commonsServices.ListCultures().Where(w => w.Culture == culture).Select(s => s.Id).FirstOrDefault();
                var list = _contentManager.Query<LocalizationPart, LocalizationPartRecord>("FaqType")
                    .ForVersion(VersionOptions.Published)
                    .Where(w => w.CultureId == idCurrentCulture || w.CultureId==0)
                    .List<FaqTypePart>();
                if (list == null)
                    return _FaqTypeRepository.Table.ToList();
                //(VersionOptions.Published).ForType("FaqType").Where(s=>s.
                return list.Select(s => s.Record).ToList();
            } else {
                return _FaqTypeRepository.Table.ToList();
            }
        }
    }
}