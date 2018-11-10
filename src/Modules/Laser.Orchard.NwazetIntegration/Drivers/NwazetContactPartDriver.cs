using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NwazetIntegration.Models;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.UI.Admin;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class NwazetContactPartDriver : ContentPartDriver<NwazetContactPart> {
        private readonly IOrchardServices _orchardServices;

        public NwazetContactPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        protected override string Prefix
        {
            get { return "Laser.Mobile.NwazetContact"; }
        }
        protected override DriverResult Display(NwazetContactPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Detail") {
                    if (part.NwazetAddressRecord != null && part.NwazetAddressRecord.Count > 0)
                        return ContentShape("Parts_NwazetContact",
                            () => shapeHelper.Parts_NwazetContact(Address: part.NwazetAddressRecord,Email:part.ContentItem));
                }
            }
            return null;
        }
        protected override DriverResult Editor(NwazetContactPart part, dynamic shapeHelper) {
            if (part.NwazetAddressRecord != null && part.NwazetAddressRecord.Count > 0)
            return ContentShape("Parts_NwazetContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/NwazetContact_Edit", Model: part.NwazetAddressRecord, Prefix: Prefix));
            return null;
        }
        // non ha senso esportare e importare i device perché il token è legato all'ambiente/sito
    }
}