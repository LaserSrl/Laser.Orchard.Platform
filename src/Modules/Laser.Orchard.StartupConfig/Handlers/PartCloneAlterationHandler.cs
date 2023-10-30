using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Autoroute.Models;
using Orchard.Core.Common.Models;
using Orchard;
using Orchard.Environment;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class PartCloneAlterationHandler : ContentHandler {
        private readonly LocalizationService _localizationService;
        private readonly IOrchardServices _orchardService;

        public PartCloneAlterationHandler(LocalizationService localizationService, IOrchardServices orchardService) {
            _localizationService = localizationService;
            _orchardService = orchardService;
            OnCloning<CommonPart>(CommonPartClone);
            OnCloning<AutoroutePart>(AutoroutePartClone);
            OnCloned<LocalizationPart>(LocalizationPartClone);
        }

        private void CommonPartClone(CloneContentContext context, CommonPart part) {
            var clonedPart = context.CloneContentItem.As<CommonPart>();
            if (clonedPart != null) {
                clonedPart.CreatedUtc = DateTime.UtcNow;
                clonedPart.ModifiedUtc = DateTime.UtcNow;
                clonedPart.PublishedUtc = null;
                clonedPart.VersionModifiedBy = _orchardService.WorkContext.CurrentUser?.UserName ?? "";
            }
        }

        private void AutoroutePartClone(CloneContentContext context, AutoroutePart part) {
            var clonedPart = context.CloneContentItem.As<AutoroutePart>();
            if (clonedPart != null) {
                clonedPart.DisplayAlias = "";
                clonedPart.PromoteToHomePage = false;
            }
        }

        private void LocalizationPartClone(CloneContentContext context, LocalizationPart part) {
            var clonedPart = context.CloneContentItem.As<LocalizationPart>();
            // We prevent that, cloning a content, the cloned content results as a duplicated translation of the orginal content
            if (clonedPart != null) {
                if (_localizationService.GetLocalizations(clonedPart, VersionOptions.Latest)
                    .Any(l => l.Culture.Culture == clonedPart.Culture.Culture && l.Id != clonedPart.Id)) {
                    clonedPart.MasterContentItem = clonedPart;
                }
            }
        }
    }
}