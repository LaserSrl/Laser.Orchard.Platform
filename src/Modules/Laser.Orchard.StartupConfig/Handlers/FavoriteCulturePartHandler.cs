using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization.Records;
using Orchard;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class FavoriteCulturePartHandler: ContentHandler {

        IRepository<CultureRecord> _cultureRepository;
        IOrchardServices _orchardService;
        public FavoriteCulturePartHandler(IRepository<FavoriteCulturePartRecord> repository, IRepository<CultureRecord> cultureRepository, IOrchardServices orchardService) {
            _cultureRepository = cultureRepository;
            _orchardService = orchardService;

            Filters.Add(StorageFilter.For(repository));

            OnUpdated<FavoriteCulturePart>((context, part) => UpdateCulture(part));

            OnLoaded<FavoriteCulturePart>((context, part) => LoadCulture(part));
        }

        private void LoadCulture(FavoriteCulturePart part) {
          var culturerecord=  _cultureRepository.Table.Where(r => r.Id == part.Culture_Id).ToList().FirstOrDefault();
            if (culturerecord != null)
                part.Culture = culturerecord.Culture;
            else
                part.Culture = _cultureRepository.Table.Where(r => r.Culture == _orchardService.WorkContext.CurrentCulture)
                    .ToList().FirstOrDefault().Culture;
        }

        private void UpdateCulture(FavoriteCulturePart part) {
            bool isSiteCulture = true;            
            if(part.Culture != null) {
                CultureRecord contentCulture = _cultureRepository.Table.Where(r => r.Culture == part.Culture)
                 .ToList().FirstOrDefault();
                if (contentCulture != null) {
                    isSiteCulture = false;
                    part.Culture = contentCulture.Culture;
                    part.Culture_Id = contentCulture.Id;
                }               
            } 

            if(isSiteCulture){
                CultureRecord siteCulture = _cultureRepository.Table.Where(r => r.Culture == _orchardService.WorkContext.CurrentCulture)
                    .ToList().FirstOrDefault();
                part.Culture_Id = siteCulture.Id;
                part.Culture = siteCulture.Culture;

            }
        }
    }
}