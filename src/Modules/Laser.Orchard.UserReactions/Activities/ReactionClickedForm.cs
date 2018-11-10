using Laser.Orchard.UserReactions.Services;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.UserReactions.Activities {
    public class ReactionClickedForm : IFormProvider {
        private readonly IUserReactionsService _userReactionsService;
        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }

        public ReactionClickedForm(IShapeFactory shapeFactory, IUserReactionsService userReactionsService) {
            _shapeFactory = shapeFactory;
            _userReactionsService = userReactionsService;
        }

        public void Describe(DescribeContext context) {
            context.Form("TheFormReactionClicked", shape => {
                var f = _shapeFactory.Form(
                    Id: "TheFormReactionClicked",
                    _Type: _shapeFactory.FieldSet(
                        Title: T("Reactions")
                        //_reactionList: _shapeFactory.SelectList(
                        //    Id: "ReactionClickedActivity_reactionList",
                        //    Name: "ReactionClickedActivity_reactionList",
                        //    Title: T("Reaction list"),
                        //    Description: T("Select one or more reactions"),
                        //    Size: 10,
                        //    Multiple: true
                        //),
                    )
                );

                var elencoReactions = _userReactionsService.GetTypesTableFiltered();
                foreach (var reaction in elencoReactions) {
                    //f._Type._reactionList.Add(new SelectListItem {
                    //    Value = Convert.ToString(reaction.Id),
                    //    Text = reaction.TypeName,
                    //    Selected = false
                    //});

                    f._Type.Add(_shapeFactory.Checkbox(
                        Id: reaction.TypeName,
                        Name: "ReactionClickedActivity_reactionTypes",
                        Value: reaction.Id.ToString(),
                        Checked: false,
                        Title: reaction.TypeName
                    ));
                }
                f._Type.Add(_shapeFactory.InputHint(
                    Description: T("Select one or more reactions")
                ));
                return f;
            });
        }
    }
}