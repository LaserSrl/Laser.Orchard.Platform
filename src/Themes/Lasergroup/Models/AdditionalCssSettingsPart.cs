using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lasergroup.Models {
    public class AdditionalCssSettingsPart : ContentPart {

        public virtual string AdditionalCss {
            get { return this.Retrieve(p => p.AdditionalCss) ?? ""; }
            set { this.Store(p => p.AdditionalCss, value ?? ""); }
        }

        public List<string> StyleSheetsPaths {
            get {
                if (string.IsNullOrWhiteSpace(AdditionalCss)) {
                    return new List<string>();
                }
                return AdditionalCss
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }
            set {
                AdditionalCss = value != null
                  ? string.Join(Environment.NewLine, value)
                  : "";
            }
        }
    }
}