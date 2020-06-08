using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.Queries.Models;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.Queries.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using System.Xml.Linq;

namespace Laser.Orchard.Queries.Drivers {
    public class QueryPickerPartDriver : ContentPartCloningDriver<QueryPickerPart> {
        private readonly IQueryPickerService _queryPickerService;
        private readonly IRepository<QueryPartRecord> _queryRepository;
        private readonly IProjectionManager _projectionManager;
        private readonly IContentManager _contentManager;

        public QueryPickerPartDriver(IQueryPickerService queryPickerService, IRepository<QueryPartRecord> queryRepository, IProjectionManager projectionManager, IContentManager contentManager) {
            _queryPickerService = queryPickerService;
            _queryRepository = queryRepository;
            _projectionManager = projectionManager;
            _contentManager = contentManager;
        }

        protected override string Prefix {
            get {
                return "QueryPickerPart";
            }
        }
        protected override DriverResult Editor(QueryPickerPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(QueryPickerPart part, IUpdateModel updater, dynamic shapeHelper) {

            var settings = part.Settings.GetModel<QueryPickerPartSettings>();
            IEnumerable<KeyValuePair<int, string>> queryList;
                IEnumerable<KeyValuePair<int, string>> oneShotList;
            if (settings.IsForHqlQueries) {
                queryList = _queryPickerService.GetHqlQueries().Select(x =>
                        new KeyValuePair<int, string>(x.Id, ((dynamic)x).TitlePart.Title));
                oneShotList = new List<KeyValuePair<int, string>>();
            }
            else {
                queryList = _queryPickerService.GetUserDefinedQueries().Select(x =>
                        new KeyValuePair<int, string>(x.Id, ((dynamic)x).TitlePart.Title));
                oneShotList = _queryPickerService.GetOneShotQueries().Select(x =>
                        new KeyValuePair<int, string>(x.Id, ((dynamic)x).TitlePart.Title));
            }

            var model = new QueryPickerVM {
                SelectedIds = part.Ids,
                AvailableQueries = queryList,
                OneShotQueries = oneShotList
            };
            if (updater != null && updater.TryUpdateModel(model, Prefix, null, null)) {
                if (HttpContext.Current.Request.Form[Prefix + ".SelectedIds"] == null) {
                    part.Ids = new int[] {};
                } else {
                    part.Ids = model.SelectedIds;
                }
            }
            var resultRecordNumber = 0;
            if (!settings.IsForHqlQueries) {
                // TODO: rendere dinamico e injettabile l'array dei contenttypes
                var combinedQueries = _queryPickerService.GetCombinedContentQuery(model.SelectedIds, null, null);
                resultRecordNumber = combinedQueries.Count();
                model.TotalItemsCount = resultRecordNumber;
            }

            return ContentShape("Parts_QueryPicker_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/QueryPicker_Edit",
                        Model: model,
                        Prefix: Prefix));
        }

        protected override void Cloning(QueryPickerPart originalPart, QueryPickerPart clonePart, CloneContentContext context) {
            clonePart.Ids = originalPart.Ids;
        }


        protected override void Importing(QueryPickerPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            List<int> idList = new List<int>();
            var queryList = root.Elements("QueryId");
            foreach (var element in queryList) {
                var ciQuery = _contentManager.ResolveIdentity(new ContentIdentity(element.Value));
                if (ciQuery != null) {
                    idList.Add(ciQuery.Id);
                }
            }
            part.Ids = idList.ToArray();
        }

        protected override void Exporting(QueryPickerPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            if (part.Ids != null) {
                foreach (var qId in part.Ids) {
                    var ciQuery = _contentManager.Get(qId);
                    if(ciQuery != null) {
                        var identity =_contentManager.GetItemMetadata(ciQuery).Identity.ToString();
                        root.Add(new XElement("QueryId", identity));
                }
            }
        }
    }
    }
}