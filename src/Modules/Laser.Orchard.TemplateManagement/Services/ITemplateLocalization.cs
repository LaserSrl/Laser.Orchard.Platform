using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.UsersExtensions.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;

namespace Laser.Orchard.TemplateManagement.Services {
    public interface ITemplateLocalization : IDependency {
        TemplatePart GetTemplate(int id, string lang);
        TemplatePart GetTemplateByUserEmailFavoriteCulture(int id, string email);
    }
    [OrchardFeature("Laser.Orchard.TemplateLocalization")]
    public class TemplateLocalization : ITemplateLocalization {
        private readonly IOrchardServices _services;
        private readonly ILocalizationService _localizationService;
        private readonly IUsersExtensionsServices _usersExtensionsServices;
        public TemplateLocalization(
            ILocalizationService localizationService,
            IOrchardServices services,
            IUsersExtensionsServices usersExtensionsServices) {
            _services = services;
            _localizationService = localizationService;
            _usersExtensionsServices = usersExtensionsServices;
        }
        public TemplatePart GetTemplateByUserEmailFavoriteCulture(int id, string email) {
            var user = _usersExtensionsServices.GetUserByMail(email.Split(';')[0]);
            return GetTemplate(id, user.ContentItem.As<FavoriteCulturePart>().Culture);
       }
        public TemplatePart GetTemplate(int id, string culture) {
            var templateCi = _services.ContentManager.Get(id);
            var localizationPart = _localizationService.GetLocalizedContentItem(templateCi, culture);
            return localizationPart.ContentItem.As<TemplatePart>();
        }
    }
}