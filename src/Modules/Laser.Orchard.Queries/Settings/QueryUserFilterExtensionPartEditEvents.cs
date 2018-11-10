using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Projections.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.Queries.Settings {

    public class QueryUserFilterExtensionPartEditEvents : ContentDefinitionEditorEventsBase {
        private readonly IProjectionManager _projectionManager;

        public QueryUserFilterExtensionPartEditEvents(IProjectionManager projectionManager) {
            _projectionManager = projectionManager;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "QueryUserFilterExtensionPart") yield break;
            var model = definition.Settings.GetModel<QueryUserFilterExtensionPartSettingVM>();
            if (model.QueryUserFilter == null)
                model.QueryUserFilter = "";
            model.SelezionatiElenco = (model.QueryUserFilter).Split(',');
            List<string> filtriselezionati = new List<string>(model.SelezionatiElenco);
            var elencofiltri = _projectionManager.DescribeFilters().OrderByDescending(x => x.Category);
            List<SelectListItem> sl = new List<SelectListItem>();
            foreach (var filtro in elencofiltri) {
                sl.Insert(0, new SelectListItem { Value = filtro.Category, Text = filtro.Name.ToString(), Selected = filtriselezionati.Contains(filtro.Category) });
            }

            model.Elenco = new SelectList((IEnumerable<SelectListItem>)sl, "Value", "Text", model.QueryUserFilter);

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {
            if (builder.Name != "QueryUserFilterExtensionPart") yield break;
            var model = new QueryUserFilterExtensionPartSettingVM();
            updateModel.TryUpdateModel(model, "QueryUserFilterExtensionPartSettingVM", null, null);
            builder.WithSetting("QueryUserFilterExtensionPartSettingVM.QueryUserFilter", (string.Join(",", model.SelezionatiElenco)));
            yield return DefinitionTemplate(model);
        }
    }
}