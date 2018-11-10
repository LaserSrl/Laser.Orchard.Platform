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

using Orchard.Tags.Services;
using System.Web.Mvc;

namespace Laser.Orchard.UserProfiler.Projections {
    public class UserTrackingFilterForm : IFormProvider {
        private readonly Work<IResourceManager> _resourceManager;
        private readonly ITagService _tagService;
        private readonly IRepository<FilterRecord> _repositoryFilters;
        private readonly IOrchardServices _orchardServices;
        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }
        public const string FormName = "UserTrackingFilterForm";
        public UserTrackingFilterForm( ITagService tagService, IShapeFactory shapeFactory, Work<IResourceManager> resourceManager, IRepository<FilterRecord> repositoryFilters, IOrchardServices orchardServices) {
            _tagService = tagService;
            _resourceManager = resourceManager;
            _repositoryFilters = repositoryFilters;
            _orchardServices = orchardServices;
            _shapeFactory = shapeFactory;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {

            context.Form(FormName, shape => {
                var f = _shapeFactory.Form(
                    Id: FormName,

                     //_UserProfilling: _shapeFactory.FieldSet(
                     //       Id: "UserProfilling",
                            _UserProfilling: _shapeFactory.TextBox(
                            Name: "UserProfillingTag",
                            Title: T("Tag"),
                            Classes: new[] { "tokenized" }
                            )
                     //   )
                       ,
                

                    _UserProfillingTitle: _shapeFactory.Markup(
                        Value: "<span>" + T("Or").ToString() + "</span><br/><fieldset><legend>" + T("List of available tags") + ":</legend>"
                    ),

                    _UserProfillingList: _shapeFactory.SelectList(
                        Id: "UserProfillinglist",
                        Name: "UserProfillinglist"
 
                    )
                    //,

                    //_UserProfillingPanel: _shapeFactory.Markup(
                    //    Value: " </fieldset>"
                    //)

            

             
            
                );

                ////   _resourceManager.Value.Require("script", "ContentPicker");
                //_resourceManager.Value.Require("script", "jQueryUI");
                ////        _resourceManager.Value.Require("style", "content-picker-admin.css");

                ////     var reactionTypes = _reactionsService.GetTypesTableFiltered();
                var allTags = _tagService.GetTags();
                f._UserProfillingList.Add(new SelectListItem { Value = "", Text = T("none").ToString() });
         
                foreach (var item in allTags) {
                    f._UserProfillingList.Add(new SelectListItem { Value =  item.TagName, Text = item.TagName });
                }
                return f;
            });
        }
    }
}

//using Laser.Orchard.UserReactions.Services;


//namespace Laser.Orchard.UserReactions.Projections {
//    public class ReactionClickedFilterForm : IFormProvider {
//        private readonly IUserReactionsService _reactionsService;
//        private readonly Work<IResourceManager> _resourceManager;
//        private readonly IRepository<FilterRecord> _repositoryFilters;
//        private readonly IOrchardServices _orchardServices;
//        protected dynamic _shapeFactory { get; set; }
//        public Localizer T { get; set; }
//        public const string FormName = "ReactionClickedFilterForm";

//        public ReactionClickedFilterForm(IUserReactionsService reactionsService, IShapeFactory shapeFactory, Work<IResourceManager> resourceManager, IRepository<FilterRecord> repositoryFilters, IOrchardServices orchardServices) {
//            _reactionsService = reactionsService;
//            _resourceManager = resourceManager;
//            _repositoryFilters = repositoryFilters;
//            _orchardServices = orchardServices;
//            _shapeFactory = shapeFactory;
//            T = NullLocalizer.Instance;
//        }
      
//    }
//}