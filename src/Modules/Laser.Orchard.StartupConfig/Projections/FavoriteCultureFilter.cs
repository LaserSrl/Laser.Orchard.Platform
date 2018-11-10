using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using System;
using System.Globalization;
using Laser.Orchard.StartupConfig.Models;
using Orchard.Localization.Services;

namespace Laser.Orchard.StartupConfig.Projections {
    public class FavoriteCultureFilter : IFilterProvider {
        private readonly ICultureManager _cultureManager;

        public FavoriteCultureFilter(ICultureManager cultureManager) {
            T = NullLocalizer.Instance;
            _cultureManager = cultureManager;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("Localization", T("Localization"), T("Localization"))
                .Element("FavoriteCulture", T("Favorite Culture"), T("Search for Content Items associated with a favorite culture."),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "FavoriteCultureForm"
                );
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;
            if (context.State != null)
                if (context.State.FavoriteCulture != null && context.State.FavoriteCulture != "") {
                    var cultureId = 0;
                    if (!int.TryParse(context.State.FavoriteCulture.Value.ToString(), out cultureId)) {
                        try {
                            cultureId = _cultureManager.GetCultureByName(context.State.FavoriteCulture.Value).Id;
                        } catch { // Nel caso in cui non riesca a recuperare la cultura dalla stringa, allora considera la culture nulla e non restituisce nessun record

                        }
                    }
                    context.Query = query.Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Eq("Culture_Id", cultureId));
                }
            return;
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Content Items associated with {0} as favorite culture.", context.State.FavoriteCulture);
        }
    }
}