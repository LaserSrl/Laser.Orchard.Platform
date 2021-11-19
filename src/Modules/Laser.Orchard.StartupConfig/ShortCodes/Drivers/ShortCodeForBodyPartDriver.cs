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
        private const string TEMPLATE_NAME = "ShortCodes/Parts/ShortCodes_Edit";
        private readonly IShortCodesServices _shortcodesServices;

        public ShortCodeForBodyPartDriver(IShortCodesServices shortcodesServices) {
            _shortcodesServices = shortcodesServices;
        }

        protected override string Prefix { get { return "ShortCodes"; } }

        protected override DriverResult Editor(BodyPart part, dynamic shapeHelper) {
            var shortCodeContext = new DescribeContext {
                Host = new HostInfos {
                    ContentId = part.Id,
                    ContentType = part.ContentItem.ContentType,
                    Part = part
                }
            };
            var viewModel = new ShortCodes.ViewModels.ShortCodesEditor {
                ContentType = part.ContentItem.ContentType,
                Part = part,
                ElementId = "Body_Text",
                ElementFlavor = part.Settings.GetModel<BodyTypePartSettings>().Flavor,
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