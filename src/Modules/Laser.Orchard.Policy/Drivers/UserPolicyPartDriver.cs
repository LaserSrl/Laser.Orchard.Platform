using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;

namespace Laser.Orchard.Policy.Drivers {
    public class UserPolicyPartDriver : ContentPartDriver<UserPolicyPart> {
        private const string CONTROLLER_ACTION = "account/register";
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private string currentControllerAction {
            get { //MVC 4
                return (_controllerContextAccessor.Context.RouteData.Values["controller"] + "/" + _controllerContextAccessor.Context.RouteData.Values["action"]).ToLowerInvariant();
            }
        }

        public UserPolicyPartDriver(IControllerContextAccessor controllerContextAccessor) {
            T = NullLocalizer.Instance;
            _controllerContextAccessor = controllerContextAccessor;

        }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "UserPolicy"; }
        }

        protected override DriverResult Editor(UserPolicyPart part, dynamic shapeHelper) {
            if (currentControllerAction == CONTROLLER_ACTION) return null; // nulla deve essere mostrato in fase di registrazione

            return ContentShape("Parts_UserPolicy_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/UserPolicy_Edit",
                                 Model: part,
                                 Prefix: Prefix));
        }
        protected override DriverResult Editor(UserPolicyPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (currentControllerAction == CONTROLLER_ACTION) return null;// nulla deve essere mostrato in fase di registrazione

            // non è necessario aggiornare il model perché le policy non sono editabili
            // ma forza il caricamento della property UserPolicyAnswers per evitare errori NHibernate sulla view
            // in caso ci siano altri errori di validazione
            part.UserPolicyAnswers.ToList();
            return Editor(part, shapeHelper);
        }


        //protected override void Exporting(UserPolicyPart part, ExportContentContext context) {

        //    if (part.UserPolicyAnswers != null) {

        //        //context.Element(part.PartDefinition.Name).SetAttributeValue("UserPolicyAnswers", part.UserPolicyAnswers);
        //        //var avPolAns = context.Element(part.PartDefinition.Name).Element("UserPolicyAnswers");

        //        foreach (UserPolicyAnswersRecord recPolicyAnswers in part.UserPolicyAnswers) {
        //            XElement avPolAns = new XElement("UserPolicyAnswers");
        //            avPolAns.SetAttributeValue("Id", recPolicyAnswers.Id);
        //            avPolAns.SetAttributeValue("AnswerDate", recPolicyAnswers.AnswerDate);
        //            avPolAns.SetAttributeValue("Accepted", recPolicyAnswers.Accepted);

        //            XElement policytextinfo = new XElement("PolicyTextInfoPartRecord");
        //            var a = recPolicyAnswers.PolicyTextInfoPartRecord;
        //            policytextinfo.SetAttributeValue("UserHaveToAccept", a.UserHaveToAccept);
        //            policytextinfo.SetAttributeValue("Priority", a.Priority);
        //            policytextinfo.SetAttributeValue("PolicyType", a.PolicyType);
        //        }
        //    }

        //}


        //protected override void Importing(UserPolicyPart part, ImportContentContext context) {
        //    var root = context.Data.Element(part.PartDefinition.Name);
        //    var importedUserPolicyAnswers = context.Attribute("UserPolicyAnswers", "UserPolicyAnswers");

        //    if (importedUserPolicyAnswers != null) {

        //        foreach (UserPolicyAnswersRecord rec in part.UserPolicyAnswers) {
                    
        //            if(Convert.ToInt32(root.Element("Id").Value)!=null)
        //                rec.Id = Convert.ToInt32(root.Element("Id").Value);

        //            if(Convert.ToDateTime(root.Element("AnswerDate").Value)!=null)
        //                rec.AnswerDate = Convert.ToDateTime(root.Element("AnswerDate").Value);

        //            if(Convert.ToBoolean(root.Element("Accepted").Value)!=null)
        //                rec.Accepted = Convert.ToBoolean(root.Element("Accepted").Value);

        //            if(Convert.ToBoolean(root.Element("PolicyTextInfoPartRecord").Parent.Element("UserHaveToAccept").Value)!=null)
        //                rec.PolicyTextInfoPartRecord.UserHaveToAccept = Convert.ToBoolean(root.Element("PolicyTextInfoPartRecord").Parent.Element("UserHaveToAccept").Value);
                    
        //            if(Convert.ToInt32(root.Element("PolicyTextInfoPartRecord").Parent.Element("Priority").Value)!=null)
        //                rec.PolicyTextInfoPartRecord.Priority = Convert.ToInt32(root.Element("PolicyTextInfoPartRecord").Parent.Element("Priority").Value);
                    
        //            if((PolicyTypeOptions)Enum.Parse(typeof(PolicyTypeOptions), root.Element("PolicyTextInfoPartRecord").Parent.Element("PolicyType").Value)!=null)
        //                rec.PolicyTextInfoPartRecord.PolicyType = (PolicyTypeOptions)Enum.Parse(typeof(PolicyTypeOptions), root.Element("PolicyTextInfoPartRecord").Parent.Element("PolicyType").Value);
        //        }
        //    }

        //}




    }

}


       

  