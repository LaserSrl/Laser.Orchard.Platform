using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Contrib.Voting.Filters {
    public class VoteCountFilterForm : VoteFilterForm {
        //public static string FormName = "VoteCountFilterForm";
        public VoteCountFilterForm(IShapeFactory shapeFactory, Work<IResourceManager> resourceManager) {
            Shape = shapeFactory;
            _resourceManager = resourceManager;
            T = NullLocalizer.Instance;
            FormName = "VoteCountFilterForm";
        }
    }
}