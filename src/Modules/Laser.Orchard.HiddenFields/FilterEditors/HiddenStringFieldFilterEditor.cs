using Laser.Orchard.HiddenFields.FilterEditors.Forms;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.FieldTypeEditors;
using Orchard.Projections.FilterEditors;
using Orchard.Projections.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.FilterEditors {
    public class HiddenStringFieldFilterEditor : IFilterEditor {

        public Localizer T { get; set; }

        public HiddenStringFieldFilterEditor() {
            T = NullLocalizer.Instance;
        }

        public bool CanHandle(Type type) {
            return new[] {
                typeof(char),
                typeof(string),
            }.Contains(type);
        }

        public string FormName {
            get { return HiddenStringFieldFilterForm.FormName; }
        }

        public Action<IHqlExpressionFactory> Filter(string property, dynamic formState) {
            return HiddenStringFieldFilterForm.GetFilterPredicate(formState, property);
        }

        public LocalizedString Display(string property, dynamic formState) {
            return HiddenStringFieldFilterForm.DisplayFilter(property, formState, T);
        }
    }

    
}