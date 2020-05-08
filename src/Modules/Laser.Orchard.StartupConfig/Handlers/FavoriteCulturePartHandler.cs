using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization.Records;
using Orchard;
using Laser.Orchard.StartupConfig.Services;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class FavoriteCulturePartHandler : ContentHandler {

        ICommonsServices _commonService;
        IOrchardServices _orchardService;
        public FavoriteCulturePartHandler(IRepository<FavoriteCulturePartRecord> repository, ICommonsServices commonService, IOrchardServices orchardService) {
            _commonService = commonService;
            _orchardService = orchardService;

            Filters.Add(StorageFilter.For(repository));

            OnUpdated<FavoriteCulturePart>((context, part) => UpdateCulture(part));

            OnLoaded<FavoriteCulturePart>((context, part) => LoadCulture(part));
        }

        private void LoadCulture(FavoriteCulturePart part) {
            var culture = _commonService.ListCultures().SingleOrDefault(x => x.Id == part.Culture_Id);
            if (culture != null)
                part.Culture = culture.Culture;
            else
                part.Culture = _orchardService.WorkContext.CurrentCulture;
        }

        private void UpdateCulture(FavoriteCulturePart part) {
            if (string.IsNullOrWhiteSpace(part.Culture)) {
                var culture = _commonService.ListCultures().SingleOrDefault(x => x.Culture == _orchardService.WorkContext.CurrentCulture);
                part.Culture = culture != null ? culture.Culture : "";
                part.Culture_Id = culture != null ? culture.Id: 0;
            }
        }
    }
}