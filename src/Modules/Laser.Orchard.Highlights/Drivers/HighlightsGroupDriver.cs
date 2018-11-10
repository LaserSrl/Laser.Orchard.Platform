using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Laser.Orchard.Highlights.Models;
using Laser.Orchard.Highlights.Services;
using Laser.Orchard.Highlights.Settings;
using Laser.Orchard.Highlights.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.MediaLibrary.Fields;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc;
using Orchard.Mvc.Html;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;


namespace Laser.Orchard.Highlights.Drivers {

    public class HighlightsGroupDriver : ContentPartCloningDriver<HighlightsGroupPart> {

        private readonly IHighlightsService _HighlightsService;
        private readonly IOrchardServices _orchardServices;
        private readonly ICultureManager _cultureManager;
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContextAccessor _workContext;
        private readonly IShapeFactory _shapeFactory;
        private readonly IProjectionManager _projectionManager;


        public HighlightsGroupDriver(IHighlightsService HighlightsService, ICultureManager cultureManager, IContentManager contentManager, IProjectionManager projectionManager,
            ITransactionManager transactions, IHttpContextAccessor httpContextAccessor, IWorkContextAccessor workContext, IShapeFactory shapeFactory,
            IOrchardServices orchardServices) {
            _HighlightsService = HighlightsService;
            _contentManager = contentManager;
            _transactions = transactions;
            _cultureManager = cultureManager;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _shapeFactory = shapeFactory;
            _projectionManager = projectionManager;
            _orchardServices = orchardServices;
        }

        protected override string Prefix {
            get { return "HighlightsGroup"; }
        }

        /// <summary>
        /// Localizzazione
        /// </summary>
        public Localizer T {
            get;
            set;
        }

        protected override DriverResult Display(HighlightsGroupPart part, string displayType, dynamic shapeHelper) {

            ViewsInfos viewsInfos = _HighlightsService.ChooseView(part);
            var group = new HighlightsGroupViewModel {
                Id = part.Id,
                DisplayPlugin = part.DisplayPlugin,
                DisplayTemplate = part.DisplayTemplate,
                ItemsSourceType = part.ItemsSourceType,
            };

            IEnumerable<dynamic> items = null;
            IList<dynamic> fromQueryItems = new List<dynamic>();
            if (part.ItemsSourceType == Enums.ItemsSourceTypes.ByHand) {
                items = _HighlightsService.GetHighlightsItemsContentByGroupId(part.Id, part.DisplayTemplate, part.DisplayPlugin, viewsInfos.SuffixShapeName.Replace("_", ""));
            } else if (part.ItemsSourceType == Enums.ItemsSourceTypes.FromQuery) {
                var queryItems = _projectionManager.GetContentItems(part.Query_Id, 0, 10);
                fromQueryItems = _HighlightsService.MapContentToHighlightsItemsViewModel(queryItems, part.DisplayTemplate, part.DisplayPlugin, viewsInfos);
            }
            var dict = new Dictionary<string, object> { 
                { "HighlightsGroup", group },
                { "HighlightsItems", part.ItemsSourceType == Enums.ItemsSourceTypes.FromQuery?fromQueryItems:items }
            };
            var args = Arguments.From(dict.Values, dict.Keys);
            return ContentShape(viewsInfos.ResultShapeName,
                () => _shapeFactory.Create(viewsInfos.ResultShapeName, args));
            //return null;
        }


        protected override DriverResult Editor(HighlightsGroupPart part, dynamic shapeHelper) {

            var HighlightsItems = _HighlightsService.GetHighlightsItemsByGroupId(part.Id);
            // populating the list of queries and layouts
            var layouts = _projectionManager.DescribeLayouts().SelectMany(x => x.Descriptors).ToList();
            var queryRecordEntries = _orchardServices.ContentManager.Query<QueryPart>().Join<TitlePartRecord>().OrderBy(x => x.Title).List()
                .Select(x => new QueryRecordEntry {
                    Id = x.Id,
                    Name = x.Name,
                });

            var group = new HighlightsGroup {
                Id = part.Id,
                ContentId = (part.ContentItem != null) ? part.ContentItem.Id : 0,
                DisplayPlugin = part.DisplayPlugin,
                DisplayTemplate = part.DisplayTemplate,
                HighlightsItems = HighlightsItems.ToList(),
                HighlightsItemsOrder = String.Join(",", HighlightsItems.Select(s => s.Id).ToArray()),
                ItemsSourceType = part.ItemsSourceType,
                Query_Id = part.Query_Id,
                QueryRecordEntries = queryRecordEntries,
            };

            return ContentShape(
                  "Parts_HighlightsGroup_Edit",
                  () => shapeHelper.EditorTemplate(
                  TemplateName: "Parts/HighlightsGroup_Edit",
                  Model: group,
                  Prefix: Prefix));

        }


        protected override DriverResult Editor(HighlightsGroupPart part, IUpdateModel updater, dynamic shapeHelper) {

            var group = new HighlightsGroup();
            var httpContext = _httpContextAccessor.Current();
            var HighlightsItems = _HighlightsService.GetHighlightsItemsByGroupId(part.Id);
            var form = httpContext.Request.Form;

            int[] Ordinati = new int[form.Count];

            string Suffisso;
            int Riga = 0;

            foreach (string key in form.Keys) {
                if (key.StartsWith("HighlightsItems")) {
                    Suffisso = key.Substring(key.IndexOf("].") + 2);
                    switch (Suffisso) {
                        case "Position":
                            //Lista[Riga, 0] = Convert.ToInt32(form[key]);
                            break;

                        case "ItemId":
                            Ordinati[Riga++] = Convert.ToInt32(form[key]);
                            break;

                        default:
                            break;
                    }
                }
            }


            String Messaggio;
            Messaggio = "";
            if (Messaggio != "") {
                updater.AddModelError(this.Prefix, T(Messaggio));
            }

            Array.Resize<int>(ref Ordinati, Riga);

            if (updater.TryUpdateModel(group, Prefix, null, null)) {
                if (group.DisplayPlugin.StartsWith(group.DisplayTemplate.ToString() + " - ")) {
                    part.DisplayPlugin = group.DisplayPlugin;
                } else {
                    part.DisplayPlugin = "";
                }
                part.DisplayTemplate = group.DisplayTemplate;
                part.ItemsSourceType = group.ItemsSourceType;
                if (group.ItemsSourceType == Enums.ItemsSourceTypes.ByHand) {
                    if (group.HighlightsItemsOrder != null) {
                        for (var i = 0; i < Ordinati.Length; i++) {
                            _HighlightsService.UpdateOrder(Convert.ToInt32(Ordinati[i]), i);
                        }
                    }
                } else {
                    part.Query_Id = group.Query_Id;
                }

            } else {
                _transactions.Cancel();
            }
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(HighlightsGroupPart part, ExportContentContext context) {
            var container = _contentManager.Get(part.Record.Id);
            if (container != null) {
                var ciQuery = _contentManager.Get(part.Query_Id);
                var containerIdentity = _contentManager.GetItemMetadata(ciQuery).Identity.ToString();
                context.Element(part.PartDefinition.Name).SetAttributeValue("DisplayPlugin", part.DisplayPlugin);
                context.Element(part.PartDefinition.Name).SetAttributeValue("DisplayTemplate", part.DisplayTemplate);
                context.Element(part.PartDefinition.Name).SetAttributeValue("Query_Id", containerIdentity);
                context.Element(part.PartDefinition.Name).SetAttributeValue("ItemsSourceType", part.ItemsSourceType);
            }
        }
        protected override void Importing(HighlightsGroupPart part, ImportContentContext context) {
            var importedDisplayPlugin = context.Attribute(part.PartDefinition.Name, "DisplayPlugin");
            if (importedDisplayPlugin != null) {
                part.DisplayPlugin = importedDisplayPlugin;
            }
            
            var displayTemplate = Enums.DisplayTemplate.List;
            Enum.TryParse<Enums.DisplayTemplate>(context.Attribute(part.PartDefinition.Name, "DisplayTemplate"), out displayTemplate); 
                part.DisplayTemplate = displayTemplate;
            
            var itemsSourceType = Enums.ItemsSourceTypes.ByHand;
            Enum.TryParse<Enums.ItemsSourceTypes>(context.Attribute(part.PartDefinition.Name, "ItemsSourceType"), out itemsSourceType);
            part.ItemsSourceType = itemsSourceType;
            
            var queryIdentifier = context.Attribute(part.PartDefinition.Name, "Query_Id");
            if (queryIdentifier != null) {
                var ciQuery = _contentManager.ResolveIdentity(new ContentIdentity(queryIdentifier));
                if (ciQuery != null) {
                    part.Query_Id = ciQuery.Id;
        }
            }
        }

        protected override void Imported(HighlightsGroupPart part, ImportContentContext context) {

            var groupID = Convert.ToInt16(context.Attribute(part.PartDefinition.Name, "GroupId"));

            FinalizeImportAndClone(groupID, part);
        }

        protected override void Exported(HighlightsGroupPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("GroupId", part.Id.ToString());
        }

        private void FinalizeImportAndClone(int groupID, HighlightsGroupPart part) {
            _contentManager.Publish(part.ContentItem);

            var MediaViewerItems = _HighlightsService.GetHighlightsItemsByGroupId(groupID);


            foreach (var mediaViewerItem in MediaViewerItems) {
                var risposta = _orchardServices.ContentManager.Clone(mediaViewerItem.ContentItem);

                foreach (var parte in risposta.Parts) {
                    if (parte.PartDefinition.Name == "HighlightsItemPart") {
                        HighlightsItemPart elemento;
                        elemento = (HighlightsItemPart)parte;

                        elemento.HighlightsGroupPartRecordId = part.Id;

                    }
                }
            }
        }

        protected override void Cloning(HighlightsGroupPart originalPart, HighlightsGroupPart clonePart, CloneContentContext context) {
            clonePart.DisplayPlugin = originalPart.DisplayPlugin;
            clonePart.DisplayTemplate = originalPart.DisplayTemplate;
            clonePart.ItemsSourceType = originalPart.ItemsSourceType;
            clonePart.Query_Id = originalPart.Query_Id;
        }

        protected override void Cloned(HighlightsGroupPart originalPart, HighlightsGroupPart clonePart, CloneContentContext context) {
            FinalizeImportAndClone(originalPart.Id, clonePart);
        }
    }



}