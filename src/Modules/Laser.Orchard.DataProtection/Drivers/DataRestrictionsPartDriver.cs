using Laser.Orchard.DataProtection.Models;
using Laser.Orchard.DataProtection.Security;
using Laser.Orchard.DataProtection.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.DataProtection.Drivers {
    public class DataRestrictionsPartDriver : ContentPartDriver<DataRestrictionsPart> {
        private readonly ITransactionManager _transactionManager;
        private readonly ITokenizer _tokenizer;
        private readonly IOrchardServices _orchardServices;
        public DataRestrictionsPartDriver(ITokenizer tokenizer, ITransactionManager transactionManager, IOrchardServices orchardServices) {
            _tokenizer = tokenizer;
            _transactionManager = transactionManager;
            _orchardServices = orchardServices;
        }
        protected override DriverResult Display(DataRestrictionsPart part, string displayType, dynamic shapeHelper) {
            return null;
        }
        protected override DriverResult Editor(DataRestrictionsPart part, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(DataProtectionPermissions.ManageDataProtection) == false) {
                return null;
            }
            var forceDefault = false;
            if (part.Settings.ContainsKey("DataRestrictionsPartSettings.ForceDefault")) {
                forceDefault = Convert.ToBoolean(part.Settings["DataRestrictionsPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            }
            if (forceDefault) {
                return null;
            }
            return ContentShape("Parts_DataRestrictionsPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/DataRestrictionsPart",
                    Model: new DataRestrictionsPartVM(part),
                    Prefix: Prefix));
        }
        protected override DriverResult Editor(DataRestrictionsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(DataProtectionPermissions.ManageDataProtection) == false) {
                return null;
            }
            DataRestrictionsPartVM vm = new DataRestrictionsPartVM(part);
            updater.TryUpdateModel(vm, Prefix, null, null);
            // applica i settings
            var restrictionsDefault = "";
            if (part.Settings.ContainsKey("DataRestrictionsPartSettings.RestrictionsDefault")) {
                restrictionsDefault = part.Settings["DataRestrictionsPartSettings.RestrictionsDefault"];
            }
            var forceDefault = false;
            if (part.Settings.ContainsKey("DataRestrictionsPartSettings.ForceDefault")) {
                forceDefault = Convert.ToBoolean(part.Settings["DataRestrictionsPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            }
            if (forceDefault) {
                vm.RestrictionsTextArea = _tokenizer.Replace(restrictionsDefault, part.ContentItem);
            } else {
                if (string.IsNullOrWhiteSpace(vm.RestrictionsTextArea)) {
                    vm.RestrictionsTextArea = _tokenizer.Replace(restrictionsDefault, part.ContentItem);
                }
            }

            // deserializza le data restrictions
            var session = _transactionManager.GetSession();
            var sep = new string[] { "\r", "\n", "\r\n" };
            var toDelete = new List<DataRestrictionsRecord>();
            toDelete.AddRange(part.Restrictions);
            part.Restrictions.Clear();
            foreach(var row in vm.RestrictionsTextArea.Split(sep, StringSplitOptions.RemoveEmptyEntries)) {
                var restrictions = toDelete.FirstOrDefault(x => x.DataRestrictionsPartRecord_id == part.Id && x.Restrictions == row);
                if(restrictions == null) {
                    // nuova data restriction
                    var record = new DataRestrictionsRecord() { Restrictions = row, DataRestrictionsPartRecord_id = part.Id };
                    session.Save(record);
                    part.Restrictions.Add(record);
                } else {
                    // data restriction invariata
                    part.Restrictions.Add(restrictions);
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}