using System;
using Laser.Orchard.CulturePicker.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;

namespace Laser.Orchard.CulturePicker.Projections {
    public class CurrentCultureWithUntranslatedFilter : IFilterProvider
    {
        private readonly ICultureManager _cultureManager;
        private readonly IWorkContextAccessor _workContextAccessor;


        public CurrentCultureWithUntranslatedFilter(IWorkContextAccessor workContextAccessor, ICultureManager cultureManager)
        {
            _workContextAccessor = workContextAccessor;
            _cultureManager = cultureManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        #region IFilterProvider Members

        public void Describe(dynamic describe)
        {
            Version orchardVersion = Utils.GetOrchardVersion();
            describe.For("Localization", T("Localization"), T("Localization"))
                .Element("ForCurrentCultureWithUntranslated", T("For current culture (include untranslated in default)"), T("Localized content items for current culture (untranslated included in default culture)"),
                         (Action<dynamic>)ApplyFilter,
                         (Func<dynamic, LocalizedString>)DisplayFilter,
                         null
                );
        }

        #endregion

        public void ApplyFilter(dynamic context)
        {
            string currentCulture = _workContextAccessor.GetContext().CurrentCulture;
            string siteCulture = _workContextAccessor.GetContext().CurrentSite.SiteCulture;
            int siteCultureId = _cultureManager.GetCultureByName(siteCulture).Id;
            int currentCultureId = siteCultureId;
            var currentCultureRecord=_cultureManager.GetCultureByName(currentCulture);
            if (currentCultureRecord != null) {
                currentCultureId = currentCultureRecord.Id;
            }
            var query = (IHqlQuery)context.Query;
            if (currentCultureId == siteCultureId)
            {
                context.Query = query.Where(x => x.ContentPartRecord<LocalizationPartRecord>(), x => x.Or(c => c.Eq("CultureId", currentCultureId), nc => nc.Eq("CultureId", 0)));
            }
            else
            {
                context.Query = query.Where(x => x.ContentPartRecord<LocalizationPartRecord>(), c => c.Eq("CultureId", currentCultureId));
            }
        }

        public LocalizedString DisplayFilter(dynamic context)
        {
            return T("For current culture (include untranslated in default)");
        }
    }
}