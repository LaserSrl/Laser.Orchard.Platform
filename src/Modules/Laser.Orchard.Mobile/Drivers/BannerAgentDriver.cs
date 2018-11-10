using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Drivers {

    [OrchardFeature("Laser.Orchard.BannerAgent")]
    public class BannerAgentDriver : ContentPartCloningDriver<BannerAgentPart> {
        protected override string Prefix {
            get
            {
                return "BannerAgent";
            }
        }
        protected override DriverResult Display(BannerAgentPart part, string displayType, dynamic shapeHelper) {
                return ContentShape("Parts_BannerAgent",
                    () => shapeHelper.Parts_BannerAgent(
                        Part: part
                        ));
        }
        protected override DriverResult Editor(BannerAgentPart part, dynamic shapeHelper) {

             return ContentShape("Parts_BannerAgent_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/BannerAgent_Edit",
                    Model: part,
                    Prefix: Prefix
                    ));
        }
        protected override DriverResult Editor(BannerAgentPart part, IUpdateModel updater, dynamic shapeHelper) {
            //var editModel = _userAgentRedirectServices.BuildEditModelForUserAgentRedirectPart(part);
            if (updater != null) {
                if (updater.TryUpdateModel(part, Prefix, null, null)) {
                    //if (part.ContentItem.Id != 0) {
                    //    // se per caso part.Id è diversa dall'Id registrato nel record relazionato, arrivo da una traduzione, quindi devo trattare tutto come se fosse un nuovo inserimento
                    //    foreach (var q in editModel.Stores) {
                    //        if (part.Id != q.UserAgentRedirectPartRecord_Id) {
                    //            q.UserAgentRedirectPartRecord_Id = part.Id;
                    //            q.Id = 0;
                    //        }
                    //    }
                    //    _userAgentRedirectServices.Update(part.ContentItem, editModel);
                    //}
                }
            }
            return Editor(part, shapeHelper);
            
            
        }

        //#region [ Import/Export ]
        //protected override void Exporting(UserAgentRedirectPart part, ExportContentContext context) {
        //    var root = context.Element(part.PartDefinition.Name);
        //    root.SetAttributeValue("AppName", part.AppName);
        //    root.SetAttributeValue("AutoRedirect", part.AutoRedirect);
        //    foreach (var q in part.Stores) {
        //        XElement store = new XElement("Stores");
        //        store.SetAttributeValue("AppStoreKey", q.AppStoreKey);
        //        store.SetAttributeValue("RedirectUrl", q.RedirectUrl);
        //        root.Add(store);
        //    }
        //}

        //protected override void Importing(UserAgentRedirectPart part, ImportContentContext context) {
        //    var root = context.Data.Element(part.PartDefinition.Name);
        //    var stores = context.Data.Element(part.PartDefinition.Name).Elements("Stores");
        //    var editModel = _userAgentRedirectServices.BuildEditModelForUserAgentRedirectPart(part);
        //    editModel.AutoRedirect = bool.Parse(root.Attribute("AutoRedirect").Value);
        //    editModel.AppName = root.Attribute("AppName").Value;
        //    var storeList = new List<AppStoreEdit>();
        //    foreach (var q in stores) {
        //        var appStoreEditModel = new AppStoreEdit {
        //            AppStoreKey = (MobileAppStores)Enum.Parse(typeof(MobileAppStores), q.Attribute("AppStoreKey").Value),
        //            RedirectUrl = q.Attribute("RedirectUrl").Value,
        //        };
        //        storeList.Add(appStoreEditModel);
        //    }
        //    editModel.Stores = storeList; // metto tutto nel model 
        //    _userAgentRedirectServices.Update(
        //            part.ContentItem, editModel); //aggiorno
        //}
        //#endregion

        //protected override void Cloning(UserAgentRedirectPart originalPart, UserAgentRedirectPart clonePart, CloneContentContext context) {
        //    clonePart.AutoRedirect = originalPart.AutoRedirect;
        //    clonePart.AppName = originalPart.AppName;
        //    foreach (var record in originalPart.Stores) {
        //        //clone the records
        //        var newRecord = new AppStoreRedirectRecord() {
        //            UserAgentRedirectPartRecord = clonePart.Record,
        //            AppStoreKey = record.AppStoreKey,
        //            RedirectUrl = record.RedirectUrl
        //        };
        //        _appStoreRedirectRepository.Create(newRecord);
        //    }
        //}
    }
}