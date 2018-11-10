using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.UI.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.UserReactions.Projections {
    public class UserReactionsQueryFilterForm : IFormProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly IUserReactionsService _reactionsService;
        private readonly WorkContext _workContext;
        private readonly Work<IResourceManager> _resourceManager;

        public const string FormName = "ReactionsFilterForm";

        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }

        public UserReactionsQueryFilterForm(WorkContext workContext, IShapeFactory shapeFactory, 
                                            IOrchardServices orchardServices, 
                                            IUserReactionsService reactionsService,
                                            Work<IResourceManager> resourceManager) {
            _shapeFactory = shapeFactory;
            _orchardServices = orchardServices;
            _reactionsService = reactionsService;
            _workContext = workContext;
            T = NullLocalizer.Instance;
            _resourceManager = resourceManager;
        }

        public void Describe(DescribeContext context) {
            context.Form("ReactionsFilterForm", shape => {
                var f = _shapeFactory.Form(                    
                    Id: "ReactionsFilterForm",

                     _Reaction: _shapeFactory.FieldSet(
                        Id: "reaction",
                        _Reaction: _shapeFactory.TextBox(
                            Name: "Reaction",
                            Title: T("Reactions types"),
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

                    _Operator: _shapeFactory.SelectList(
                        Id: "operator", Name: "Operator",
                        Title: T("Operator"),
                        Size: 1,
                        Multiple: false
                    ),

                    _FieldSetSingle: _shapeFactory.FieldSet(
                        Id: "fieldset-single",
                        _Value: _shapeFactory.TextBox(
                            Id: "value", Name: "Value",
                            Title: T("Value"),
                            Classes: new[] { "tokenized" }
                        )
                    ),

                    _FieldSetMin: _shapeFactory.FieldSet(
                        Id: "fieldset-min",
                        _Min: _shapeFactory.TextBox(
                            Id: "min", Name: "Min",
                            Title: T("Min"),
                            Classes: new[] { "tokenized" }
                            )
                        ),

                    _FieldSetMax: _shapeFactory.FieldSet(
                        Id: "fieldset-max",
                        _Max: _shapeFactory.TextBox(
                            Id: "max", Name: "Max",
                            Title: T("Max"),
                            Classes: new[] { "tokenized" }
                            )
                        )
                );

                var reactionTypes = _reactionsService.GetTypesTableFiltered();
                foreach (var item in reactionTypes) {
                    f._ReactionsList.Add(item.TypeName);
                }

                _resourceManager.Value.Include("script", "~/Modules/Orchard.Projections/Scripts/numeric-editor-filter.js", "~/Modules/Orchard.Projections/Scripts/numeric-editor-filter.js");

                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.Equals), Text = T("Is equal to").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.NotEquals), Text = T("Is not equal to").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.Between), Text = T("Is between to").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.GreaterThan), Text = T("Is greater than").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.GreaterThanEquals), Text = T("Is greater than or equal").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.LessThan), Text = T("Is less than").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.LessThanEquals), Text = T("Is less than or equal").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.NotBetween), Text = T("Not between").Text });
                return f;
            });
        }
    }
}