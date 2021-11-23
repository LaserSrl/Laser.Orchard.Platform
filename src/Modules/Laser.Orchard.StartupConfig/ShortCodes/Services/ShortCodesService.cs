using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using Laser.Orchard.StartupConfig.ShortCodes.Settings.Models;
using Laser.Orchard.StartupConfig.ShortCodes.Settings.ViewModels;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes.Services {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ShortCodesService : IShortCodesService {
        private readonly IEnumerable<IShortCodeProvider> _providers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly DescribeContext _emptyContext;
        public ShortCodesService(IEnumerable<IShortCodeProvider> providers, IContentDefinitionManager contentDefinitionManager) {
            _providers = providers;
            _contentDefinitionManager = contentDefinitionManager;
            _emptyContext = new DescribeContext();
        }

        public IEnumerable<IShortCodeProvider> GetProviders() {
            return _providers;
        }

        public IEnumerable<IShortCodeProvider> GetEnabledProviders(DescribeContext context) {
            if (context == null) {
                context = _emptyContext;
            }
            if (context.Host.Field != null) { //It's a field
                ShortCodesSettingsViewModel settings = new ShortCodesSettingsViewModel();
                ContentPartFieldDefinition defintion = context.Host.Field.PartFieldDefinition;
                if (defintion != null) {
                    settings.Populate(defintion.Settings.GetModel<ShortCodesSettings>(), null);
                }
                return _providers.Where(x => settings.EnabledShortCodes.Contains(x.Describe(context).Name));
            }
            if (!string.IsNullOrWhiteSpace(context.Host.ContentType) && context.Host.Part != null) { //It's a part
                ShortCodesSettingsViewModel settings = new ShortCodesSettingsViewModel();
                ContentTypePartDefinition defintion = context.Host.Part.TypePartDefinition;
                if (defintion != null) {
                    settings.Populate(defintion.Settings.GetModel<ShortCodesSettings>(), null);
                }
                return _providers.Where(x => settings.EnabledShortCodes.Contains(x.Describe(context).Name));
            }
            return _providers;
        }

    }
}