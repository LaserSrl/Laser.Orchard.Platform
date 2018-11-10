using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.ButtonToWorkflows.Models;
using Laser.Orchard.ButtonToWorkflows.Settings;
using Laser.Orchard.ButtonToWorkflows.Security;
using Orchard.Security.Permissions;
using Orchard;


namespace Laser.Orchard.ButtonToWorkflows.ViewModels {

    public class ButtonToWorkflowsVM {
        private readonly ButtonToWorkflowsPart _ButtonToWorkflowsPart;


        public ButtonToWorkflowsVM(ButtonToWorkflowsPart ButtonToWorkflowsPart) {

            _ButtonToWorkflowsPart = ButtonToWorkflowsPart;
            ElencoButtons = new List<ButtonToWorkflowsVMItem>();
            var settings = ButtonToWorkflowsPart.TypePartDefinition.Settings.GetModel<ButtonsSetting>();
            settings.ButtonNumber = settings.ButtonNumber[0].Split(',');
            foreach (var bw in settings.ButtonNumber) {
                ButtonToWorkflowsVMItem btw = new ButtonToWorkflowsVMItem();
                btw.ButtonNumber = Convert.ToInt32(bw);
                ElencoButtons.Add(btw);
            }
            this.ButtonDenied = ButtonToWorkflowsPart.ButtonsDenied;
            //this.ButtonNumber = settings.ButtonNumber.Split(',').Select(x => Convert.ToInt32(x)).ToList<Int32>();//(Convert.ToInt32(settings.ButtonNumber));

            //  IEnumerable<Permission> listPermission = ButtonPermissions.GetPermissions();
            //var permissionToTest=  listPermission[Convert.ToInt16(settings.ButtonNumber)];


        }
        public List<ButtonToWorkflowsVMItem> ElencoButtons { get; set; }
        public bool ButtonDenied { get; set; }
    }
    public class ButtonToWorkflowsVMItem {
        public string ButtonText { get; set; }
        public string ButtonAction { get; set; }
        public Int32 ButtonNumber { get; set; }
    }
}



