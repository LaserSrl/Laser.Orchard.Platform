using Laser.Orchard.Mobile.Models;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.UI.Admin;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Mobile.Drivers {
    public class MobileContactPartDriver : ContentPartDriver<MobileContactPart> {
        private readonly IOrchardServices _orchardServices;

        public MobileContactPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        protected override string Prefix {
            get { return "Laser.Mobile.MobileContact"; }
        }

        protected override DriverResult Display(MobileContactPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Detail") {
                    List<PushNotificationRecord> viewModel = new List<PushNotificationRecord>();
                    if (part.MobileEntries != null && part.MobileEntries.Value != null && part.MobileEntries.Value.Count > 0)
                        viewModel = part.MobileEntries.Value.ToList();
                    return ContentShape("Parts_MobileContact",
                        () => shapeHelper.Parts_MobileContact(Devices: viewModel));
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        }
        protected override DriverResult Editor(MobileContactPart part, dynamic shapeHelper) {
            List<PushNotificationRecord> viewModel = new List<PushNotificationRecord>();
            if (part.MobileEntries != null && part.MobileEntries.Value != null && part.MobileEntries.Value.Count > 0)
                viewModel = part.MobileEntries.Value.ToList();
            return ContentShape("Parts_MobileContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobileContact_Edit", Model: viewModel, Prefix: Prefix));
        }
        // non ha senso esportare e importare i device perché il token è legato all'ambiente/sito
    }
}