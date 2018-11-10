using Laser.Orchard.CulturePicker.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using System;
using System.Linq;

namespace Laser.Orchard.CulturePicker.Projections
{
    public interface IFilterProvider : IEventHandler
    {
        void Describe(dynamic describe);
    }

    public class CurrentCultureFilter : IFilterProvider
    {
        private readonly ICultureManager _cultureManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;
        public CurrentCultureFilter(IWorkContextAccessor workContextAccessor, ICultureManager cultureManager, IContentManager contentManager)
        {
            _workContextAccessor = workContextAccessor;
            _cultureManager = cultureManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        #region IFilterProvider Members

        public void Describe(dynamic describe)
        {
            Version orchardVersion = Utils.GetOrchardVersion();
            describe.For("Localization", T("Localization"), T("Localization"))
                .Element("ForCurrentCultureOrDefault", T("For current or default culture"), T("Localized content items for current or default culture"),
                         (Action<dynamic>)ApplyFilter,
                         (Func<dynamic, LocalizedString>)DisplayFilter,
                         null
                );
        }

        #endregion

        public void ApplyFilter(dynamic context)
        {
            string siteCulture = _workContextAccessor.GetContext().CurrentSite.SiteCulture;
            int siteCultureId = _cultureManager.GetCultureByName(siteCulture).Id;
            string currentCulture = _workContextAccessor.GetContext().CurrentCulture;
            var currentCultureRecord = _cultureManager.GetCultureByName(currentCulture);
            int currentCultureId = siteCultureId;
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
                var listIdsQ = _contentManager.HqlQuery()
                    .ForPart<LocalizationPart>()
                    .Where(alias => alias.ContentPartRecord<LocalizationPartRecord>(), x => x.Or(expr => expr.Eq("CultureId", currentCultureId), expr2 => expr2.Eq("CultureId", 0))).List();
                var listIds = new int[] { 0 };
                if (listIdsQ != null)
                {
                    listIds = listIdsQ.Where(w => w.MasterContentItem != null).Select(s => s.MasterContentItem.Id).ToArray();
                }

                context.Query = query.Where(x => x.ContentPartRecord<LocalizationPartRecord>(),
                                            x => x.Or(
                                                        y => y.Or(
                                                            z => z.Eq("CultureId", currentCultureId),
                                                            h => h.And(
                                                                       z => z.Eq("CultureId", siteCultureId),
                                                                       z => z.Not(a => a.InG("Id", listIds))
                                                                       )
                                                                  ),
                                                        y => y.Eq("CultureId", 0)
                                                     )
                                            );
            }
        }

        public LocalizedString DisplayFilter(dynamic context)
        {
            return T("For current or default culture");
        }
    }
}