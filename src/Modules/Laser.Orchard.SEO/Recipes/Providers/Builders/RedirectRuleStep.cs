using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Recipes.Services;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Laser.Orchard.SEO.Recipes.Providers.Builders {
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectRuleStep : RecipeBuilderStep {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionWriter _contentDefinitionWriter;
        private readonly IRedirectService _redirectService;

        public RedirectRuleStep(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IContentDefinitionWriter contentDefinitionWriter,
            IRedirectService redirectService) {

            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _contentDefinitionWriter = contentDefinitionWriter;
            _redirectService = redirectService;
        }
        public override LocalizedString Description {
            get { return T("Exports Redirects. If there are 2 redirects in the exported file with the same SourceUrl, only one will be inserted during the import"); }
        }

        public override LocalizedString DisplayName {
            get { return T("Redirects Rule"); }
        }

        public override string Name {
            get { return "Redirects"; }
        }

        // Priority is used to order steps during execution. Steps at higher priority are executed first.
        public override int Priority { get { return 20; } }

        // Position is used to order steps when displaying them in the UI. We set it lower than
        // what is defined for the ContentStep, so that this step is shown before.
        public override int Position { get { return 15; } }

        public override void Build(BuildContext context) {
            // Export the Redirects: This adds to the exported xml commands to recreate the records.
            context.RecipeDocument.Element("Orchard")
                .Add(ExportInternalRecordsCommands());
        }

        private XElement ExportInternalRecordsCommands() {
            var result = _redirectService.GetRedirects();
            if (result.Count() > 0) {
                var root = new XElement("Command");
                root.Add(Environment.NewLine);
                foreach (var red in result) {
                    root.Add($"redirects import {red.CreatedDateTime.Ticks} {red.SourceUrl} {red.DestinationUrl} {red.IsPermanent} " + Environment.NewLine);
                }
                return root;
            }
            else
                return null;
        }
    }
}