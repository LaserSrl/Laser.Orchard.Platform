using Laser.Orchard.UserReactions.Services;
using Orchard;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Web;
using Orchard.Projections.Models;
using System.Xml;

namespace Laser.Orchard.UserReactions.Projections {
    public class ReactionClickedFilterForm : IFormProvider {
        private readonly IUserReactionsService _reactionsService;
        private readonly Work<IResourceManager> _resourceManager;
        private readonly IRepository<FilterRecord> _repositoryFilters;
        private readonly IOrchardServices _orchardServices;
        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }
        public const string FormName = "ReactionClickedFilterForm";

        public ReactionClickedFilterForm(IUserReactionsService reactionsService, IShapeFactory shapeFactory, Work<IResourceManager> resourceManager, IRepository<FilterRecord> repositoryFilters, IOrchardServices orchardServices) {
            _reactionsService = reactionsService;
            _resourceManager = resourceManager;
            _repositoryFilters = repositoryFilters;
            _orchardServices = orchardServices;
            _shapeFactory = shapeFactory;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            // recupera il valore salvato per il content picker field
            char[] separator = { ',' };
            List<ContentItem> contentItem = new List<ContentItem>();
            var httpContext = HttpContext.Current;
            if (httpContext != null) {
                var filterId = httpContext.Request["filterId"];
                if (string.IsNullOrEmpty(filterId) == false) {
                    var filter = _repositoryFilters.Get(Convert.ToInt32(filterId));
                    if (filter != null) {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(filter.State);
                        var node = doc.SelectSingleNode("Form/ContentId");
                        if (node != null) {
                            var arrIds = node.InnerText.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            if (arrIds.Length > 0) {
                                contentItem.Add(_orchardServices.ContentManager.Get(Convert.ToInt32(arrIds[0])));
                            }
                        }
                    }
                }
            }
            // compone il form
            context.Form("ReactionClickedFilterForm", shape => {
                var f = _shapeFactory.Form(
                    Id: "ReactionClickedFilterForm",

                     _Reaction: _shapeFactory.FieldSet(
                            Id: "reaction",
                            _Reaction: _shapeFactory.TextBox(
                            Name: "Reaction",
                            Title: T("Reaction type"),
                            Classes: new[] { "tokenized" }
                            )
                        ),

                    _ReactionTitle: _shapeFactory.Markup(
                        Value: "<fieldset><legend>" + T("List of available reactions") + ":</legend>"
                    ),

                    _ReactionsList: _shapeFactory.List(
                        Id: "reactionslist"
                    ),

                    _ReactionPanel: _shapeFactory.Markup(
                        Value: " </fieldset>"
                    ),

                    _FieldSetSingle: _shapeFactory.FieldSet(
                        Id: "fieldset-content-item",
                        _Value: _shapeFactory.ContentPicker_Edit(
                            Required: false,
                            Multiple: false,
                            DisplayName: "Content",
                            IdsFieldId: "contentId",
                            SelectedItemsFieldName: "ContentId",
                            ContentItems: contentItem,
                            Hint: "Select a content or populate the following field.",
                            PartName: "",
                            FieldName: ""
                        ),
                        _ValueTokenized: _shapeFactory.TextBox(
                            Id: "contentIdTokenized", Name: "ContentIdTokenized",
                            Title: T("Tokenized Content ID"),
                            Classes: new[] { "tokenized" }
                            )
                    )
                );

                _resourceManager.Value.Require("script", "ContentPicker");
                _resourceManager.Value.Require("script", "jQueryUI");
                _resourceManager.Value.Require("style", "content-picker-admin.css");

                var reactionTypes = _reactionsService.GetTypesTableFiltered();
                foreach (var item in reactionTypes) {
                    f._ReactionsList.Add(item.TypeName);
                }
                return f;
            });
        }
    }
}