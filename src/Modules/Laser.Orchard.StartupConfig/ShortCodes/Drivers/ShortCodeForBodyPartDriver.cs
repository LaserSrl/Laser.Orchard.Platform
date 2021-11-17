using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes.Drivers {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ShortCodeForBodyPartDriver : ContentPartDriver<BodyPart> {
        private const string TEMPLATE_NAME= "ShortCodes/Parts/ShortCodes_Edit";
        private readonly IEnumerable<IShortCodeProvider> _providers;

        public ShortCodeForBodyPartDriver(IEnumerable<IShortCodeProvider> providers) {
            _providers = providers;
        }

        protected override string Prefix { get { return "ShortCodes"; } }

        protected override DriverResult Editor(BodyPart part, dynamic shapeHelper) {
            var viewModel = new ShortCodes.ViewModels.ShortCodesEditor {
                ContentType = part.ContentItem.ContentType,
                Part = part,
                ElementId = "Body_Text",
                ElementFlavor = part.Settings.GetModel<BodyTypePartSettings>().Flavor,
                Descriptors = _providers.Select(x=>x.Describe()).OrderBy(x=>x.Name)
            };
            return ContentShape("Parts_ShortCodes_Edit", 
                                () => shapeHelper.EditorTemplate(TemplateName: TEMPLATE_NAME,
                                Model: viewModel, Prefix: Prefix)
            );
        }
    }
}