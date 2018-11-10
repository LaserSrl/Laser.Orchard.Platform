using System.Linq;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement.Shapes {
	[OrchardFeature("Laser.Orchard.TemplateManagement")]
	public class ParserPickerShape : IShapeTableProvider {
		private readonly Work<ITemplateService> _templateService;

		public ParserPickerShape(Work<ITemplateService> templateService) {
			_templateService = templateService;
		}

		public void Discover(ShapeTableBuilder builder) {
			builder.Describe("ParserPicker").OnDisplaying(context => {
				context.Shape.Parsers = _templateService.Value.GetParsers().ToList();
			});
		}
	}
}