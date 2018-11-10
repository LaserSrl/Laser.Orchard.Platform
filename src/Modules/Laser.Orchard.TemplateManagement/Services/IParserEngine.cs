using Laser.Orchard.TemplateManagement.Models;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.TemplateManagement.Services {

    public interface IParserEngine : IDependency {
        string Id { get; }
        string DisplayText { get; }
        string LayoutBeacon { get; }

        string ParseTemplate(TemplatePart template, ParseTemplateContext context);
    }

    public class ParseTemplateContext {
        public object Model { get; set; }
        public Dictionary<string, object> ViewBag { get; set; }
    }
}