using Laser.Orchard.DataProtection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Laser.Orchard.DataProtection.ViewModels {
    public class DataRestrictionsPartVM {
        public DataRestrictionsPartVM(DataRestrictionsPart part) {
            DataRestrictions = part;
            var sb = new StringBuilder();
            foreach (var row in part.Restrictions) {
                sb.AppendLine(row.Restrictions);
            }
            RestrictionsTextArea = sb.ToString();
        }
        public DataRestrictionsPart DataRestrictions { get; set; }
        public string RestrictionsTextArea { get; set; }
    }
}