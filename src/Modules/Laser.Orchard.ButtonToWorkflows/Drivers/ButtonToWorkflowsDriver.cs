using Laser.Orchard.ButtonToWorkflows.Models;
using Laser.Orchard.ButtonToWorkflows.Settings;
using Laser.Orchard.ButtonToWorkflows.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Scheduling.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ButtonToWorkflows.Drivers {

    public class ButtonToWorkflowsDriver : ContentPartDriver<ButtonToWorkflowsPart> {
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }
        private readonly IWorkflowManager _workflowManager;
        private readonly INotifier _notifier;
        private readonly IRepository<ScheduledTaskRecord> _repositoryScheduledTask;

        protected override string Prefix {
            get { return "Laser.Mobile.ButtonToWorkflows"; }
        }

        public ButtonToWorkflowsDriver(IOrchardServices orchardServices, IWorkflowManager workflowManager, INotifier notifier, IRepository<ScheduledTaskRecord> repositoryScheduledTask) {
            _orchardServices = orchardServices;
            _workflowManager = workflowManager;
            _notifier = notifier;
            _repositoryScheduledTask = repositoryScheduledTask;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(ButtonToWorkflowsPart part, dynamic shapeHelper) {
            var model = new ButtonToWorkflowsVM(part);
            var settings = part.TypePartDefinition.Settings.GetModel<ButtonsSetting>();
            settings.ButtonNumber = settings.ButtonNumber[0].Split(',');
            //    model.ButtonText = settings.ButtonText;
            ButtonToWorkflowsSettingsPart settingmodulo = _orchardServices.WorkContext.CurrentSite.As<ButtonToWorkflowsSettingsPart>();
            try {
                string[] elencoButtonsText = settingmodulo.ButtonsText.Split('£');
                string[] elencoButtonsAction = settingmodulo.ButtonsAction.Split('£');
                foreach (string intbutton in settings.ButtonNumber) {
                    model.ElencoButtons.Where(x => x.ButtonNumber == Convert.ToInt32(intbutton)).FirstOrDefault().ButtonAction = elencoButtonsAction[Convert.ToInt32(intbutton)];
                    model.ElencoButtons.Where(x => x.ButtonNumber == Convert.ToInt32(intbutton)).FirstOrDefault().ButtonText = elencoButtonsText[Convert.ToInt32(intbutton)];
                }
                //for (int i = 0; i < elencoButtonsText.Count(); i++) {
                //    if (elencoButtonsText[i] == model.ButtonText) {
                //        model.ButtonAction = elencoButtonsAction[i];
                //        model.ButtonText = settings.ButtonText;
                //    }
                //}
            }
            catch { }
            return ContentShape("Parts_ButtonToWorkflows", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ButtonToWorkflows", Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(ButtonToWorkflowsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var bigmodel = new ButtonToWorkflowsVM(part);
            if (updater.TryUpdateModel(bigmodel, Prefix, null, null))

                if (part.ContentItem.Id != 0) {
                    foreach (ButtonToWorkflowsVMItem model in bigmodel.ElencoButtons)
                        if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.CustomButton" + model.ButtonText) {
                            var content = _orchardServices.ContentManager.Get(part.ContentItem.Id, VersionOptions.Latest);
                            //_workflowManager.TriggerEvent("ButtonToWorkflowsSubmitted", content, () => new Dictionary<string, object> { { "Content", content } });
                            //   var settings = part.TypePartDefinition.Settings.GetModel<ButtonsSetting>();
                            //   settings.ButtonNumber = settings.ButtonNumber[0].Split(',');
                            ButtonToWorkflowsSettingsPart settingmodulo = _orchardServices.WorkContext.CurrentSite.As<ButtonToWorkflowsSettingsPart>();
                            string[] elencoButtonsMessage = settingmodulo.ButtonsMessage.Split('£');
                            string[] elencoButtonsActionAsync = settingmodulo.ButtonsAsync.Split('£');
                            part.ActionToExecute = model.ButtonAction + "_btn" + (model.ButtonNumber + 1).ToString();
                            //   _workflowManager.TriggerEvent(model.ButtonAction+"_btn"+(model.ButtonNumber+1).ToString(), content, () => new Dictionary<string, object> { { "Content", content } });
                            part.MessageToWrite = elencoButtonsMessage[model.ButtonNumber].ToString();
                           string valAsync= string.IsNullOrEmpty(elencoButtonsActionAsync[model.ButtonNumber]) ? "" : elencoButtonsActionAsync[model.ButtonNumber].ToLower();
                            part.ActionAsync = valAsync.Equals("true");
                        }
                }
                else {
                    updater.AddModelError("Error Saving Content Item", T("Error Saving Content Item"));
                }
            //  var viewModel = new ButtonToWorkflowsVM();
            return ContentShape("Parts_ButtonToWorkflows", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ButtonToWorkflows", Model: bigmodel, Prefix: Prefix));
        }

        protected override void Importing(ButtonToWorkflowsPart part, ImportContentContext context) {
            //throw new NotImplementedException();
            //TODO: Effettuare check su consistenza UserId

            //var root = context.Data.Element(part.PartDefinition.Name);
            //part.FromUser = root.Attribute("FromUser").Value;
            //part.ToUser = root.Attribute("ToUser").Value;
            //int nTempId;
            //if (int.TryParse(root.Attribute("FromIdUser").Value, out nTempId)) {
            //    part.FromIdUser = int.Parse(root.Attribute("FromIdUser").Value);
            //}
            //if (int.TryParse(root.Attribute("ToIdUser").Value, out nTempId)) {
            //    part.FromIdUser = int.Parse(root.Attribute("ToIdUser").Value);
            //}
            //part.ActionToExecute = root.Attribute("ActionToExecute").Value;
            //part.MessageToWrite = root.Attribute("MessageToWrite").Value;




            // Mod 30-11-2016 
            //////////var importedFromUser = context.Attribute(part.PartDefinition.Name, "FromUser");
            //////////if (importedFromUser != null) {
            //////////    part.FromUser = importedFromUser;
            //////////}

            //////////var importedToUser = context.Attribute(part.PartDefinition.Name, "ToUser");
            //////////if (importedToUser != null) {
            //////////    part.ToUser = importedToUser;
            //////////}

            //////////var importedFromIdUser = context.Attribute(part.PartDefinition.Name, "FromIdUser");
            //////////if (importedFromIdUser != null) {
            //////////    part.FromIdUser = Convert.ToInt32(importedFromIdUser);
            //////////}

            //////////var importedToIdUser = context.Attribute(part.PartDefinition.Name, "ToIdUser");
            //////////if (importedToIdUser != null) {
            //////////    part.ToIdUser = Convert.ToInt32(importedToIdUser);
            //////////}



            //var importedActionToExecute = context.Attribute(part.PartDefinition.Name, "ActionToExecute");
            //if (importedActionToExecute != null) {
            //    part.ActionToExecute = importedActionToExecute;
            //}

            //var importedMessageToWrite = context.Attribute(part.PartDefinition.Name, "MessageToWrite");
            //if (importedMessageToWrite != null) {
            //    part.MessageToWrite = importedMessageToWrite;
            //}

        }

        protected override void Exporting(ButtonToWorkflowsPart part, ExportContentContext context) {

            //var root = context.Element(part.PartDefinition.Name);
            //root.SetAttributeValue("FromUser", part.FromUser);
            //root.SetAttributeValue("ToUser", part.ToUser);
            //root.SetAttributeValue("FromIdUser", part.FromIdUser);
            //root.SetAttributeValue("ToIdUser", part.ToIdUser);
            //root.SetAttributeValue("ActionToExecute", part.ActionToExecute);
            //root.SetAttributeValue("MessageToWrite", part.MessageToWrite);
            //context.Element(part.PartDefinition.Name).SetAttributeValue("FromUser", part.FromUser);
            //context.Element(part.PartDefinition.Name).SetAttributeValue("ToUser", part.ToUser);
            //context.Element(part.PartDefinition.Name).SetAttributeValue("FromIdUser", part.FromIdUser);
            //context.Element(part.PartDefinition.Name).SetAttributeValue("ToIdUser", part.ToIdUser);
            //context.Element(part.PartDefinition.Name).SetAttributeValue("ActionToExecute", part.ActionToExecute);
            //context.Element(part.PartDefinition.Name).SetAttributeValue("MessageToWrite", part.MessageToWrite);
        }
    }
}