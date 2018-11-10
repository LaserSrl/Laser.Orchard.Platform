using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Highlights.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;

namespace Laser.Orchard.Highlights.ViewModels {

    public class HighlightsItemViewModel {
        public HighlightsItemViewModel() {
            Title = "";
            TitleSize = "";
            Body = "";
            ItemOrder = 0;
            LinkTarget = Enums.LinkTargets._self;
            LinkUrl = "";
            GroupShapeName = "";
            LinkText = "";
            MediaUrl = "";
            Video = false;
        }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Body { get; set; }
        public string MediaUrl { get; set; }
        public string LinkUrl { get; set; }
        public string LinkText { get; set; }
        public string TitleSize { get; set; }
        public bool Video { get; set; }
        public Enums.LinkTargets LinkTarget { get; set; }
        public int ItemOrder { get; set; }
        public string GroupShapeName { get; set; }
        public string GroupDisplayPlugin { get; set; }
        public Enums.DisplayTemplate GroupDisplayTemplate { get; set; }
        public string DisplayType { get; set; }
        public IContent Content { get; set; }
    }
}