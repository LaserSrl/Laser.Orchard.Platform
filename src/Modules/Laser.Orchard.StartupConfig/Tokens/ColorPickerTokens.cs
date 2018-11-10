using System;
using System.Linq;
using Orchard.Taxonomies.Fields;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Orchard.Fields.Fields;
using Orchard.ContentManagement.Aspects;
using Laser.Orchard.StartupConfig.ColorPicker;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class ColorPickerTokens : ITokenProvider {
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }
        public ColorPickerTokens(IOrchardServices orchardServices) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
        }
        public void Describe(DescribeContext context) {

        }
        public void Evaluate(EvaluateContext context) {
            //((dynamic)context.Data["Content"]).CommonPart.Creator.Value
            context.For<ColorPickerField>("ColorPickerField")
                            .Token("Color", field => field.Value ?? "")
                            .Chain("Color", "Text", field => field.Value ?? "");
        }

    }
}