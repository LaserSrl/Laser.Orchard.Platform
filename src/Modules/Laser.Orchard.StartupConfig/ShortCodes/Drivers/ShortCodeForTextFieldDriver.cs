using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Settings;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes.Drivers {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ShortCodeForTextFieldDriver : ContentFieldDriver<TextField> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private const string TEMPLATE_NAME = "ShortCodes/ShortCodes_Edit";
        private readonly IShortCodesService _shortcodesServices;

        public ShortCodeForTextFieldDriver(IOrchardServices services, IEnumerable<IHtmlFilter> htmlFilters, IShortCodesService shortcodesServices) {
            _htmlFilters = htmlFilters;
            Services = services;
            _shortcodesServices = shortcodesServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        private static string GetPrefix(TextField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private string GetDifferentiator(TextField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Editor(ContentPart part, TextField field, dynamic shapeHelper) {
            var shortCodeContext = new DescribeContext {
                Host = new HostInfos {
                    ContentId = part.Id,
                    ContentType = part.ContentItem.ContentType,
                    Part = part,
                    Field = field
                }
            };
            var viewModel = new ShortCodes.ViewModels.ShortCodesEditor {
                ContentType = part.ContentItem.ContentType,
                Part = part,
                Field = field,
                ElementName = GetPrefix(field, part) + ".Text",
                ElementFlavor = field.PartFieldDefinition.Settings.GetModel<TextFieldSettings>().Flavor,
                Descriptors = _shortcodesServices
                                    .GetEnabledProviders(shortCodeContext)
                                    .Select(x => x.Describe(shortCodeContext))
                                    .OrderBy(x => x.Name)
            };
            if (viewModel.Descriptors != null && viewModel.Descriptors.Count() > 0) {
                return ContentShape("Parts_ShortCodes_Edit",
                                    () => shapeHelper.EditorTemplate(TemplateName: TEMPLATE_NAME,
                                    Model: viewModel, Prefix: Prefix));
            }
            return null;
        }

    }
}