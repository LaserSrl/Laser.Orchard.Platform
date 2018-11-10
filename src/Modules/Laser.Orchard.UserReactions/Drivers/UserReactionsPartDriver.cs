using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard.Localization;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Collections;
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;
using Orchard.Data;


namespace Laser.Orchard.UserReactions.Drivers {
    public class UserReactionsPartDriver : ContentPartDriver<UserReactionsPart> {

        //richiamo il service per settaggio dati 
        //Definisci una variabile settata alla interfaccia
        private readonly IOrchardServices _orchardServices;
        private readonly IUserReactionsService _userReactionService;

        private readonly IRepository<UserReactionsTypesRecord> _repositoryTypesRecord;
        private readonly IRepository<UserReactionsSummaryRecord> _repositorySummaryRecord;

        //Crea nel costruttore il settaggio alla var che esegue una select (quando si istanzia la classe si settano i dati nel costruttore) 
        public UserReactionsPartDriver(IUserReactionsService userReactionService, IOrchardServices orchardServices,
            IRepository<UserReactionsTypesRecord> repositoryTypesRecord, IRepository<UserReactionsSummaryRecord> repositorySummaryRecord) {
            _userReactionService = userReactionService;
            _orchardServices = orchardServices;
            _repositoryTypesRecord = repositoryTypesRecord;
            _repositorySummaryRecord = repositorySummaryRecord;
        }

        public Localizer T { get; set; }


        
       

        //Evento display 
        protected override DriverResult Display(UserReactionsPart part, string displayType, dynamic shapeHelper) {

            IList<UserReactionsVM> viewmodel = null;           

            //Gestione visualizzazione amministratore
            if (displayType == "SummaryAdmin") {
                viewmodel = _userReactionService.GetTot(part);
                return ContentShape("Parts_UserReactions_SummaryAdmin", () => shapeHelper
                    .Parts_UserReactions_SummaryAdmin(UserReaction: viewmodel));
            }


            //Passare la view model da definire 
            if (displayType == "Detail") {
                viewmodel = _userReactionService.GetTot(part);
                bool authorized = _userReactionService.HasPermission(part.ContentItem.ContentType);
                bool requireLogin = false;
                if(authorized == false && _orchardServices.WorkContext.CurrentUser == null) {
                    requireLogin = true;
                }
                return ContentShape("Parts_UserReactions_Detail", () => shapeHelper
                    .Parts_UserReactions_Detail(UserReaction: viewmodel, Authorized: authorized, RequireLogin: requireLogin));
            }

            //Passare la view model da definire 
            if (displayType == "Summary") {
                viewmodel = _userReactionService.GetTot(part);
                return ContentShape("Parts_UserReactions_Summary", () => shapeHelper
                   .Parts_UserReactions_Summary(UserReaction: viewmodel));

            }

            return null;

        }


        ///// <summary>
        ///// GET Editor.
        ///// </summary>   
        //Evento Edit
        protected override DriverResult Editor(UserReactionsPart part, dynamic shapeHelper) {

            List<UserReactionsVM> viewmodel = _userReactionService.GetTot(part);
            UserReactionsTotalRec listUserView = new UserReactionsTotalRec();
            listUserView.UserReactionsTotals = viewmodel;
                return ContentShape("Parts_UserReactions_Edit", () => shapeHelper.EditorTemplate(
                                      TemplateName: "Parts/UserReactionsEdit",
                                      Model: listUserView,
                                      Prefix: Prefix));


        }



        //protected override void Importing(UserReactionsPart part, ImportContentContext context) {

        //    var root = context.Data.Element(part.PartDefinition.Name);
        //    var reactions = context.Data.Element(part.PartDefinition.Name).Elements("Reactions");

        //    //aggiorna il numero delle reactions sulla tabella di summary. Le reaction type sono aggiornate con import dei settings
        //    foreach (var reacts in reactions) 
        //    {
        //        var singleReact = new UserReactionsSummaryRecord();
        //        singleReact.Quantity = int.Parse(reacts.Attribute("Quantity").Value);         
                        
        //        var recType = reacts.Element("UserReactionsTypesRecord");
        //        if (recType != null) 
        //        {
        //            singleReact.UserReactionsTypesRecord = _repositoryTypesRecord.Get(tr => tr.TypeName == recType.Attribute("TypeName").Value);
        //         }
        //        _repositorySummaryRecord.Create(singleReact);
        //         part.Reactions.Add(singleReact);    
        //   }            
        //}


        //protected override void Exporting(UserReactionsPart part, ExportContentContext context) {
            
        //    var root = context.Element(part.PartDefinition.Name);

        //    if (part.Reactions.Count() > 0) {
        //        foreach (UserReactionsSummaryRecord receq in part.Reactions) 
        //        {
        //            XElement reactions = new XElement("Reactions");
        //            reactions.SetAttributeValue("Id", receq.Id);
        //            reactions.SetAttributeValue("Quantity", receq.Quantity);
        //            root.Add(reactions);

        //            XElement userReactionsTypesRecord = new XElement("UserReactionsTypesRecord");
        //            userReactionsTypesRecord.SetAttributeValue("Id", receq.UserReactionsTypesRecord.Id);
        //            userReactionsTypesRecord.SetAttributeValue("TypeName", receq.UserReactionsTypesRecord.TypeName);
        //            userReactionsTypesRecord.SetAttributeValue("Priority", receq.UserReactionsTypesRecord.Priority);
        //            userReactionsTypesRecord.SetAttributeValue("CssName", ((receq.UserReactionsTypesRecord.CssName != null) ? receq.UserReactionsTypesRecord.CssName : ""));
        //            userReactionsTypesRecord.SetAttributeValue("Activating", receq.UserReactionsTypesRecord.Activating);
                    
        //            reactions.Add(userReactionsTypesRecord);
                   
        //        }

        //    }

        //}


        
      

    }
}