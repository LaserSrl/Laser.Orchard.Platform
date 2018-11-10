using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Voting.Functions;
using Contrib.Voting.Models;
using Contrib.Voting.Services;
using Contrib.Voting.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;

namespace Contrib.Voting.Drivers {
    public class VoteWidgetPartDriver : ContentPartCloningDriver<VoteWidgetPart> {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IRepository<ResultRecord> _resultRepository;
        private readonly IEnumerable<IFunction> _functions;

        public VoteWidgetPartDriver(
            IContentManager contentManager, 
            IContentDefinitionManager contentDefinitionManager,
            IRepository<ResultRecord> resultRepository,
            IEnumerable<IFunction> functions) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _resultRepository = resultRepository;
            _functions = functions;
        }

        protected override DriverResult Display(VoteWidgetPart part, string displayType, dynamic shapeHelper) {
            // retrieve all content types implementing a specific content part

            Action<Orderable<ResultRecord>> order;
            if (part.Ascending){
                order = x => x.Asc(r => r.Value);
            } else {
                order = x => x.Asc(r => r.Value);
            }

            // filter results based on Widget's parameters
            var resultPredicate = PredicateBuilder.True<ResultRecord>();
            resultPredicate = resultPredicate.And(r => r.FunctionName == part.FunctionName);    

            // filter on dimension
            if(String.IsNullOrEmpty(part.Dimension)) {
                resultPredicate = resultPredicate.And(
                    r => r.Dimension == null || r.Dimension == String.Empty);
            }
            else {
                resultPredicate = resultPredicate.And(
                    r => r.Dimension == part.Dimension);                
            }

            // filter on content type
            if(!String.IsNullOrEmpty(part.ContentType)) {
                resultPredicate = resultPredicate.And(r => r.ContentType == part.ContentType);
            }

            var results = _resultRepository.Fetch(resultPredicate, order, 0, part.Count).Select(r => r.ContentItemRecord.Id);

            // build the Summary display for each content item
            var list = shapeHelper.List();
            list.AddRange(results.Select(id => _contentManager.BuildDisplay(_contentManager.Get(id), "Summary")));

            return ContentShape(shapeHelper.Parts_VoteWidget_List(ContentPart: part, ContentItems: list));
        }

        protected override DriverResult Editor(VoteWidgetPart part, dynamic shapeHelper) {
            var viewModel = new VoteWidgetViewModel {
                Part = part, 
                ContentTypeNames = GetContentTypes(),
                FunctionNames = GetFunctionNames()
            };

            return ContentShape("Parts_Vote_Widget_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Vote.Widget", Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(VoteWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new VoteWidgetViewModel {
                Part = part
            };

            updater.TryUpdateModel(viewModel, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(VoteWidgetPart part, ImportContentContext context) {
            part.Dimension = context.Attribute(part.PartDefinition.Name, "Dimension");
            part.FunctionName = context.Attribute(part.PartDefinition.Name, "FunctionName");
            part.ContentType = context.Attribute(part.PartDefinition.Name, "ContentType");
            part.Count = Int32.Parse(context.Attribute(part.PartDefinition.Name, "Count"));
            part.Ascending = bool.Parse(context.Attribute(part.PartDefinition.Name, "Ascending"));
        }

        protected override void Exporting(VoteWidgetPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Dimension", part.Dimension);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FunctionName", part.FunctionName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ContentType", part.ContentType);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Count", part.Count);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Ascending", part.Ascending);
        }

        protected override void Cloning(VoteWidgetPart originalPart, VoteWidgetPart clonePart, CloneContentContext context) {
            clonePart.FunctionName = originalPart.FunctionName;
            clonePart.ContentType = originalPart.ContentType;
            clonePart.Count = originalPart.Count;
            clonePart.Ascending = originalPart.Ascending;
            clonePart.Dimension = originalPart.Dimension;
        }

        private IEnumerable<string> GetContentTypes() {
            return _contentDefinitionManager
                .ListTypeDefinitions()
                .Select(t => t.Name)
                .OrderBy(x => x);
        }


        private IEnumerable<string> GetFunctionNames() {
            return _functions.Select(f => f.Name).OrderBy(f => f);
        }
    }
}