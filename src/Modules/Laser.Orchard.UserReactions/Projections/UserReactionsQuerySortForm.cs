using Laser.Orchard.UserReactions.Services;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.UserReactions.Projections {
    public class UserReactionsQuerySortForm : IFormProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly IUserReactionsService _reactionsService;
        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }

        public UserReactionsQuerySortForm(IShapeFactory shapeFactory, IOrchardServices orchardServices, IUserReactionsService reactionsService) {
            _shapeFactory = shapeFactory;
            _orchardServices = orchardServices;
            _reactionsService = reactionsService;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            context.Form("ReactionsSortForm", shape => {
                var f = _shapeFactory.Form(
                    Id: "ReactionsSortForm",
                    _Reaction: _shapeFactory.SelectList(
                        Id: "reaction", Name: "Reaction",
                        Title: T("Reaction"),
                        Size: 1,
                        Multiple: false),
                    _Separator: _shapeFactory.Markup( // separatore
                        Value: "<br/>"),
                    _SortAsc: _shapeFactory.Radio(
                        Id: "sortAsc", Name: "Sort",
                        Title: T("Sort order ascending"),
                        Value: true),
                    _SortDesc: _shapeFactory.Radio(
                        Id: "sortDesc", Name: "Sort",
                        Title: T("Sort order descending"),
                        Value: false)
                );
                var reactionType = _reactionsService.GetTypesTableFiltered();

                foreach (var item in reactionType) {
                    f._Reaction.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.TypeName });
                }
                return f;
            });
        }
    }
}