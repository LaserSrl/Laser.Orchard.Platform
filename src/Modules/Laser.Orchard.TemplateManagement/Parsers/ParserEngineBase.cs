using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;

namespace Laser.Orchard.TemplateManagement.Parsers {
	public abstract class ParserEngineBase : Component, IParserEngine {
		public virtual string Id {
			get { return GetType().FullName; }
		}

		public virtual string DisplayText {
			get { return GetType().Name; }
		}

		public abstract string LayoutBeacon { get; }
        public abstract string ParseTemplate(TemplatePart template, ParseTemplateContext context);
	}
}